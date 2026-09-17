using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using RevitMCPCommandSet.Models.Common;
using RevitMCPSDK.API.Interfaces;

namespace RevitMCPCommandSet.Services.Electrical
{
    /// <summary>Collects bounded, read-only MEP electrical inventory data on Revit's API thread.</summary>
    public class GetElectricalModelSummaryEventHandler : IExternalEventHandler, IWaitableExternalEventHandler
    {
        private readonly ManualResetEvent _resetEvent = new ManualResetEvent(false);
        private bool _includeSelectedElements = true;

        public AIResult<object> Result { get; private set; }

        public void Configure(bool includeSelectedElements)
        {
            _includeSelectedElements = includeSelectedElements;
            _resetEvent.Reset();
        }

        public bool WaitForCompletion(int timeoutMilliseconds = 10000)
        {
            return _resetEvent.WaitOne(timeoutMilliseconds);
        }

        public void Execute(UIApplication app)
        {
            try
            {
                var uiDocument = app.ActiveUIDocument;
                var document = uiDocument.Document;
                var circuits = new FilteredElementCollector(document)
                    .OfClass(typeof(ElectricalSystem))
                    .Cast<ElectricalSystem>()
                    .ToList();

                var selected = _includeSelectedElements
                    ? uiDocument.Selection.GetElementIds()
                        .Select(document.GetElement)
                        .Where(IsElectricalElement)
                        .Select(element => new
                        {
#if REVIT2024_OR_GREATER
                            id = element.Id.Value,
#else
                            id = element.Id.IntegerValue,
#endif
                            name = element.Name,
                            category = element.Category?.Name ?? string.Empty
                        })
                        .ToList()
                    : null;

                var summary = new Dictionary<string, object>
                {
                    ["electricalEquipment"] = CountCategory(document, BuiltInCategory.OST_ElectricalEquipment),
                    ["electricalFixtures"] = CountCategory(document, BuiltInCategory.OST_ElectricalFixtures),
                    ["lightingFixtures"] = CountCategory(document, BuiltInCategory.OST_LightingFixtures),
                    ["conduits"] = CountCategory(document, BuiltInCategory.OST_Conduit),
                    ["conduitFittings"] = CountCategory(document, BuiltInCategory.OST_ConduitFitting),
                    ["electricalCircuits"] = circuits.Count,
                    ["circuitsWithoutPanel"] = circuits.Count(circuit => circuit.BaseEquipment == null),
                    ["activeView"] = document.ActiveView?.Name ?? string.Empty,
                    ["selectedElectricalElements"] = selected,
                    ["readOnly"] = true
                };

                Result = new AIResult<object>
                {
                    Success = true,
                    Message = "Electrical model summary retrieved. Review this scope before requesting a modifying electrical operation.",
                    Response = summary
                };
            }
            catch (Exception ex)
            {
                Result = new AIResult<object>
                {
                    Success = false,
                    Message = $"Failed to retrieve electrical model summary: {ex.Message}"
                };
            }
            finally
            {
                _resetEvent.Set();
            }
        }

        public string GetName() => "Get Electrical Model Summary";

        private static int CountCategory(Document document, BuiltInCategory category)
        {
            return new FilteredElementCollector(document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements()
                .Count;
        }

        private static bool IsElectricalElement(Element element)
        {
            if (element?.Category == null)
                return false;

            var categoryId = element.Category.Id.IntegerValue;
            return categoryId == (int)BuiltInCategory.OST_ElectricalEquipment
                || categoryId == (int)BuiltInCategory.OST_ElectricalFixtures
                || categoryId == (int)BuiltInCategory.OST_LightingFixtures
                || categoryId == (int)BuiltInCategory.OST_Conduit
                || categoryId == (int)BuiltInCategory.OST_ConduitFitting;
        }
    }
}
