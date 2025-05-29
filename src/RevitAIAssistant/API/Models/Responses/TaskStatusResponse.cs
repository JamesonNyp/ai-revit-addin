using System;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.API.Models.Responses
{
    /// <summary>
    /// Response containing current task execution status
    /// </summary>
    public class TaskStatusResponse
    {
        /// <summary>
        /// Execution identifier
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Current status (pending, in_progress, completed, failed)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// Current step being executed
        /// </summary>
        public string CurrentStep { get; set; } = string.Empty;

        /// <summary>
        /// Estimated time remaining in seconds
        /// </summary>
        public int? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// Whether approval is required to continue
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Approval details if required
        /// </summary>
        public ApprovalDetails? ApprovalDetails { get; set; }

        /// <summary>
        /// Results if task is completed
        /// </summary>
        public TaskExecutionResults? Results { get; set; }

        /// <summary>
        /// Error message if failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}