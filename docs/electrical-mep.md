# Electrical MEP foundation

This fork keeps [pySC](https://github.com/schauh11/pySC) as a pinned Git submodule in `third_party/pySC`. Its Electrical tab supplies the initial pyRevit-facing capabilities: circuits, panel schedules and phase balance, conduit operations, voltage drop/conduit sizing, and fire-alarm coverage checks.

The MCP and pyRevit interfaces are deliberately separated at this stage. pySC operates directly through the Revit API inside pyRevit; MCP operations run through the authenticated local Revit plug-in. This avoids opening a second unauthenticated bridge into Revit.

## Session-aware installation

Stage the extension into the pyRevit custom-extension root selected for a session:

```powershell
.\scripts\install-pyrevit-electrical.ps1 -ExtensionRoot "D:\RevitProfiles\Project-A\Extensions" -ProfileName "Project-A" -RevitVersion 2025,2026
```

The script saves each profile under `%APPDATA%\RevitMCP\pyrevit-extension-profiles.json`, including the extension path, target Revit versions, and exact pySC commit. It refuses to overwrite an existing installation unless `-Force` is explicitly supplied.

For the chosen Revit session, add that profile's extension root in **pyRevit Settings → Custom Extension Directories**, then reload pyRevit. This lets different projects or sessions select different extension roots without silently changing another session's tools.

## Operating rules

- Start with inspection and reporting tools; review affected elements before a modifying tool.
- Treat NEC 2023 calculator outputs as engineering assistance, not a sealed code-compliance determination.
- Use the built-in Revit panel or a configured MCP client for model changes. The built-in panel reads the live session endpoint and authentication token; it does not use a fixed port.
- Update the pySC submodule only through a reviewed commit, then restage the selected profile.
