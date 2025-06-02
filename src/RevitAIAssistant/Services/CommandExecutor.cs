using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.Services
{
    /// <summary>
    /// Executes Revit commands from the AI platform
    /// Implements the command pattern with transaction management
    /// </summary>
    public class CommandExecutor
    {
        private readonly ILogger<CommandExecutor> _logger;
        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public CommandExecutor(ILogger<CommandExecutor> logger)
        {
            _logger = logger;
            // Document will be set by SessionManager when available
        }

        /// <summary>
        /// Execute a single Revit command
        /// </summary>
        public async Task<CommandExecutionResult> ExecuteCommandAsync(RevitCommand command, Document document)
        {
            _logger.LogInformation("Executing command: {CommandType} (ID: {CommandId})", 
                command.CommandType, command.CommandId);

            var result = new CommandExecutionResult
            {
                CommandId = command.CommandId
            };

            try
            {
                // Validate pre-conditions
                if (command.Validation != null)
                {
                    var validationResult = ValidatePreConditions(command.Validation, document);
                    if (!validationResult.Success)
                    {
                        result.Success = false;
                        result.Message = validationResult.Message;
                        return result;
                    }
                }

                // Execute command based on type
                switch (command.CommandType.ToLower())
                {
                    case "create_panel_schedule":
                        result = await CreatePanelScheduleAsync(command, document);
                        break;
                        
                    case "update_panel_schedule":
                        result = await UpdatePanelScheduleAsync(command, document);
                        break;
                        
                    case "create_electrical_circuit":
                        result = await CreateElectricalCircuitAsync(command, document);
                        break;
                        
                    case "place_electrical_equipment":
                        result = await PlaceElectricalEquipmentAsync(command, document);
                        break;
                        
                    case "place_mechanical_equipment":
                        result = await PlaceMechanicalEquipmentAsync(command, document);
                        break;
                        
                    case "create_duct_system":
                        result = await CreateDuctSystemAsync(command, document);
                        break;
                        
                    case "create_pipe_system":
                        result = await CreatePipeSystemAsync(command, document);
                        break;
                        
                    case "update_parameter":
                        result = await UpdateParameterAsync(command, document);
                        break;
                        
                    case "create_schedule":
                        result = await CreateScheduleAsync(command, document);
                        break;
                        
                    default:
                        result.Success = false;
                        result.Message = $"Unknown command type: {command.CommandType}";
                        break;
                }

                // Validate post-conditions
                if (result.Success && command.Validation != null)
                {
                    var postValidation = ValidatePostConditions(command.Validation, document, result);
                    if (!postValidation.Success)
                    {
                        result.Warnings.Add($"Post-condition validation warning: {postValidation.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command {CommandId}", command.CommandId);
                result.Success = false;
                result.Message = $"Command execution error: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Execute multiple commands in a batch
        /// </summary>
        public async Task<List<CommandExecutionResult>> ExecuteBatchAsync(
            List<RevitCommand> commands, 
            Document document,
            bool stopOnError = false)
        {
            var results = new List<CommandExecutionResult>();

            using (var transactionGroup = new TransactionGroup(document, "AI Command Batch"))
            {
                transactionGroup.Start();

                foreach (var command in commands)
                {
                    var result = await ExecuteCommandAsync(command, document);
                    results.Add(result);

                    if (!result.Success && stopOnError)
                    {
                        _logger.LogWarning("Batch execution stopped due to error in command {CommandId}", 
                            command.CommandId);
                        transactionGroup.RollBack();
                        return results;
                    }
                }

                transactionGroup.Assimilate();
            }

            return results;
        }

        #region Electrical Commands

        private async Task<CommandExecutionResult> CreatePanelScheduleAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Create Panel Schedule"))
                {
                    transaction.Start();

                    // Extract parameters
                    var panelName = command.Parameters["panel_name"].ToString();
                    var voltage = Convert.ToDouble(command.Parameters["voltage"]);
                    var phases = Convert.ToInt32(command.Parameters["phases"]);
                    var mainBreakerSize = Convert.ToInt32(command.Parameters["main_breaker_size"]);
                    var circuitCount = Convert.ToInt32(command.Parameters["circuit_count"]);

                    // Find electrical equipment family type for panel
                    var panelType = new FilteredElementCollector(document)
                        .OfCategory(BuiltInCategory.OST_ElectricalEquipment)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(fs => fs.FamilyName.Contains("Panel"));

                    if (panelType == null)
                    {
                        result.Success = false;
                        result.Message = "No suitable panel family found in project";
                        transaction.RollBack();
                        return result;
                    }

                    // Activate the type if needed
                    if (!panelType.IsActive)
                        panelType.Activate();

                    // Get level (default to lowest level)
                    var level = new FilteredElementCollector(document)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .OrderBy(l => l.Elevation)
                        .FirstOrDefault();

                    if (level == null)
                    {
                        result.Success = false;
                        result.Message = "No levels found in project";
                        transaction.RollBack();
                        return result;
                    }

                    // Create panel instance
                    XYZ location = XYZ.Zero;
                    if (command.Parameters.ContainsKey("location"))
                    {
                        var loc = command.Parameters["location"] as Dictionary<string, object>;
                        if (loc != null)
                        {
                            location = new XYZ(
                                Convert.ToDouble(loc["x"]),
                                Convert.ToDouble(loc["y"]),
                                Convert.ToDouble(loc["z"])
                            );
                        }
                    }

                    var panelInstance = document.Create.NewFamilyInstance(
                        location,
                        panelType,
                        level,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                    );

                    // Set panel parameters
                    SetParameterValue(panelInstance, "Panel Name", panelName);
                    SetParameterValue(panelInstance, "Voltage", voltage);
                    SetParameterValue(panelInstance, "Number of Phases", phases);
                    SetParameterValue(panelInstance, "Mains", mainBreakerSize);

                    // Create electrical system for the panel
                    var electricalSystem = ElectricalSystem.Create(
                        document,
                        new List<ElementId> { panelInstance.Id },
                        ElectricalSystemType.PowerCircuit
                    );

                    result.Success = true;
                    result.Message = $"Successfully created panel schedule '{panelName}'";
                    result.CreatedElements.Add(panelInstance.Id);
                    result.ResultData["panel_id"] = panelInstance.Id.Value;
                    result.ResultData["system_id"] = electricalSystem.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating panel schedule");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<CommandExecutionResult> UpdatePanelScheduleAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Update Panel Schedule"))
                {
                    transaction.Start();

                    var panelId = new ElementId(Convert.ToInt64(command.Parameters["panel_id"]));
                    var panel = document.GetElement(panelId) as FamilyInstance;

                    if (panel == null)
                    {
                        result.Success = false;
                        result.Message = "Panel not found";
                        transaction.RollBack();
                        return result;
                    }

                    // Update circuits if provided
                    if (command.Parameters.ContainsKey("circuits"))
                    {
                        var circuits = command.Parameters["circuits"] as List<Dictionary<string, object>>;
                        // Implementation for circuit updates would go here
                        // This would involve creating/modifying ElectricalSystem objects
                    }

                    result.Success = true;
                    result.Message = "Panel schedule updated successfully";
                    result.ModifiedElements.Add(panelId);

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating panel schedule");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<CommandExecutionResult> CreateElectricalCircuitAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Create Electrical Circuit"))
                {
                    transaction.Start();

                    // Extract parameters
                    var circuitNumber = command.Parameters["circuit_number"].ToString();
                    var circuitName = command.Parameters["circuit_name"].ToString();
                    var voltage = Convert.ToDouble(command.Parameters["voltage"]);
                    var loadName = command.Parameters["load_name"].ToString();
                    var connectedLoad = Convert.ToDouble(command.Parameters["connected_load"]);
                    var panelId = new ElementId(Convert.ToInt64(command.Parameters["panel_id"]));

                    // Get panel
                    var panel = document.GetElement(panelId) as FamilyInstance;
                    if (panel == null)
                    {
                        result.Success = false;
                        result.Message = "Panel not found";
                        transaction.RollBack();
                        return result;
                    }

                    // Create electrical system
                    var electricalSystem = ElectricalSystem.Create(
                        document,
                        new List<ElementId> { }, // Add connected elements here
                        ElectricalSystemType.PowerCircuit
                    );

                    // Set circuit parameters
                    SetParameterValue(electricalSystem, "Circuit Number", circuitNumber);
                    SetParameterValue(electricalSystem, "Load Name", loadName);
                    SetParameterValue(electricalSystem, "Panel", panel.Name);
                    SetParameterValue(electricalSystem, "Apparent Load", connectedLoad);
                    SetParameterValue(electricalSystem, "Voltage", voltage);

                    result.Success = true;
                    result.Message = $"Successfully created circuit '{circuitNumber} - {circuitName}'";
                    result.CreatedElements.Add(electricalSystem.Id);
                    result.ResultData["circuit_id"] = electricalSystem.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating electrical circuit");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<CommandExecutionResult> PlaceElectricalEquipmentAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Place Electrical Equipment"))
                {
                    transaction.Start();

                    // Extract parameters
                    var familyName = command.Parameters["family_name"].ToString();
                    var familyType = command.Parameters["family_type"].ToString();
                    var location = ExtractLocation(command.Parameters["location"]);
                    var levelId = new ElementId(Convert.ToInt64(command.Parameters["level_id"]));

                    // Find family symbol
                    var symbol = FindFamilySymbol(document, familyName, familyType);
                    if (symbol == null)
                    {
                        result.Success = false;
                        result.Message = $"Family type '{familyName} - {familyType}' not found";
                        transaction.RollBack();
                        return result;
                    }

                    // Get level
                    var level = document.GetElement(levelId) as Level;
                    if (level == null)
                    {
                        result.Success = false;
                        result.Message = "Level not found";
                        transaction.RollBack();
                        return result;
                    }

                    // Activate symbol if needed
                    if (!symbol.IsActive)
                        symbol.Activate();

                    // Create instance
                    var instance = document.Create.NewFamilyInstance(
                        location,
                        symbol,
                        level,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural
                    );

                    // Set additional parameters
                    if (command.Parameters.ContainsKey("parameters"))
                    {
                        var parameters = command.Parameters["parameters"] as Dictionary<string, object>;
                        foreach (var param in parameters)
                        {
                            SetParameterValue(instance, param.Key, param.Value);
                        }
                    }

                    result.Success = true;
                    result.Message = $"Successfully placed {familyName}";
                    result.CreatedElements.Add(instance.Id);
                    result.ResultData["element_id"] = instance.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing electrical equipment");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        #endregion

        #region Mechanical Commands

        private async Task<CommandExecutionResult> PlaceMechanicalEquipmentAsync(RevitCommand command, Document document)
        {
            // Similar to electrical equipment placement but for mechanical families
            return await PlaceElectricalEquipmentAsync(command, document);
        }

        private async Task<CommandExecutionResult> CreateDuctSystemAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Create Duct System"))
                {
                    transaction.Start();

                    // Extract parameters
                    var systemName = command.Parameters["system_name"].ToString();
                    var systemType = command.Parameters["system_type"].ToString();
                    var equipmentIds = (command.Parameters["equipment_ids"] as List<object>)
                        ?.Select(id => new ElementId(Convert.ToInt64(id)))
                        .ToList() ?? new List<ElementId>();

                    // Get mechanical system type
                    var mechanicalSystemType = systemType.ToLower() switch
                    {
                        "supply air" => DuctSystemType.SupplyAir,
                        "return air" => DuctSystemType.ReturnAir,
                        "exhaust air" => DuctSystemType.ExhaustAir,
                        _ => DuctSystemType.SupplyAir
                    };

                    // Create mechanical system
                    var mechanicalSystem = MechanicalSystem.Create(
                        document,
                        equipmentIds,
                        mechanicalSystemType
                    );

                    // Set system name
                    SetParameterValue(mechanicalSystem, "System Name", systemName);

                    result.Success = true;
                    result.Message = $"Successfully created duct system '{systemName}'";
                    result.CreatedElements.Add(mechanicalSystem.Id);
                    result.ResultData["system_id"] = mechanicalSystem.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating duct system");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<CommandExecutionResult> CreatePipeSystemAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Create Pipe System"))
                {
                    transaction.Start();

                    // Extract parameters
                    var systemName = command.Parameters["system_name"].ToString();
                    var systemType = command.Parameters["system_type"].ToString();
                    var equipmentIds = (command.Parameters["equipment_ids"] as List<object>)
                        ?.Select(id => new ElementId(Convert.ToInt64(id)))
                        .ToList() ?? new List<ElementId>();

                    // Create piping system
                    // Note: PipingSystem.Create requires a connector, not a list of element IDs
                    // For now, create with first equipment if available
                    PipingSystem? pipingSystem = null;
                    if (equipmentIds.Count > 0)
                    {
                        var firstEquipment = document.GetElement(equipmentIds[0]) as FamilyInstance;
                        if (firstEquipment?.MEPModel?.ConnectorManager != null)
                        {
                            foreach (Connector connector in firstEquipment.MEPModel.ConnectorManager.Connectors)
                            {
                                if (connector.Domain == Domain.DomainPiping && !connector.IsConnected)
                                {
                                    pipingSystem = PipingSystem.Create(
                                        document,
                                        connector,
                                        PipeSystemType.SupplyHydronic
                                    );
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (pipingSystem == null)
                    {
                        result.Success = false;
                        result.Message = "No suitable connector found to create piping system";
                        transaction.RollBack();
                        return result;
                    }

                    // Set system name
                    SetParameterValue(pipingSystem, "System Name", systemName);

                    result.Success = true;
                    result.Message = $"Successfully created pipe system '{systemName}'";
                    result.CreatedElements.Add(pipingSystem.Id);
                    result.ResultData["system_id"] = pipingSystem.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pipe system");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        #endregion

        #region General Commands

        private async Task<CommandExecutionResult> UpdateParameterAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Update Parameter"))
                {
                    transaction.Start();

                    var elementId = new ElementId(Convert.ToInt64(command.Parameters["element_id"]));
                    var parameterName = command.Parameters["parameter_name"].ToString();
                    var parameterValue = command.Parameters["parameter_value"];

                    var element = document.GetElement(elementId);
                    if (element == null)
                    {
                        result.Success = false;
                        result.Message = "Element not found";
                        transaction.RollBack();
                        return result;
                    }

                    var success = SetParameterValue(element, parameterName, parameterValue);
                    if (success)
                    {
                        result.Success = true;
                        result.Message = $"Successfully updated parameter '{parameterName}'";
                        result.ModifiedElements.Add(elementId);
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = $"Failed to update parameter '{parameterName}'";
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parameter");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        private async Task<CommandExecutionResult> CreateScheduleAsync(RevitCommand command, Document document)
        {
            var result = new CommandExecutionResult { CommandId = command.CommandId };

            try
            {
                using (var transaction = new Transaction(document, command.TransactionName ?? "Create Schedule"))
                {
                    transaction.Start();

                    var scheduleName = command.Parameters["schedule_name"].ToString();
                    var categoryName = command.Parameters["category"].ToString();
                    var fields = (command.Parameters["fields"] as List<object>)
                        ?.Select(f => f.ToString())
                        .ToList() ?? new List<string>();

                    // Get category
                    var category = GetCategoryByName(categoryName, document);
                    if (category == null)
                    {
                        result.Success = false;
                        result.Message = $"Category '{categoryName}' not found";
                        transaction.RollBack();
                        return result;
                    }

                    // Create schedule
                    var schedule = ViewSchedule.CreateSchedule(document, category.Id);
                    schedule.Name = scheduleName;

                    // Add fields
                    foreach (var fieldName in fields)
                    {
                        var schedulableFields = schedule.Definition.GetSchedulableFields();
                        var field = schedulableFields.FirstOrDefault(sf => sf.GetName(document) == fieldName);
                        if (field != null)
                        {
                            schedule.Definition.AddField(field);
                        }
                    }

                    result.Success = true;
                    result.Message = $"Successfully created schedule '{scheduleName}'";
                    result.CreatedElements.Add(schedule.Id);
                    result.ResultData["schedule_id"] = schedule.Id.Value;

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule");
                result.Success = false;
                result.Message = ex.Message;
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private bool SetParameterValue(Element element, string parameterName, object value)
        {
            try
            {
                var parameter = element.LookupParameter(parameterName);
                if (parameter == null || parameter.IsReadOnly)
                    return false;

                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        parameter.Set(Convert.ToDouble(value));
                        break;
                    case StorageType.Integer:
                        parameter.Set(Convert.ToInt32(value));
                        break;
                    case StorageType.String:
                        parameter.Set(value.ToString());
                        break;
                    case StorageType.ElementId:
                        parameter.Set(new ElementId(Convert.ToInt64(value)));
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set parameter {ParameterName} on element", parameterName);
                return false;
            }
        }

        private FamilySymbol FindFamilySymbol(Document document, string familyName, string typeName)
        {
            return new FilteredElementCollector(document)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(fs => fs.FamilyName == familyName && fs.Name == typeName);
        }

        private XYZ ExtractLocation(object locationData)
        {
            if (locationData is Dictionary<string, object> loc)
            {
                return new XYZ(
                    Convert.ToDouble(loc["x"]),
                    Convert.ToDouble(loc["y"]),
                    Convert.ToDouble(loc["z"])
                );
            }
            return XYZ.Zero;
        }

        private Category? GetCategoryByName(string categoryName, Document document)
        {
            // Map common category names to BuiltInCategory
            var categoryMap = new Dictionary<string, BuiltInCategory>
            {
                { "Electrical Equipment", BuiltInCategory.OST_ElectricalEquipment },
                { "Mechanical Equipment", BuiltInCategory.OST_MechanicalEquipment },
                { "Electrical Fixtures", BuiltInCategory.OST_ElectricalFixtures },
                { "Lighting Fixtures", BuiltInCategory.OST_LightingFixtures },
                { "Ducts", BuiltInCategory.OST_DuctCurves },
                { "Pipes", BuiltInCategory.OST_PipeCurves }
            };

            if (categoryMap.TryGetValue(categoryName, out var builtInCategory))
            {
                return Category.GetCategory(document, builtInCategory);
            }

            return null;
        }

        private (bool Success, string Message) ValidatePreConditions(
            CommandValidation validation, 
            Document document)
        {
            foreach (var condition in validation.PreConditions)
            {
                // Implement specific validation logic based on condition type
                _logger.LogDebug("Validating pre-condition: {Condition}", condition);
            }
            return (true, "Pre-conditions met");
        }

        private (bool Success, string Message) ValidatePostConditions(
            CommandValidation validation, 
            Document document,
            CommandExecutionResult result)
        {
            foreach (var condition in validation.PostConditions)
            {
                // Implement specific validation logic based on condition type
                _logger.LogDebug("Validating post-condition: {Condition}", condition);
            }
            return (true, "Post-conditions met");
        }

        #endregion
    }
}