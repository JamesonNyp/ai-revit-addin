using System.Collections.Generic;

namespace RevitAIAssistant.API.Models.Responses
{
    /// <summary>
    /// Response from a general query to the AI platform
    /// </summary>
    public class QueryResponse
    {
        /// <summary>
        /// The AI's response text
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Session identifier
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Response metadata
        /// </summary>
        public ResponseMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// Metadata about the AI response
    /// </summary>
    public class ResponseMetadata
    {
        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Response type (answer, task_plan, clarification)
        /// </summary>
        public string ResponseType { get; set; } = "answer";

        /// <summary>
        /// Code references mentioned
        /// </summary>
        public List<string> References { get; set; } = new();

        /// <summary>
        /// Whether human review is recommended
        /// </summary>
        public bool RequiresReview { get; set; }
    }
}