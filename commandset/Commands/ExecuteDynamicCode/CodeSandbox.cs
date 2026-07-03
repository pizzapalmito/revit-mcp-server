using System;
using System.Linq;

namespace RevitMCPCommandSet.Commands.ExecuteDynamicCode
{
    public static class CodeSandbox
    {
        private static readonly string[] ProhibitedPatterns =
        {
            "System.IO",
            "System.Net",
            "System.Diagnostics.Process",
            "Microsoft.Win32",
            "System.Reflection.Emit",
            "System.Runtime.InteropServices"
        };

        public static CodeSandboxValidationResult Validate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return CodeSandboxValidationResult.Allowed();

            var normalizedCode = Normalize(code);
            foreach (var pattern in ProhibitedPatterns)
            {
                var normalizedPattern = Normalize(pattern);
                if (normalizedCode.IndexOf(normalizedPattern, StringComparison.Ordinal) >= 0)
                {
                    return CodeSandboxValidationResult.Blocked(
                        $"Code execution blocked: prohibited namespace or API pattern '{pattern}' is not allowed.");
                }
            }

            return CodeSandboxValidationResult.Allowed();
        }

        private static string Normalize(string value)
        {
            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }
    }

    public class CodeSandboxValidationResult
    {
        public bool IsAllowed { get; }
        public string Message { get; }

        private CodeSandboxValidationResult(bool isAllowed, string message)
        {
            IsAllowed = isAllowed;
            Message = message;
        }

        public static CodeSandboxValidationResult Allowed()
        {
            return new CodeSandboxValidationResult(true, string.Empty);
        }

        public static CodeSandboxValidationResult Blocked(string message)
        {
            return new CodeSandboxValidationResult(false, message);
        }
    }
}
