using System.Collections.Generic;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.API.Models.Responses
{
    /// <summary>
    /// Response containing the engineering task execution plan
    /// </summary>
    public class TaskPlanResponse
    {
        /// <summary>
        /// Unique task identifier
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Current status of the task plan
        /// </summary>
        public string Status { get; set; } = "ready";

        /// <summary>
        /// The execution plan
        /// </summary>
        public EngineeringExecutionPlan Plan { get; set; } = new();

        /// <summary>
        /// Estimated total duration in seconds
        /// </summary>
        public int EstimatedDuration { get; set; }

        /// <summary>
        /// Any warnings or considerations
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}