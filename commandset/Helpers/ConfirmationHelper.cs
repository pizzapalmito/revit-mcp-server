using Autodesk.Revit.UI;

namespace RevitMCPCommandSet.Helpers
{
    public static class ConfirmationHelper
    {
        /// <summary>
        /// Shows a native Revit TaskDialog asking the user to confirm a destructive operation.
        /// Returns true if the user clicks Yes, false otherwise.
        /// </summary>
        public static bool Confirm(string action, int elementCount)
        {
            if (elementCount <= 0) return true;

            var dialog = new TaskDialog("MCP Operation Confirmation")
            {
                MainContent = $"About to {action} {elementCount} element(s). Continue?",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.No
            };

            return dialog.Show() == TaskDialogResult.Yes;
        }

        /// <summary>
        /// Requires an explicit in-product acknowledgement before executing
        /// arbitrary code supplied by an MCP client.
        /// </summary>
        public static bool ConfirmAiCodeExecution()
        {
            var dialog = new TaskDialog("MCP Security Confirmation")
            {
                MainInstruction = "Run AI-generated C# code?",
                MainContent = "This code runs with your Revit and Windows permissions. " +
                              "It can modify the model and access files available to your account.",
                CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                DefaultButton = TaskDialogResult.No
            };

            return dialog.Show() == TaskDialogResult.Yes;
        }
    }
}
