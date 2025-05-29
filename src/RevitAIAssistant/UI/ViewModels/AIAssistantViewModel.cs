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

namespace RevitAIAssistant.UI.ViewModels
{
    /// <summary>
    /// View model for the main AI Assistant panel
    /// </summary>
    public class AIAssistantViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIAssistantViewModel> _logger;
        private readonly AIOrchestrationClient _orchestrationClient;
        private readonly SessionManager _sessionManager;
        private readonly RevitContextExtractor _contextExtractor;
        
        private string _inputText = string.Empty;
        private bool _isTaskExecuting;
        private ActiveTaskInfo? _activeTask;
        private ThemeColors _theme;
        private EngineeringContextViewModel _currentContext;

        public AIAssistantViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<AIAssistantViewModel>>();
            _orchestrationClient = serviceProvider.GetRequiredService<AIOrchestrationClient>();
            _sessionManager = serviceProvider.GetRequiredService<SessionManager>();
            _contextExtractor = serviceProvider.GetRequiredService<RevitContextExtractor>();
            
            // Initialize theme
            _theme = ThemeManager.CurrentTheme;
            ThemeManager.ThemeChanged += OnThemeChanged;
            
            // Initialize collections
            Messages = new ObservableCollection<ChatMessage>();
            
            // Initialize context
            _currentContext = new EngineeringContextViewModel();
            UpdateContext();
            
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

        #endregion

        #region Commands

        public ICommand SendCommand { get; }
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
                // Get current context
                var context = await _contextExtractor.GetCurrentContextAsync();
                
                // Check if this is a task-oriented query
                if (IsTaskQuery(userMessage))
                {
                    // Create task plan
                    var planRequest = new TaskPlanRequest
                    {
                        Description = userMessage,
                        Context = context,
                        Priority = DeterminePriority(userMessage)
                    };

                    var planResponse = await _orchestrationClient.CreateTaskPlanAsync(planRequest);
                    
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
                    // Simple query - send to AI for response
                    var response = await _orchestrationClient.SendQueryAsync(userMessage, context);
                    
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
                _logger.LogError(ex, "Error processing message");
                
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
                
                // Create execution request
                var executionRequest = new TaskExecutionRequest
                {
                    Mode = ExecutionMode.Supervised,
                    Parameters = new Dictionary<string, object>
                    {
                        ["autoApprove"] = false,
                        ["notifyOnComplete"] = true
                    }
                };

                var executionResponse = await _orchestrationClient.ExecuteTaskAsync(taskId, executionRequest);
                
                // Create active task tracker
                ActiveTask = new ActiveTaskInfo
                {
                    TaskId = taskId,
                    ExecutionId = executionResponse.ExecutionId,
                    Title = "Executing engineering task...",
                    Progress = 0,
                    CurrentStep = "Initializing...",
                    TimeRemaining = "Calculating..."
                };

                // Start monitoring execution
                await MonitorTaskExecutionAsync(executionResponse.ExecutionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing task plan");
                AddSystemMessage($"Error executing task: {ex.Message}");
            }
            finally
            {
                IsTaskExecuting = false;
                ActiveTask = null;
            }
        }

        private async Task MonitorTaskExecutionAsync(string executionId)
        {
            while (IsTaskExecuting)
            {
                try
                {
                    var status = await _orchestrationClient.GetTaskStatusAsync(executionId);
                    
                    if (ActiveTask != null)
                    {
                        ActiveTask.Progress = status.Progress;
                        ActiveTask.CurrentStep = status.CurrentStep;
                        ActiveTask.TimeRemaining = FormatTimeRemaining(status.EstimatedTimeRemaining);
                    }

                    // Check for approval required
                    if (status.RequiresApproval)
                    {
                        await ShowApprovalDialogAsync(status.ApprovalDetails);
                    }

                    // Check if complete
                    if (status.Status == "completed" || status.Status == "failed")
                    {
                        IsTaskExecuting = false;
                        
                        if (status.Status == "completed")
                        {
                            AddSystemMessage("Task completed successfully!", MessageType.Success);
                            
                            // Process results
                            if (status.Results != null)
                            {
                                await ProcessTaskResultsAsync(status.Results);
                            }
                        }
                        else
                        {
                            AddSystemMessage($"Task failed: {status.Error}", MessageType.Error);
                        }
                    }

                    // Wait before next check
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error monitoring task execution");
                    AddSystemMessage($"Error monitoring task: {ex.Message}", MessageType.Error);
                    break;
                }
            }
        }

        private async Task ProcessTaskResultsAsync(TaskExecutionResults results)
        {
            // Process Revit commands if any
            if (results.RevitCommands?.Any() == true)
            {
                foreach (var command in results.RevitCommands)
                {
                    await _orchestrationClient.QueueCommandAsync(command);
                }
                
                AddSystemMessage($"Queued {results.RevitCommands.Count} Revit commands for execution.");
            }

            // Show calculation results
            if (results.Calculations?.Any() == true)
            {
                var calculationMessage = new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    RichContent = new CalculationResultsContent
                    {
                        Calculations = results.Calculations
                    },
                    Timestamp = DateTime.Now
                };
                Messages.Add(calculationMessage);
            }

            // Show generated documentation
            if (results.Documentation != null)
            {
                var docMessage = new ChatMessage
                {
                    Role = MessageRole.Assistant,
                    RichContent = new DocumentationContent
                    {
                        Documentation = results.Documentation,
                        OnExport = async format => await ExportDocumentationAsync(results.Documentation, format)
                    },
                    Timestamp = DateTime.Now
                };
                Messages.Add(docMessage);
            }
        }

        private async Task ShowApprovalDialogAsync(ApprovalDetails details)
        {
            // This would show a modal dialog for PE approval
            // For now, just add a message
            var approvalMessage = new ChatMessage
            {
                Role = MessageRole.System,
                Content = $"PE Approval Required: {details.Description}",
                Timestamp = DateTime.Now
            };
            Messages.Add(approvalMessage);
            
            // In real implementation, this would show a dialog and wait for response
            await Task.Delay(100);
        }

        private async Task ExportDocumentationAsync(EngineeringDocumentation doc, string format)
        {
            // Export documentation in requested format
            _logger.LogInformation("Exporting documentation in {Format} format", format);
            
            // Implementation would export to file
            await Task.Delay(100);
            
            AddSystemMessage($"Documentation exported as {format}.");
        }

        private void UpdateContext()
        {
            try
            {
                var context = _contextExtractor.GetCurrentContextAsync().Result;
                CurrentContext = new EngineeringContextViewModel
                {
                    Summary = FormatContextSummary(context),
                    SelectedElementsCount = context.SelectedElements?.Count ?? 0,
                    HasActiveSystem = !string.IsNullOrEmpty(context.ActiveSystem),
                    ActiveSystemName = context.ActiveSystem ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating context");
            }
        }

        private string FormatContextSummary(EngineeringContext context)
        {
            if (string.IsNullOrEmpty(context.ProjectName))
                return "No project loaded";
                
            var parts = new List<string> { context.ProjectName };
            
            if (!string.IsNullOrEmpty(context.Discipline))
                parts.Add(context.Discipline);
                
            if (!string.IsNullOrEmpty(context.Phase))
                parts.Add(context.Phase);
                
            return string.Join(" • ", parts);
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

        private string DeterminePriority(string query)
        {
            var highPriorityKeywords = new[] { "urgent", "asap", "critical", "immediately" };
            var lowerQuery = query.ToLower();
            
            return highPriorityKeywords.Any(keyword => lowerQuery.Contains(keyword)) ? "high" : "normal";
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
                         "What would you like to work on today?",
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
            // Show settings dialog
            _logger.LogInformation("Showing settings dialog");
        }

        private void OnThemeChanged(object? sender, ThemeChangedEventArgs e)
        {
            Theme = e.NewTheme;
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

    #region Supporting Classes

    public class ChatMessage
    {
        public MessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public object? RichContent { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsTyping { get; set; }
        public MessageType MessageType { get; set; } = MessageType.Info;
    }

    public enum MessageRole
    {
        User,
        Assistant,
        System
    }

    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ActiveTaskInfo : INotifyPropertyChanged
    {
        private double _progress;
        private string _currentStep = string.Empty;
        private string _timeRemaining = string.Empty;

        public string TaskId { get; set; } = string.Empty;
        public string ExecutionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public string CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged();
            }
        }

        public string TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EngineeringContextViewModel
    {
        public string Summary { get; set; } = "No project loaded";
        public int SelectedElementsCount { get; set; }
        public bool HasActiveSystem { get; set; }
        public string ActiveSystemName { get; set; } = string.Empty;
        public bool HasContent => SelectedElementsCount > 0 || HasActiveSystem;
    }

    #endregion
}