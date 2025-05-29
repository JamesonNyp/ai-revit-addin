using System;

namespace RevitAIAssistant.API.Models.Responses
{
    /// <summary>
    /// Response when starting task execution
    /// </summary>
    public class TaskExecutionResponse
    {
        /// <summary>
        /// Unique execution identifier
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Task being executed
        /// </summary>
        public string TaskId { get; set; } = string.Empty;

        /// <summary>
        /// Current execution status
        /// </summary>
        public string Status { get; set; } = "started";

        /// <summary>
        /// Execution start time
        /// </summary>
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Initial message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}