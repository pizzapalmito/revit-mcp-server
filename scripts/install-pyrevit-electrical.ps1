<#
.SYNOPSIS
Stages the pinned pySC Electrical MEP tools in a selected pyRevit extension root.

.DESCRIPTION
Each saved profile records the intended Revit versions and the extension root.
The script never replaces an existing installed extension unless -Force is given.
After staging, add the reported extension root in pyRevit Settings > Custom Extension Directories
for the Revit session that should use that profile, then reload pyRevit.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ExtensionRoot,

    [ValidateNotNullOrEmpty()]
    [string]$ProfileName = "default",

    [ValidateSet("2025", "2026")]
    [string[]]$RevitVersion = @("2025", "2026"),

    [switch]$Force
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $repoRoot "third_party\pySC"
$extensionName = "RevitMCP_Electrical.extension"

if (-not (Test-Path -LiteralPath (Join-Path $sourceRoot "pySC.tab"))) {
    throw "pySC is not available. Run 'git submodule update --init --recursive' from the Revit MCP repository first."
}

$resolvedRoot = [System.IO.Path]::GetFullPath($ExtensionRoot)
if (-not (Test-Path -LiteralPath $resolvedRoot)) {
    New-Item -ItemType Directory -Path $resolvedRoot -Force | Out-Null
}

$targetExtension = Join-Path $resolvedRoot $extensionName
if (Test-Path -LiteralPath $targetExtension) {
    if (-not $Force) {
        throw "'$targetExtension' already exists. It was not changed. Re-run with -Force only after reviewing or backing up local changes."
    }
    Remove-Item -LiteralPath $targetExtension -Recurse -Force
}

New-Item -ItemType Directory -Path $targetExtension | Out-Null
Copy-Item -LiteralPath (Join-Path $sourceRoot "pySC.tab") -Destination $targetExtension -Recurse
Copy-Item -LiteralPath (Join-Path $sourceRoot "lib") -Destination $targetExtension -Recurse
Copy-Item -LiteralPath (Join-Path $sourceRoot "LICENSE") -Destination (Join-Path $targetExtension "pySC-LICENSE")

$profileDirectory = Join-Path $env:APPDATA "RevitMCP"
$profilePath = Join-Path $profileDirectory "pyrevit-extension-profiles.json"
if (-not (Test-Path -LiteralPath $profileDirectory)) {
    New-Item -ItemType Directory -Path $profileDirectory -Force | Out-Null
}

$profiles = @{}
if (Test-Path -LiteralPath $profilePath) {
    $existing = Get-Content -LiteralPath $profilePath -Raw | ConvertFrom-Json
    if ($existing.profiles) {
        foreach ($property in $existing.profiles.PSObject.Properties) {
            $profiles[$property.Name] = $property.Value
        }
    }
}

$profiles[$ProfileName] = [ordered]@{
    extensionRoot = $resolvedRoot
    extensionPath = $targetExtension
    revitVersions = @($RevitVersion)
    installedAtUtc = [DateTime]::UtcNow.ToString("o")
    pyScCommit = (git -C $sourceRoot rev-parse HEAD).Trim()
}

[ordered]@{
    schemaVersion = 1
    profiles = $profiles
} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $profilePath -Encoding UTF8

Write-Host "Installed $extensionName for profile '$ProfileName'." -ForegroundColor Green
Write-Host "Extension root: $resolvedRoot"
Write-Host "Revit targets: $($RevitVersion -join ', ')"
Write-Host "Profile file: $profilePath"
Write-Host "In each intended Revit session, add the extension root in pyRevit Settings > Custom Extension Directories, then reload pyRevit."
