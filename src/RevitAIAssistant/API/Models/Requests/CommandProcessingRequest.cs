using RevitAIAssistant.Models;

namespace RevitAIAssistant.API.Models.Requests
{
    /// <summary>
    /// Request to process a Revit command
    /// </summary>
    public class CommandProcessingRequest
    {
        /// <summary>
        /// Unique command identifier
        /// </summary>
        public string CommandId { get; set; } = string.Empty;

        /// <summary>
        /// The Revit command to process
        /// </summary>
        public RevitCommand Command { get; set; } = new();

        /// <summary>
        /// Current engineering context
        /// </summary>
        public EngineeringContext Context { get; set; } = new();
    }
}