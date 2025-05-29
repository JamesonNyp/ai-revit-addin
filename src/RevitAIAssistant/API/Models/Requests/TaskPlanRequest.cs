using System.Collections.Generic;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.API.Models.Requests
{
    /// <summary>
    /// Request to create an engineering task execution plan
    /// </summary>
    public class TaskPlanRequest
    {
        /// <summary>
        /// Natural language description of the engineering task
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Current engineering context from Revit
        /// </summary>
        public EngineeringContext Context { get; set; } = new();

        /// <summary>
        /// Task priority (low, normal, high, urgent)
        /// </summary>
        public string Priority { get; set; } = "normal";

        /// <summary>
        /// Additional constraints for task execution
        /// </summary>
        public TaskConstraints? Constraints { get; set; }
    }

    /// <summary>
    /// Constraints for task execution
    /// </summary>
    public class TaskConstraints
    {
        /// <summary>
        /// Maximum execution time in seconds
        /// </summary>
        public int? MaxExecutionTime { get; set; }

        /// <summary>
        /// Whether PE approval is required for all steps
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Specific standards to follow
        /// </summary>
        public List<string> EnforceStandards { get; set; } = new();

        /// <summary>
        /// Optimization preference (speed, accuracy, cost)
        /// </summary>
        public string OptimizationPreference { get; set; } = "accuracy";
    }
}