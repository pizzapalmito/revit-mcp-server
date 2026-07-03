using RevitMCPCommandSet.Commands.ExecuteDynamicCode;
using TUnit.Core;

namespace RevitMCPCommandSet.Tests.ExecuteDynamicCode;

public class CodeSandboxTests
{
    [Test]
    public async Task Validate_AllowsSafeRevitCode()
    {
        var result = CodeSandbox.Validate("var count = new FilteredElementCollector(document).OfClass(typeof(Wall)).GetElementCount(); return count;");

        await Assert.That(result.IsAllowed).IsTrue();
        await Assert.That(result.Message).IsEqualTo("");
    }

    [Test]
    [Arguments("using System.IO; return 1;", "System.IO")]
    [Arguments("var text = System.IO.File.ReadAllText(\"C:\\\\temp\\\\x.txt\"); return text;", "System.IO")]
    [Arguments("System.Net.WebRequest.Create(\"https://example.com\"); return 1;", "System.Net")]
    [Arguments("System.Diagnostics.Process.Start(\"cmd.exe\"); return 1;", "System.Diagnostics.Process")]
    [Arguments("Microsoft.Win32.Registry.CurrentUser.ToString(); return 1;", "Microsoft.Win32")]
    [Arguments("System.Reflection.Emit.AssemblyBuilderAccess.Run.ToString(); return 1;", "System.Reflection.Emit")]
    [Arguments("System.Runtime.InteropServices.Marshal.SizeOf(typeof(int)); return 1;", "System.Runtime.InteropServices")]
    public async Task Validate_BlocksProhibitedNamespaces(string code, string blockedPattern)
    {
        var result = CodeSandbox.Validate(code);

        await Assert.That(result.IsAllowed).IsFalse();
        await Assert.That(result.Message).Contains(blockedPattern);
    }
}
