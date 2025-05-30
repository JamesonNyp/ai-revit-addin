using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitAIAssistant.Models
{
    /// <summary>
    /// Comprehensive engineering context for AI processing
    /// </summary>
    public class EngineeringContext
    {
        public ProjectInfo ProjectInfo { get; set; } = new();
        public List<ElementInfo> SelectedElements { get; set; } = new();
        public ViewInfo ActiveView { get; set; } = new();
        public List<SystemInfo> VisibleSystems { get; set; } = new();
        public EngineerInfo CurrentEngineer { get; set; } = new();
        public List<string> ApplicableCodes { get; set; } = new();
        public string ActiveSystem { get; set; } = string.Empty;
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Detailed project information
    /// </summary>
    public class ProjectInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BuildingType { get; set; } = string.Empty; // Office, Hospital, Industrial, etc.
        public double GrossArea { get; set; } // Square feet
        public int NumberOfFloors { get; set; }
        public string ConstructionType { get; set; } = string.Empty;
        public string OccupancyType { get; set; } = string.Empty;
        public Dictionary<string, string> ProjectParameters { get; set; } = new();
    }

    /// <summary>
    /// Information about selected Revit elements
    /// </summary>
    public class ElementInfo
    {
        public ElementId Id { get; set; }
        public string UniqueId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public XYZ Location { get; set; }
        public string Level { get; set; } = string.Empty;
        public string SystemName { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty;
        
        // Electrical specific
        public double? ApparentLoad { get; set; } // VA
        public double? Voltage { get; set; }
        public string LoadClassification { get; set; } = string.Empty;
        public string PanelName { get; set; } = string.Empty;
        public string CircuitNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Active view information
    /// </summary>
    public class ViewInfo
    {
        public ElementId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ViewType { get; set; } = string.Empty; // Plan, 3D, Schedule, etc.
        public string Discipline { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double Scale { get; set; }
        public BoundingBoxXYZ BoundingBox { get; set; }
    }

    /// <summary>
    /// MEP system information
    /// </summary>
    public class SystemInfo
    {
        public ElementId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SystemType { get; set; } = string.Empty;
        public string SystemClassification { get; set; } = string.Empty;
        public List<ElementId> Components { get; set; } = new();
        
        // Electrical systems
        public double? TotalConnectedLoad { get; set; } // VA
        public double? DemandLoad { get; set; } // VA
        public double? SystemVoltage { get; set; }
        public int? NumberOfPhases { get; set; }
        
        // Mechanical systems
        public double? FlowRate { get; set; }
        public double? StaticPressure { get; set; }
        public string FluidType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Current engineer information
    /// </summary>
    public class EngineerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public EngineerRole Role { get; set; } = EngineerRole.EIT;
        public List<string> Disciplines { get; set; } = new() { "Electrical" };
        public string Company { get; set; } = string.Empty;
    }

    public enum EngineerRole
    {
        EIT,      // Engineer in Training
        PE,       // Professional Engineer
        Senior,   // Senior Engineer
        Principal // Principal Engineer
    }

    /// <summary>
    /// Engineering execution plan for complex tasks
    /// </summary>
    public class EngineeringExecutionPlan
    {
        public string PlanId { get; set; } = Guid.NewGuid().ToString();
        public string Objective { get; set; } = string.Empty;
        public PlanComplexity Complexity { get; set; }
        public string EstimatedDuration { get; set; } = string.Empty;
        public EngineeringDiscipline Discipline { get; set; }
        public ExecutionStrategy ExecutionStrategy { get; set; } = new();
        public TaskBreakdown TaskBreakdown { get; set; } = new();
        public List<EngineeringTask> Tasks { get; set; } = new();
        public List<ApprovalPoint> ApprovalPoints { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
    }

    public enum PlanComplexity
    {
        Simple,    // < 5 steps, single discipline
        Moderate,  // 5-10 steps, may involve coordination
        Complex,   // 10+ steps, multi-discipline coordination
        Critical   // Requires PE approval at multiple points
    }

    public enum EngineeringDiscipline
    {
        Electrical,
        Mechanical,
        Plumbing,
        Fire_Protection,
        Multi_Discipline
    }

    /// <summary>
    /// Strategy for executing the engineering plan
    /// </summary>
    public class ExecutionStrategy
    {
        public string Approach { get; set; } = string.Empty;
        public List<string> KeyConsiderations { get; set; } = new();
        public List<string> RiskFactors { get; set; } = new();
        public string OptimizationFocus { get; set; } = string.Empty; // Cost, Time, Quality, etc.
    }

    /// <summary>
    /// Breakdown methodology for the task
    /// </summary>
    public class TaskBreakdown
    {
        public string Methodology { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public List<string> Assumptions { get; set; } = new();
        public List<string> Constraints { get; set; } = new();
    }

    /// <summary>
    /// Individual engineering task within a plan
    /// </summary>
    public class EngineeringTask
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Methodology { get; set; } = string.Empty;
        public string ExpectedOutcome { get; set; } = string.Empty;
        public string EstimatedTime { get; set; } = string.Empty;
        public AssignedAgent AssignedAgent { get; set; }
        public List<string> RequiredInputs { get; set; } = new();
        public List<string> Deliverables { get; set; } = new();
        public List<string> PotentialIssues { get; set; } = new();
        public bool RequiresPEApproval { get; set; }
        public List<RevitCommand> RevitCommands { get; set; } = new();
        public TaskStatus Status { get; set; } = TaskStatus.Pending;
        public TaskResult? Result { get; set; }
    }

    public enum AssignedAgent
    {
        Electrical_System_Designer,
        Electrical_Calculations,
        Electrical_QA_QC,
        Mechanical_System_Designer,
        Mechanical_Calculations,
        Mechanical_QA_QC,
        Cross_Discipline_Coordinator,
        Standards_Compliance,
        Document_Management
    }

    public enum TaskStatus
    {
        Pending,
        InProgress,
        WaitingForApproval,
        Approved,
        Completed,
        Failed,
        Skipped
    }

    /// <summary>
    /// Result of an engineering task execution
    /// </summary>
    public class TaskResult
    {
        public bool Success { get; set; }
        public string Summary { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Professional Engineer approval point
    /// </summary>
    public class ApprovalPoint
    {
        public string ApprovalId { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public string ApprovalType { get; set; } = string.Empty; // Calculation, Design Decision, Code Interpretation
        public List<string> RequiredDocuments { get; set; } = new();
        public string LiabilityStatement { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? Comments { get; set; }
    }

    public enum ApprovalStatus
    {
        Pending,
        UnderReview,
        Approved,
        Rejected,
        ConditionallyApproved
    }

        public string Message { get; set; } = string.Empty;
        public List<ElementId> CreatedElements { get; set; } = new();
        public List<ElementId> ModifiedElements { get; set; } = new();
        public Dictionary<string, object> ResultData { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Engineering calculation result
    /// </summary>
    public class CalculationResult
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Methodology { get; set; } = string.Empty;
        public List<CalculationStep> Steps { get; set; } = new();
        public Dictionary<string, double> Results { get; set; } = new();
        public List<string> CodeReferences { get; set; } = new();
        public string Units { get; set; } = string.Empty;
        public double SafetyFactor { get; set; } = 1.0;
        public bool MeetsRequirements { get; set; }
        public List<string> Notes { get; set; } = new();
    }

    /// <summary>
    /// Individual calculation step
    /// </summary>
    public class CalculationStep
    {
        public int StepNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Formula { get; set; } = string.Empty;
        public Dictionary<string, double> Inputs { get; set; } = new();
        public double Result { get; set; }
        public string Units { get; set; } = string.Empty;
        public string? CodeReference { get; set; }
    }

    /// <summary>
    /// Engineering documentation
    /// </summary>
    public class EngineeringDocumentation
    {
        public string DocumentId { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public DocumentType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<Attachment> Attachments { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public bool RequiresPESeal { get; set; }
    }

    public enum DocumentType
    {
        CalculationReport,
        DesignNarrative,
        EquipmentSchedule,
        ComplianceReport,
        QAQCChecklist,
        BasisOfDesign,
        Specification
    }

    /// <summary>
    /// Document attachment
    /// </summary>
    public class Attachment
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // PDF, DWG, Excel, etc.
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long Size { get; set; }
    }

    /// <summary>
    /// Task execution results
    /// </summary>
    public class TaskExecutionResults
    {
        public List<RevitCommand> RevitCommands { get; set; } = new();
        public List<CalculationResult> Calculations { get; set; } = new();
        public EngineeringDocumentation? Documentation { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Approval details for PE review
    /// </summary>
    public class ApprovalDetails
    {
        public string ApprovalId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Dictionary<string, object> ReviewData { get; set; } = new();
        public List<string> RequiredActions { get; set; } = new();
    }
}