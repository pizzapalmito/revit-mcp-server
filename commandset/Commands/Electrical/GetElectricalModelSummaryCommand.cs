using Autodesk.Revit.UI;
using Newtonsoft.Json.Linq;
using RevitMCPCommandSet.Services.Electrical;
using RevitMCPSDK.API.Base;

namespace RevitMCPCommandSet.Commands.Electrical
{
    /// <summary>Read-only electrical discovery command used before electrical changes.</summary>
    public class GetElectricalModelSummaryCommand : ExternalEventCommandBase
    {
        private static readonly object ExecutionLock = new object();
        private GetElectricalModelSummaryEventHandler SummaryHandler => (GetElectricalModelSummaryEventHandler)Handler;

        public override string CommandName => "get_electrical_model_summary";

        public GetElectricalModelSummaryCommand(UIApplication uiApp)
            : base(new GetElectricalModelSummaryEventHandler(), uiApp)
        {
        }

        public override object Execute(JObject parameters, string requestId)
        {
            lock (ExecutionLock)
            {
                try
                {
                    SummaryHandler.Configure(parameters?["includeSelectedElements"]?.Value<bool>() ?? true);
                    if (RaiseAndWaitForCompletion(15000))
                        return SummaryHandler.Result;

                    throw new TimeoutException("Electrical model summary timed out");
                }
                catch (Exception ex)
                {
                    throw new Exception($"Get electrical model summary failed: {ex.Message}");
                }
            }
        }
    }
}
