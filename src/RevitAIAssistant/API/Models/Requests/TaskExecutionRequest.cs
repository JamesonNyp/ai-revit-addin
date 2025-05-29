using System.Collections.Generic;

namespace RevitAIAssistant.API.Models.Requests
{
    /// <summary>
    /// Request to execute a task plan
    /// </summary>
    public class TaskExecutionRequest
    {
        /// <summary>
        /// Execution mode (automatic, supervised, manual)
        /// </summary>
        public ExecutionMode Mode { get; set; } = ExecutionMode.Supervised;

        /// <summary>
        /// Additional execution parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

    /// <summary>
    /// Task execution modes
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// Fully automatic execution without user intervention
        /// </summary>
        Automatic,

        /// <summary>
        /// Execution with user confirmation at key points
        /// </summary>
        Supervised,

        /// <summary>
        /// Manual step-by-step execution
        /// </summary>
        Manual
    }
}