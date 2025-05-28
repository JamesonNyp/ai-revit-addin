using System;
using System.Collections.Generic;

namespace RevitAIAssistant.Models
{
    /// <summary>
    /// Base class for all Revit commands
    /// </summary>
    public class RevitCommand
    {
        public string CommandType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public CommandPriority Priority { get; set; } = CommandPriority.Normal;
        public bool RequiresTransaction { get; set; } = true;
        public string? TransactionName { get; set; }
    }

    public enum CommandPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    /// <summary>
    /// Command to create a new element
    /// </summary>
    public class CreateElementCommand : RevitCommand
    {
        public string ElementType { get; set; } = string.Empty;
        public string? FamilyName { get; set; }
        public string? TypeName { get; set; }
        public Point3D? Location { get; set; }
        public string? Level { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();

        public CreateElementCommand()
        {
            CommandType = "CreateElement";
        }
    }

    /// <summary>
    /// Command to modify element parameters
    /// </summary>
    public class ModifyParametersCommand : RevitCommand
    {
        public List<int> ElementIds { get; set; } = new();
        public Dictionary<string, object> ParameterValues { get; set; } = new();
        public bool CreateIfMissing { get; set; } = false;

        public ModifyParametersCommand()
        {
            CommandType = "ModifyParameters";
        }
    }

    /// <summary>
    /// Command to run calculations
    /// </summary>
    public class RunCalculationCommand : RevitCommand
    {
        public string CalculationType { get; set; } = string.Empty;
        public List<int> TargetElementIds { get; set; } = new();
        public Dictionary<string, object> CalculationParameters { get; set; } = new();
        public bool UpdateElements { get; set; } = true;
        public string? ReportFormat { get; set; }

        public RunCalculationCommand()
        {
            CommandType = "RunCalculation";
        }
    }

    /// <summary>
    /// Command to generate documentation
    /// </summary>
    public class GenerateDocumentationCommand : RevitCommand
    {
        public string DocumentationType { get; set; } = string.Empty;
        public List<string> ViewNames { get; set; } = new();
        public string? TemplateName { get; set; }
        public Dictionary<string, object> Options { get; set; } = new();
        public string? OutputPath { get; set; }

        public GenerateDocumentationCommand()
        {
            CommandType = "GenerateDocumentation";
        }
    }

    /// <summary>
    /// Command to validate model
    /// </summary>
    public class ValidateModelCommand : RevitCommand
    {
        public List<string> ValidationRules { get; set; } = new();
        public string? Scope { get; set; }
        public bool FixAutomatically { get; set; } = false;
        public Dictionary<string, object> ValidationOptions { get; set; } = new();

        public ValidateModelCommand()
        {
            CommandType = "ValidateModel";
        }
    }

    /// <summary>
    /// Command result
    /// </summary>
    public class CommandResult
    {
        public string CommandId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Message { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public List<CommandError> Errors { get; set; } = new();
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan ExecutionTime { get; set; }
    }

    public class CommandError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int? ElementId { get; set; }
    }
}