using System;
using System.Collections.Generic;
using RevitAIAssistant.Services;

namespace RevitAIAssistant.Models
{
    /// <summary>
    /// Content model for displaying orchestration progress in the chat
    /// </summary>
    public class OrchestrationProgressContent
    {
        public MockOrchestrationService.OrchestrationProcess Process { get; set; } = new MockOrchestrationService.OrchestrationProcess();
    }

    /// <summary>
    /// Content model for displaying orchestration results
    /// </summary>
    public class OrchestrationResultsContent
    {
        public string ProcessType { get; set; } = "";
        public TimeSpan ExecutionTime { get; set; }
        public List<MockOrchestrationService.OrchestrationStep> Steps { get; set; } = new List<MockOrchestrationService.OrchestrationStep>();
    }
}