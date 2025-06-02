using System;
using System.Collections.Generic;

namespace RevitAIAssistant.Models
{
    /// <summary>
    /// Content model for displaying engineering execution plans
    /// </summary>
    public class ExecutionPlanContent
    {
        public EngineeringExecutionPlan Plan { get; set; } = new();
        public Action? OnApprove { get; set; }
        public Action? OnReject { get; set; }
    }

    /// <summary>
    /// Content model for displaying calculation results
    /// </summary>
    public class CalculationResultsContent
    {
        public List<CalculationResult> Calculations { get; set; } = new();
    }

    /// <summary>
    /// Content model for displaying engineering documentation
    /// </summary>
    public class DocumentationContent
    {
        public EngineeringDocumentation Documentation { get; set; } = new();
        public Action<string>? OnExport { get; set; }
    }
}