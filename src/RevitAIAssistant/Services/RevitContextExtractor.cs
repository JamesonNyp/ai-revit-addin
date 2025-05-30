using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.Services
{
    /// <summary>
    /// Extracts engineering context from the current Revit session
    /// Provides rich contextual information for AI processing
    /// </summary>
    public class RevitContextExtractor
    {
        private readonly ILogger<RevitContextExtractor> _logger;
        private readonly SessionManager _sessionManager;

        public RevitContextExtractor(ILogger<RevitContextExtractor> logger, SessionManager sessionManager)
        {
            _logger = logger;
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Extract comprehensive engineering context from current Revit state
        /// </summary>
        public async Task<EngineeringContext> GetCurrentContextAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var document = _sessionManager.CurrentDocument;
                    var uiDocument = _sessionManager.CurrentUIDocument;
                    
                    if (document == null)
                    {
                        _logger.LogWarning("No active document found");
                        return new EngineeringContext();
                    }

                    var context = new EngineeringContext
                    {
                        ProjectInfo = ExtractProjectInfo(document),
                        SelectedElements = ExtractSelectedElements(uiDocument),
                        ActiveView = ExtractActiveViewInfo(uiDocument),
                        VisibleSystems = ExtractVisibleSystems(document, uiDocument.ActiveView),
                        CurrentEngineer = GetCurrentEngineer(),
                        ApplicableCodes = GetApplicableCodes(document),
                        ProjectName = document.Title,
                        Discipline = DetermineActiveDiscipline(uiDocument.ActiveView),
                        Phase = GetProjectPhase(document),
                        ActiveSystem = GetActiveSystemName(uiDocument)
                    };

                    // Add additional metadata
                    context.AdditionalData["document_path"] = document.PathName;
                    context.AdditionalData["is_workshared"] = document.IsWorkshared;
                    context.AdditionalData["active_workset"] = GetActiveWorksetName(document);
                    context.AdditionalData["units"] = GetProjectUnits(document);

                    _logger.LogInformation("Extracted context for project: {ProjectName}", context.ProjectName);
                    return context;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error extracting Revit context");
                    return new EngineeringContext();
                }
            });
        }

        /// <summary>
        /// Extract project information
        /// </summary>
        private ProjectDetails ExtractProjectInfo(Document document)
        {
            var projectInfo = new ProjectDetails();

            try
            {
                var revitProjectInfo = document.ProjectInformation;
                
                projectInfo.Name = revitProjectInfo.Name;
                projectInfo.Number = revitProjectInfo.Number;
                projectInfo.Address = revitProjectInfo.Address;
                projectInfo.BuildingType = GetParameterValue(revitProjectInfo, "Building Type") ?? "Unknown";
                
                // Calculate gross area from area schemes or levels
                projectInfo.GrossArea = CalculateGrossArea(document);
                projectInfo.NumberOfFloors = GetNumberOfFloors(document);
                
                // Extract project parameters
                foreach (Parameter param in revitProjectInfo.Parameters)
                {
                    if (param.HasValue && !param.IsReadOnly)
                    {
                        var value = GetParameterValueAsString(param);
                        if (!string.IsNullOrEmpty(value))
                        {
                            projectInfo.ProjectParameters[param.Definition.Name] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting project info");
            }

            return projectInfo;
        }

        /// <summary>
        /// Extract information about selected elements
        /// </summary>
        private List<ElementInfo> ExtractSelectedElements(UIDocument uiDocument)
        {
            var elementInfos = new List<ElementInfo>();

            try
            {
                var selectedIds = uiDocument.Selection.GetElementIds();
                
                foreach (var elementId in selectedIds)
                {
                    var element = uiDocument.Document.GetElement(elementId);
                    if (element == null) continue;

                    var elementInfo = new ElementInfo
                    {
                        Id = elementId,
                        UniqueId = element.UniqueId,
                        Category = element.Category?.Name ?? "Unknown",
                        Location = GetElementLocation(element),
                        Level = GetElementLevel(element)
                    };

                    // Extract family information
                    if (element is FamilyInstance familyInstance)
                    {
                        elementInfo.FamilyName = familyInstance.Symbol.FamilyName;
                        elementInfo.TypeName = familyInstance.Symbol.Name;
                        
                        // Extract electrical properties
                        if (IsElectricalElement(element))
                        {
                            ExtractElectricalProperties(familyInstance, elementInfo);
                        }
                    }

                    // Extract all parameters
                    foreach (Parameter param in element.Parameters)
                    {
                        if (param.HasValue)
                        {
                            elementInfo.Parameters[param.Definition.Name] = GetParameterValueAsObject(param);
                        }
                    }

                    elementInfos.Add(elementInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting selected elements");
            }

            return elementInfos;
        }

        /// <summary>
        /// Extract electrical properties from element
        /// </summary>
        private void ExtractElectricalProperties(FamilyInstance element, ElementInfo elementInfo)
        {
            try
            {
                // Get electrical connector information
                var connectorManager = element.MEPModel?.ConnectorManager;
                if (connectorManager != null)
                {
                    foreach (Connector connector in connectorManager.Connectors)
                    {
                        if (connector.Domain == Domain.DomainElectrical)
                        {
                            // In Revit 2025, Voltage property was removed from Connector
                            // Get voltage from the electrical system instead
                            var voltage = GetConnectorVoltage(connector);
                            if (voltage.HasValue)
                                elementInfo.Voltage = voltage.Value;
                            elementInfo.ApparentLoad = GetParameterValue<double>(element, "Apparent Load");
                            elementInfo.LoadClassification = GetParameterValue(element, "Load Classification") ?? string.Empty;
                            break;
                        }
                    }
                }

                // Get panel and circuit information
                var electricalSystems = element.MEPModel?.GetElectricalSystems();
                if (electricalSystems != null && electricalSystems.Any())
                {
                    var primarySystem = electricalSystems.First();
                    elementInfo.PanelName = primarySystem.PanelName ?? string.Empty;
                    elementInfo.CircuitNumber = primarySystem.CircuitNumber ?? string.Empty;
                    elementInfo.SystemName = primarySystem.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error extracting electrical properties");
            }
        }

        /// <summary>
        /// Extract active view information
        /// </summary>
        private ViewInfo ExtractActiveViewInfo(UIDocument uiDocument)
        {
            var viewInfo = new ViewInfo();

            try
            {
                var view = uiDocument.ActiveView;
                
                viewInfo.Id = view.Id;
                viewInfo.Name = view.Name;
                viewInfo.ViewType = view.ViewType.ToString();
                viewInfo.Discipline = GetViewDiscipline(view);
                viewInfo.Scale = view.Scale;
                
                // Get level for plan views
                if (view is ViewPlan viewPlan)
                {
                    viewInfo.Level = viewPlan.GenLevel?.Name ?? string.Empty;
                }
                
                // Get bounding box
                // BoundingBox only from CropBox if active (Outline is BoundingBoxUV)
                viewInfo.BoundingBox = view.CropBoxActive ? view.CropBox : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting view info");
            }

            return viewInfo;
        }

        /// <summary>
        /// Extract visible MEP systems in current view
        /// </summary>
        private List<SystemInfo> ExtractVisibleSystems(Document document, View view)
        {
            var systems = new List<SystemInfo>();

            try
            {
                // Extract electrical systems
                var electricalSystems = new FilteredElementCollector(document, view.Id)
                    .OfClass(typeof(ElectricalSystem))
                    .Cast<ElectricalSystem>()
                    .Take(50); // Limit for performance

                foreach (var elecSystem in electricalSystems)
                {
                    var systemInfo = new SystemInfo
                    {
                        Id = elecSystem.Id,
                        Name = elecSystem.Name,
                        SystemType = "Electrical",
                        SystemClassification = elecSystem.SystemType.ToString(),
                        SystemVoltage = GetElectricalSystemVoltage(elecSystem),
                        TotalConnectedLoad = GetSystemLoad(elecSystem, LoadType.ApparentLoad),
                        DemandLoad = GetSystemLoad(elecSystem, LoadType.DemandLoad)
                    };

                    // Get connected elements
                    systemInfo.Components = elecSystem.Elements
                        .Cast<Element>()
                        .Select(e => e.Id)
                        .ToList();

                    systems.Add(systemInfo);
                }

                // Extract mechanical systems (ducts)
                var mechanicalSystems = new FilteredElementCollector(document, view.Id)
                    .OfClass(typeof(MechanicalSystem))
                    .Cast<MechanicalSystem>()
                    .Take(50);

                foreach (var mechSystem in mechanicalSystems)
                {
                    var systemInfo = new SystemInfo
                    {
                        Id = mechSystem.Id,
                        Name = mechSystem.Name,
                        SystemType = "Mechanical",
                        SystemClassification = mechSystem.SystemType.ToString(),
                        FlowRate = GetParameterValue<double>(mechSystem, "Flow"),
                        StaticPressure = GetParameterValue<double>(mechSystem, "Static Pressure")
                    };

                    systemInfo.Components = mechSystem.Elements
                        .Cast<Element>()
                        .Select(e => e.Id)
                        .ToList();

                    systems.Add(systemInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting visible systems");
            }

            return systems;
        }

        /// <summary>
        /// Get current engineer information
        /// </summary>
        private EngineerInfo GetCurrentEngineer()
        {
            // In production, this would integrate with company directory or Revit user info
            return new EngineerInfo
            {
                Name = Environment.UserName,
                Email = $"{Environment.UserName}@company.com",
                Role = EngineerRole.EIT,
                Disciplines = new List<string> { "Electrical", "Mechanical" },
                Company = "Engineering Firm"
            };
        }

        /// <summary>
        /// Get applicable codes based on project location
        /// </summary>
        private List<string> GetApplicableCodes(Document document)
        {
            // In production, this would be based on project location
            return new List<string>
            {
                "NEC 2020",
                "ASHRAE 90.1-2019",
                "IMC 2021",
                "IBC 2021",
                "NFPA 70",
                "NFPA 90A",
                "NFPA 110"
            };
        }

        #region Helper Methods

        private XYZ GetElementLocation(Element element)
        {
            var location = element.Location;
            
            if (location is LocationPoint locationPoint)
                return locationPoint.Point;
            
            if (location is LocationCurve locationCurve)
                return locationCurve.Curve.GetEndPoint(0);
                
            return XYZ.Zero;
        }

        private string GetElementLevel(Element element)
        {
            // Try to get level from element
            var levelParam = element.LookupParameter("Level");
            if (levelParam != null && levelParam.HasValue)
            {
                var levelId = levelParam.AsElementId();
                var level = element.Document.GetElement(levelId) as Level;
                return level?.Name ?? string.Empty;
            }

            // For MEP elements, check reference level
            if (element is FamilyInstance familyInstance)
            {
                return familyInstance.Host?.Name ?? string.Empty;
            }

            return string.Empty;
        }

        private bool IsElectricalElement(Element element)
        {
            var electricalCategories = new[]
            {
                BuiltInCategory.OST_ElectricalEquipment,
                BuiltInCategory.OST_ElectricalFixtures,
                BuiltInCategory.OST_LightingFixtures,
                BuiltInCategory.OST_ElectricalCircuit,
                BuiltInCategory.OST_CableTray,
                BuiltInCategory.OST_Conduit
            };

            return element.Category != null && 
                   electricalCategories.Contains((BuiltInCategory)element.Category.Id.Value);
        }

        private double CalculateGrossArea(Document document)
        {
            // Sum area from all levels
            var levels = new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Cast<Level>();

            double totalArea = 0;
            foreach (var level in levels)
            {
                // Get area plans for this level
                var areaPlans = new FilteredElementCollector(document)
                    .OfClass(typeof(ViewPlan))
                    .Cast<ViewPlan>()
                    .Where(vp => vp.AreaScheme != null && vp.GenLevel?.Id == level.Id);

                foreach (var areaPlan in areaPlans)
                {
                    var areas = new FilteredElementCollector(document, areaPlan.Id)
                        .OfCategory(BuiltInCategory.OST_Areas)
                        .Cast<Area>();

                    totalArea += areas.Sum(a => a.Area);
                }
            }

            return totalArea;
        }

        private int GetNumberOfFloors(Document document)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(Level))
                .Count();
        }

        private string GetViewDiscipline(View view)
        {
            // Determine discipline based on view type and template
            var viewTemplate = view.ViewTemplateId != ElementId.InvalidElementId
                ? view.Document.GetElement(view.ViewTemplateId) as View
                : null;

            var templateName = viewTemplate?.Name ?? view.Name;
            
            if (templateName.ToLower().Contains("electrical") || templateName.ToLower().Contains("power"))
                return "Electrical";
            
            if (templateName.ToLower().Contains("mechanical") || templateName.ToLower().Contains("hvac"))
                return "Mechanical";
                
            if (templateName.ToLower().Contains("plumbing"))
                return "Plumbing";
                
            return "General";
        }

        private string DetermineActiveDiscipline(View view)
        {
            return GetViewDiscipline(view);
        }

        private string GetProjectPhase(Document document)
        {
            // Get from project information parameters
            var projectInfo = document.ProjectInformation;
            var phaseParam = projectInfo.LookupParameter("Project Phase") ?? 
                           projectInfo.LookupParameter("Design Phase");
            
            if (phaseParam != null && phaseParam.HasValue)
                return phaseParam.AsString();
                
            return "Design Development"; // Default
        }

        private string GetActiveSystemName(UIDocument uiDocument)
        {
            // Check if a system is selected
            var selectedIds = uiDocument.Selection.GetElementIds();
            foreach (var id in selectedIds)
            {
                var element = uiDocument.Document.GetElement(id);
                if (element is MEPSystem system)
                {
                    return system.Name;
                }
            }
            
            return string.Empty;
        }

        private string GetActiveWorksetName(Document document)
        {
            if (!document.IsWorkshared)
                return string.Empty;
                
            var activeWorksetId = document.GetWorksetTable().GetActiveWorksetId();
            var workset = document.GetWorksetTable().GetWorkset(activeWorksetId);
            return workset.Name;
        }

        private Dictionary<string, string> GetProjectUnits(Document document)
        {
            var units = new Dictionary<string, string>();
            
            try
            {
                var formatOptions = document.GetUnits().GetFormatOptions(SpecTypeId.Length);
                units["Length"] = formatOptions.GetUnitTypeId().TypeId;
                
                formatOptions = document.GetUnits().GetFormatOptions(SpecTypeId.Area);
                units["Area"] = formatOptions.GetUnitTypeId().TypeId;
                
                formatOptions = document.GetUnits().GetFormatOptions(SpecTypeId.ElectricalPower);
                units["Power"] = formatOptions.GetUnitTypeId().TypeId;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting project units");
            }
            
            return units;
        }

        private string GetParameterValue(Element element, string parameterName)
        {
            var param = element.LookupParameter(parameterName);
            return param?.HasValue == true ? GetParameterValueAsString(param) : null;
        }

        private T GetParameterValue<T>(Element element, string parameterName)
        {
            var param = element.LookupParameter(parameterName);
            if (param?.HasValue != true)
                return default(T);
                
            try
            {
                return (T)GetParameterValueAsObject(param);
            }
            catch
            {
                return default(T);
            }
        }

        private string GetParameterValueAsString(Parameter parameter)
        {
            if (!parameter.HasValue)
                return string.Empty;
                
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString() ?? string.Empty;
                case StorageType.Integer:
                    return parameter.AsInteger().ToString();
                case StorageType.Double:
                    return parameter.AsValueString() ?? parameter.AsDouble().ToString("F2");
                case StorageType.ElementId:
                    var elementId = parameter.AsElementId();
                    if (elementId != ElementId.InvalidElementId)
                    {
                        var element = parameter.Element.Document.GetElement(elementId);
                        return element?.Name ?? elementId.ToString();
                    }
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private object? GetParameterValueAsObject(Parameter parameter)
        {
            if (!parameter.HasValue)
                return null;
                
            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.AsString();
                case StorageType.Integer:
                    return parameter.AsInteger();
                case StorageType.Double:
                    return parameter.AsDouble();
                case StorageType.ElementId:
                    return parameter.AsElementId().Value;
                default:
                    return null;
            }
        }

        private double GetSystemLoad(ElectricalSystem system, LoadType loadType)
        {
            try
            {
                var loadParam = loadType == LoadType.ApparentLoad 
                    ? system.LookupParameter("Total Connected Load") 
                    : system.LookupParameter("Total Demand Load");
                    
                return loadParam?.AsDouble() ?? 0.0;
            }
            catch
            {
                return 0.0;
            }
        }
        
        private double? GetConnectorVoltage(Connector connector)
        {
            try
            {
                // In Revit 2025, voltage is obtained through the electrical system
                var owner = connector.Owner;
                if (owner is FamilyInstance familyInstance)
                {
                    var elecSystems = familyInstance.MEPModel?.GetElectricalSystems();
                    if (elecSystems != null && elecSystems.Any())
                    {
                        return GetElectricalSystemVoltage(elecSystems.First());
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private double GetElectricalSystemVoltage(ElectricalSystem elecSystem)
        {
            try
            {
                // In Revit 2025, use parameter instead of direct property
                var voltageParam = elecSystem.LookupParameter("Voltage") ?? 
                                  elecSystem.LookupParameter("RBS_ELEC_VOLTAGE");
                return voltageParam?.AsDouble() ?? 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        private enum LoadType
        {
            ApparentLoad,
            DemandLoad
        }

        #endregion
    }
}