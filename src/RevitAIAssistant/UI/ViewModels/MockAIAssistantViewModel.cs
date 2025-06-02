using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevitAIAssistant.API.Models.Requests;
using RevitAIAssistant.API.Models.Responses;
using RevitAIAssistant.Models;
using RevitAIAssistant.Services;
using RevitAIAssistant.UI.Commands;
using RevitAIAssistant.UI.Themes;
using RevitAIAssistant.UI.Dialogs;
using RevitAIAssistant.UI.Controls;

namespace RevitAIAssistant.UI.ViewModels
{
    /// <summary>
    /// Mock view model for UI testing without backend connection
    /// </summary>
    public class MockAIAssistantViewModel : INotifyPropertyChanged
    {
        private readonly MockAIService _mockService;
        private readonly MockOrchestrationService _orchestrationService;
        private string _inputText = string.Empty;
        private bool _isTaskExecuting;
        private ActiveTaskInfo? _activeTask;
        private ThemeColors _theme;
        private EngineeringContextViewModel _currentContext;
        private int _taskStatusCallCount = 0;
        private MockOrchestrationService.OrchestrationProcess? _activeProcess;

        public MockAIAssistantViewModel()
        {
            _mockService = new MockAIService();
            _orchestrationService = new MockOrchestrationService();
            
            // Subscribe to orchestration updates
            _orchestrationService.ProcessUpdated += OnOrchestrationProcessUpdated;
            
            // Initialize theme
            ThemeManager.Initialize();
            _theme = ThemeManager.CurrentTheme;
            ThemeManager.ThemeChanged += OnThemeChanged;
            
            // Initialize collections
            Messages = new ObservableCollection<ChatMessage>();
            
            // Initialize context with mock data
            _currentContext = new EngineeringContextViewModel
            {
                Summary = "Office Building • Electrical • Design Development",
                SelectedElementsCount = 42,
                HasActiveSystem = true,
                ActiveSystemName = "Power Distribution System"
            };
            
            // Initialize commands
            SendCommand = new RelayCommand(async () => await SendMessageAsync(), () => CanSend);
            QuickActionCommand = new RelayCommand<string>(async action => await ExecuteQuickActionAsync(action));
            ShowSettingsCommand = new RelayCommand(() => ShowSettings());
            
            // Add welcome message
            AddWelcomeMessage();
        }

        #region Properties

        public ObservableCollection<ChatMessage> Messages { get; }

        public ThemeColors Theme
        {
            get => _theme;
            private set
            {
                _theme = value;
                OnPropertyChanged();
            }
        }

        public string InputText
        {
            get => _inputText;
            set
            {
                _inputText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
                OnPropertyChanged(nameof(CanSendMessage));
                // Force command to re-evaluate CanExecute
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsTaskExecuting
        {
            get => _isTaskExecuting;
            set
            {
                _isTaskExecuting = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSend));
            }
        }

        public ActiveTaskInfo? ActiveTask
        {
            get => _activeTask;
            set
            {
                _activeTask = value;
                OnPropertyChanged();
            }
        }

        public EngineeringContextViewModel CurrentContext
        {
            get => _currentContext;
            set
            {
                _currentContext = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasContext));
            }
        }

        public bool HasContext => CurrentContext.HasContent;
        public bool CanSend => !string.IsNullOrWhiteSpace(InputText) && !IsTaskExecuting;
        public bool CanSendMessage => CanSend; // Alias for XAML binding compatibility

        #endregion

        #region Commands

        public ICommand SendCommand { get; }
        public ICommand SendMessageCommand => SendCommand; // Alias for XAML binding compatibility
        public ICommand QuickActionCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        #endregion

        #region Methods

        private async Task SendMessageAsync()
        {
            if (!CanSend) return;

            var userMessage = InputText.Trim();
            InputText = string.Empty;

            // Add user message to chat
            var userChatMessage = new ChatMessage
            {
                Role = MessageRole.User,
                Content = userMessage,
                Timestamp = DateTime.Now
            };
            Messages.Add(userChatMessage);

            // Show typing indicator
            var typingMessage = new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "Thinking...",
                IsTyping = true,
                Timestamp = DateTime.Now
            };
            Messages.Add(typingMessage);

            try
            {
                // Check if this is a task-oriented query
                if (IsTaskQuery(userMessage))
                {
                    // Get mock task plan
                    var planResponse = await _mockService.GetMockTaskPlan(userMessage);
                    
                    // Remove typing indicator
                    Messages.Remove(typingMessage);
                    
                    // Show execution plan
                    var planMessage = new ChatMessage
                    {
                        Role = MessageRole.Assistant,
                        RichContent = new ExecutionPlanContent
                        {
                            Plan = planResponse.Plan,
                            OnApprove = async () => await ExecuteTaskPlanAsync(planResponse.TaskId),
                            OnReject = () => AddSystemMessage("Task execution cancelled by user.")
                        },
                        Timestamp = DateTime.Now
                    };
                    Messages.Add(planMessage);
                }
                else
                {
                    // Simple query - get mock response
                    var response = await _mockService.GetMockResponse(userMessage);
                    
                    // Remove typing indicator
                    Messages.Remove(typingMessage);
                    
                    // Add AI response
                    var aiMessage = new ChatMessage
                    {
                        Role = MessageRole.Assistant,
                        Content = response.Response,
                        Timestamp = DateTime.Now
                    };
                    Messages.Add(aiMessage);
                }
            }
            catch (Exception ex)
            {
                // Remove typing indicator
                Messages.Remove(typingMessage);
                
                // Show error message
                AddSystemMessage($"Error: {ex.Message}. Please try again.");
            }
        }

        private async Task ExecuteQuickActionAsync(string action)
        {
            var query = action switch
            {
                "calculate_loads" => "Calculate electrical loads for the selected elements",
                "size_service" => "Size the electrical service for this building",
                "check_code" => "Check the selected electrical design against NEC requirements",
                "generate_schedules" => "Generate panel schedules for the electrical equipment",
                "qa_review" => "Perform QA/QC review on the electrical design",
                _ => throw new ArgumentException($"Unknown quick action: {action}")
            };

            InputText = query;
            await SendMessageAsync();
        }

        private async Task ExecuteTaskPlanAsync(string taskId)
        {
            try
            {
                IsTaskExecuting = true;
                _taskStatusCallCount = 0;
                
                // Start orchestration process
                var query = Messages.LastOrDefault(m => m.Role == MessageRole.User)?.Content ?? "Execute engineering task";
                _activeProcess = await _orchestrationService.StartProcessAsync(query);
                
                // Create active task tracker
                ActiveTask = new ActiveTaskInfo
                {
                    TaskId = taskId,
                    ExecutionId = _activeProcess.ProcessId,
                    Title = GetProcessTitle(_activeProcess.ProcessType),
                    Progress = 0,
                    CurrentStep = "Initializing orchestration...",
                    TimeRemaining = "Calculating..."
                };
                
                // Add orchestration message
                AddOrchestrationMessage(_activeProcess);

                // Start monitoring orchestration
                await MonitorOrchestrationAsync();
            }
            catch (Exception ex)
            {
                AddSystemMessage($"Error executing task: {ex.Message}");
            }
            finally
            {
                IsTaskExecuting = false;
                ActiveTask = null;
                _activeProcess = null;
            }
        }

        private async Task MonitorTaskExecutionAsync(string executionId)
        {
            while (IsTaskExecuting)
            {
                try
                {
                    var status = await _mockService.GetMockTaskStatus(executionId, _taskStatusCallCount++);
                    
                    if (ActiveTask != null)
                    {
                        ActiveTask.Progress = status.Progress;
                        ActiveTask.CurrentStep = status.CurrentStep;
                        ActiveTask.TimeRemaining = FormatTimeRemaining(status.EstimatedTimeRemaining);
                    }

                    // Check for approval required
                    if (status.RequiresApproval)
                    {
                        await ShowMockApprovalDialogAsync(status.ApprovalDetails);
                    }

                    // Check if complete
                    if (status.Status == "completed")
                    {
                        IsTaskExecuting = false;
                        
                        AddSystemMessage("Task completed successfully!", MessageType.Success);
                        
                        // Show results
                        if (status.Results?.Calculations?.Any() == true)
                        {
                            var calculationMessage = new ChatMessage
                            {
                                Role = MessageRole.Assistant,
                                RichContent = new CalculationResultsContent
                                {
                                    Calculations = status.Results.Calculations
                                },
                                Timestamp = DateTime.Now
                            };
                            Messages.Add(calculationMessage);
                        }
                    }

                    // Wait before next check
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    AddSystemMessage($"Error monitoring task: {ex.Message}", MessageType.Error);
                    break;
                }
            }
        }

        private async Task ShowMockApprovalDialogAsync(ApprovalDetails? details)
        {
            if (details == null) return;
            
            var approvalMessage = new ChatMessage
            {
                Role = MessageRole.System,
                Content = $"PE Approval Required: {details.Description}\n\nThis is a mock approval dialog. In the real implementation, a modal dialog would appear for PE review.",
                MessageType = MessageType.Warning,
                Timestamp = DateTime.Now
            };
            Messages.Add(approvalMessage);
            
            // Simulate approval delay
            await Task.Delay(2000);
            
            AddSystemMessage("Mock approval granted. Continuing execution...", MessageType.Success);
        }

        private bool IsTaskQuery(string query)
        {
            var taskKeywords = new[] 
            { 
                "calculate", "size", "design", "check", "verify", "review", 
                "generate", "create", "analyze", "evaluate", "determine" 
            };
            
            var lowerQuery = query.ToLower();
            return taskKeywords.Any(keyword => lowerQuery.Contains(keyword));
        }

        private string FormatTimeRemaining(int? seconds)
        {
            if (!seconds.HasValue) return "Unknown";
            
            if (seconds < 60) return $"{seconds} seconds";
            if (seconds < 3600) return $"{seconds / 60} minutes";
            
            return $"{seconds / 3600:F1} hours";
        }

        private void AddWelcomeMessage()
        {
            var welcomeMessage = new ChatMessage
            {
                Role = MessageRole.Assistant,
                Content = "Hello! I'm your AI Engineering Assistant. I can help you with:\n\n" +
                         "• Electrical load calculations\n" +
                         "• Equipment sizing and placement\n" +
                         "• Code compliance verification\n" +
                         "• Engineering design reviews\n" +
                         "• Documentation generation\n\n" +
                         "What would you like to work on today?\n\n" +
                         "*This is a mock interface for UI testing*",
                Timestamp = DateTime.Now
            };
            Messages.Add(welcomeMessage);
        }

        private void AddSystemMessage(string message, MessageType type = MessageType.Info)
        {
            var systemMessage = new ChatMessage
            {
                Role = MessageRole.System,
                Content = message,
                MessageType = type,
                Timestamp = DateTime.Now
            };
            Messages.Add(systemMessage);
        }

        private void ShowSettings()
        {
            AddSystemMessage("Settings dialog would appear here. This is a mock implementation.");
        }

        private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            Theme = e.NewTheme;
        }

        private string GetProcessTitle(string processType)
        {
            return processType switch
            {
                "electrical_load_calculation" => "Electrical Load Calculation",
                "mechanical_equipment_sizing" => "Mechanical Equipment Sizing",
                "code_compliance_review" => "Code Compliance Review",
                _ => "Engineering Task Execution"
            };
        }

        private void AddOrchestrationMessage(MockOrchestrationService.OrchestrationProcess process)
        {
            var orchestrationContent = new OrchestrationProgressContent
            {
                Process = process
            };

            var message = new ChatMessage
            {
                Role = MessageRole.Assistant,
                RichContent = orchestrationContent,
                Timestamp = DateTime.Now
            };
            Messages.Add(message);
        }

        private async Task MonitorOrchestrationAsync()
        {
            // The actual monitoring is now handled by the OnOrchestrationProcessUpdated event handler
            // This method just waits for completion
            if (_activeProcess == null) return;

            while (IsTaskExecuting && _activeProcess.OverallStatus != "completed")
            {
                await Task.Delay(500); // Just wait for completion
            }
        }

        private void AddCompletionMessage(MockOrchestrationService.OrchestrationProcess process)
        {
            AddSystemMessage("✅ Task completed successfully!", MessageType.Success);

            // Show summary of results
            var completedSteps = process.Steps.Where(s => s.Status == "completed" && !string.IsNullOrEmpty(s.Result));
            if (completedSteps.Any())
            {
                var resultsContent = new OrchestrationResultsContent
                {
                    ProcessType = GetProcessTitle(process.ProcessType),
                    ExecutionTime = DateTime.Now - process.StartTime,
                    Steps = completedSteps.ToList()
                };

                var resultsMessage = new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    RichContent = resultsContent,
                    Timestamp = DateTime.Now
                };
                Messages.Add(resultsMessage);
            }
        }

        private void OnOrchestrationProcessUpdated(object? sender, MockOrchestrationService.OrchestrationUpdateEventArgs e)
        {
            if (e.Process == null || e.Process.ProcessId != _activeProcess?.ProcessId) return;

            // Update the active process
            _activeProcess = e.Process;

            // Update the orchestration message in the chat
            var orchestrationMessage = Messages.LastOrDefault(m => m.RichContent is OrchestrationProgressContent);
            if (orchestrationMessage != null)
            {
                // Create a new content instance to force UI update
                var newContent = new OrchestrationProgressContent { Process = _activeProcess };
                orchestrationMessage.RichContent = null; // Clear first
                orchestrationMessage.RichContent = newContent; // Then set new content
            }

            // Update active task info
            if (ActiveTask != null && _activeProcess != null)
            {
                ActiveTask.Progress = _activeProcess.OverallProgress;
                
                // Find current running step
                var currentStep = _activeProcess.Steps.FirstOrDefault(s => s.Status == "running");
                if (currentStep != null)
                {
                    ActiveTask.CurrentStep = $"{currentStep.Name}: {currentStep.Description}";
                    
                    // Show sub-task progress if available
                    if (currentStep.Progress > 0 && currentStep.Progress < 100 && currentStep.SubTasks.Count > 0)
                    {
                        var subTaskIndex = Math.Min((int)(currentStep.Progress / 100.0 * currentStep.SubTasks.Count), currentStep.SubTasks.Count - 1);
                        var currentSubTask = currentStep.SubTasks.ElementAtOrDefault(subTaskIndex);
                        if (!string.IsNullOrEmpty(currentSubTask))
                        {
                            ActiveTask.CurrentStep = $"{currentStep.Name}: {currentSubTask}";
                        }
                    }
                }

                // Update time remaining
                if (_activeProcess.EstimatedEndTime.HasValue)
                {
                    var remaining = _activeProcess.EstimatedEndTime.Value - DateTime.Now;
                    ActiveTask.TimeRemaining = remaining.TotalSeconds > 0 
                        ? FormatTimeRemaining((int)remaining.TotalSeconds)
                        : "Completing...";
                }
            }

            // Check for completion
            if (_activeProcess.OverallStatus == "completed" && IsTaskExecuting)
            {
                IsTaskExecuting = false;
                AddCompletionMessage(_activeProcess);
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}