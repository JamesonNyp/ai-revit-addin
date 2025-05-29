using RevitAIAssistant.Models;

namespace RevitAIAssistant.API.Models.Requests
{
    /// <summary>
    /// General query request to the AI platform
    /// </summary>
    public class QueryRequest
    {
        /// <summary>
        /// The user's query in natural language
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Current engineering context
        /// </summary>
        public EngineeringContext Context { get; set; } = new();

        /// <summary>
        /// Session identifier for conversation continuity
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
    }
}