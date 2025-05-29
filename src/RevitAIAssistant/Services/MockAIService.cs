using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RevitAIAssistant.API.Models.Requests;
using RevitAIAssistant.API.Models.Responses;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.Services
{
    /// <summary>
    /// Mock AI service for UI testing without backend
    /// </summary>
    public class MockAIService
    {
        private readonly Random _random = new();
        private readonly List<string> _mockResponses = new()
        {
            "I'll help you calculate the electrical loads for this building. Based on the selected elements, I can see you have a mixed-use office building with 847 kVA of connected load.",
            "Let me analyze the panel schedule requirements. For a building of this size, I recommend a 1200A main service with distribution panels on each floor.",
            "I've reviewed the electrical design against NEC 2020 requirements. The main areas to address are:\n\n• Voltage drop on feeder circuits exceeds 3%\n• Emergency lighting coverage needs adjustment\n• Ground fault protection coordination",
            "The HVAC load calculations show:\n\n• Total cooling load: 245 tons\n• Total heating load: 1,850 MBH\n• Outside air requirement: 12,500 CFM\n\nI recommend using a VAV system with hot water reheat for optimal efficiency.",
            "Based on my analysis, the electrical service should be sized as follows:\n\n• Main Service: 1200A @ 480/277V, 3-phase, 4-wire\n• Main Breaker: 1200A with LSI protection\n• Conductor: 4 sets of 600 kcmil copper, THHN\n• Conduit: 4-4\" PVC\n\nThis provides 20% spare capacity for future expansion."
        };

        public async Task<QueryResponse> GetMockResponse(string query)
        {
            // Simulate processing delay
            await Task.Delay(_random.Next(500, 2000));

            return new QueryResponse
            {
                Response = _mockResponses[_random.Next(_mockResponses.Count)],
                SessionId = Guid.NewGuid().ToString(),
                Metadata = new ResponseMetadata
                {
                    Confidence = 0.85 + _random.NextDouble() * 0.14,
                    ResponseType = "answer",
                    References = new List<string> 
                    { 
                        "NEC Article 220", 
                        "NEC Article 230",
                        "ASHRAE 90.1-2019"
                    },
                    RequiresReview = _random.Next(10) > 7
                }
            };
        }

        public async Task<TaskPlanResponse> GetMockTaskPlan(string description)
        {
            await Task.Delay(1500);

            var plan = new EngineeringExecutionPlan
            {
                PlanId = Guid.NewGuid().ToString(),
                Objective = "Size electrical service for office building with 847 kVA connected load",
                Complexity = PlanComplexity.Complex,
                EstimatedDuration = "8-12 minutes",
                Discipline = EngineeringDiscipline.Electrical,
                ExecutionStrategy = new ExecutionStrategy
                {
                    Approach = "Comprehensive electrical service sizing following NEC 2020 requirements with demand factor analysis and future expansion considerations.",
                    KeyConsiderations = new List<string>
                    {
                        "Apply appropriate demand factors per NEC Article 220",
                        "Consider future expansion requirements (20% minimum)",
                        "Ensure voltage drop compliance",
                        "Coordinate with utility requirements"
                    }
                },
                TaskBreakdown = new TaskBreakdown
                {
                    Methodology = "Sequential analysis with verification at each step",
                    Rationale = "Ensures accuracy and code compliance while optimizing for cost",
                    Assumptions = new List<string>
                    {
                        "Standard occupancy as per building type",
                        "Normal utility service available",
                        "No special redundancy requirements"
                    }
                },
                Tasks = new List<EngineeringTask>
                {
                    new EngineeringTask
                    {
                        StepNumber = 1,
                        Title = "Extract and Categorize Connected Loads",
                        Purpose = "Identify all electrical loads and classify by NEC category",
                        Methodology = "Scan model for electrical equipment and extract load parameters",
                        ExpectedOutcome = "Comprehensive load schedule categorized by type",
                        EstimatedTime = "1-2 minutes",
                        AssignedAgent = AssignedAgent.Electrical_System_Designer
                    },
                    new EngineeringTask
                    {
                        StepNumber = 2,
                        Title = "Apply NEC Demand Factors",
                        Purpose = "Calculate actual demand load using appropriate factors",
                        Methodology = "Apply NEC Table 220.42 for lighting and Table 220.44 for receptacles",
                        ExpectedOutcome = "Total calculated demand load",
                        EstimatedTime = "1-2 minutes",
                        AssignedAgent = AssignedAgent.Electrical_Calculations,
                        RequiresPEApproval = true
                    },
                    new EngineeringTask
                    {
                        StepNumber = 3,
                        Title = "Size Main Service Equipment",
                        Purpose = "Determine service entrance conductor and equipment sizes",
                        Methodology = "Size per NEC Article 230 with 125% continuous load factor",
                        ExpectedOutcome = "Service size, conductor size, and main breaker rating",
                        EstimatedTime = "2-3 minutes",
                        AssignedAgent = AssignedAgent.Electrical_Calculations
                    },
                    new EngineeringTask
                    {
                        StepNumber = 4,
                        Title = "Design Panel Distribution Strategy",
                        Purpose = "Layout main distribution and branch panel configuration",
                        Methodology = "Optimize for load balancing and future flexibility",
                        ExpectedOutcome = "Panel schedule with breaker assignments",
                        EstimatedTime = "2-3 minutes",
                        AssignedAgent = AssignedAgent.Electrical_System_Designer
                    },
                    new EngineeringTask
                    {
                        StepNumber = 5,
                        Title = "Verify Voltage Drop and Coordination",
                        Purpose = "Ensure system meets voltage drop limits and selectivity",
                        Methodology = "Calculate voltage drop for critical feeders, verify breaker coordination",
                        ExpectedOutcome = "Voltage drop calculations and time-current curves",
                        EstimatedTime = "1-2 minutes",
                        AssignedAgent = AssignedAgent.Electrical_QA_QC,
                        RequiresPEApproval = true
                    },
                    new EngineeringTask
                    {
                        StepNumber = 6,
                        Title = "Generate Construction Documents",
                        Purpose = "Create panel schedules and one-line diagram",
                        Methodology = "Auto-generate from design data with Revit integration",
                        ExpectedOutcome = "Complete electrical schedules and diagrams",
                        EstimatedTime = "1 minute",
                        AssignedAgent = AssignedAgent.Document_Management
                    }
                },
                ApprovalPoints = new List<ApprovalPoint>
                {
                    new ApprovalPoint
                    {
                        Description = "Demand factor calculations and load analysis",
                        ApprovalType = "Calculation Verification",
                        RequiredDocuments = new List<string> 
                        { 
                            "Load calculation worksheet",
                            "Demand factor justification",
                            "NEC code references"
                        },
                        LiabilityStatement = "The reviewing engineer certifies that the demand factors applied comply with NEC requirements and are appropriate for the building occupancy type."
                    },
                    new ApprovalPoint
                    {
                        Description = "Final service sizing and protection coordination",
                        ApprovalType = "Design Decision",
                        RequiredDocuments = new List<string>
                        {
                            "Service sizing calculations",
                            "Voltage drop analysis",
                            "Coordination study"
                        },
                        LiabilityStatement = "The reviewing engineer approves the electrical service design as safe, code-compliant, and appropriate for the intended use."
                    }
                }
            };

            return new TaskPlanResponse
            {
                TaskId = Guid.NewGuid().ToString(),
                Status = "ready",
                Plan = plan,
                EstimatedDuration = 600, // 10 minutes
                Warnings = new List<string>()
            };
        }

        public async Task<TaskStatusResponse> GetMockTaskStatus(string executionId, int callCount)
        {
            await Task.Delay(500);

            var progress = Math.Min(callCount * 15, 100);
            var steps = new[] 
            { 
                "Extracting connected loads...",
                "Applying demand factors...",
                "Sizing service equipment...",
                "Designing panel distribution...",
                "Verifying voltage drop...",
                "Generating documentation..."
            };

            var currentStep = Math.Min(callCount / 2, steps.Length - 1);
            var status = progress >= 100 ? "completed" : "in_progress";

            var response = new TaskStatusResponse
            {
                ExecutionId = executionId,
                Status = status,
                Progress = progress,
                CurrentStep = steps[currentStep],
                EstimatedTimeRemaining = Math.Max(0, 600 - (callCount * 60)),
                RequiresApproval = (progress == 30 || progress == 75) && callCount % 3 == 0
            };

            if (response.RequiresApproval)
            {
                response.ApprovalDetails = new ApprovalDetails
                {
                    ApprovalId = Guid.NewGuid().ToString(),
                    Description = progress == 30 
                        ? "Approve demand factor calculations" 
                        : "Approve final service sizing",
                    Type = "calculation_verification",
                    ReviewData = new Dictionary<string, object>
                    {
                        ["connected_load"] = 847.5,
                        ["demand_load"] = 678.0,
                        ["service_size"] = 800
                    }
                };
            }

            if (status == "completed")
            {
                response.Results = new TaskExecutionResults
                {
                    RevitCommands = new List<RevitCommand>
                    {
                        new RevitCommand
                        {
                            CommandType = "create_panel_schedule",
                            Parameters = new Dictionary<string, object>
                            {
                                ["panel_name"] = "MDP-1",
                                ["voltage"] = 480,
                                ["phases"] = 3,
                                ["main_breaker_size"] = 1200
                            }
                        }
                    },
                    Calculations = new List<CalculationResult>
                    {
                        new CalculationResult
                        {
                            Name = "Service Sizing Calculation",
                            Description = "NEC-compliant electrical service sizing",
                            Results = new Dictionary<string, double>
                            {
                                ["Connected Load"] = 847.5,
                                ["Demand Load"] = 678.0,
                                ["Service Size"] = 800,
                                ["Voltage Drop"] = 2.3
                            },
                            MeetsRequirements = true
                        }
                    }
                };
            }

            return response;
        }
    }
}