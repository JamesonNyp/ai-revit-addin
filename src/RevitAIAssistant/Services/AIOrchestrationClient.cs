using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using RevitAIAssistant.API.Models.Requests;
using RevitAIAssistant.API.Models.Responses;
using RevitAIAssistant.Models;

namespace RevitAIAssistant.Services
{
    /// <summary>
    /// HTTP client for communicating with the AI Engineering Platform
    /// Implements command queue pattern for reliable task execution
    /// </summary>
    public class AIOrchestrationClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIOrchestrationClient> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
        private readonly Queue<PendingCommand> _commandQueue;
        private readonly SemaphoreSlim _commandQueueLock;
        private CancellationTokenSource? _processingCancellation;

        public AIOrchestrationClient(HttpClient httpClient, ILogger<AIOrchestrationClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandQueue = new Queue<PendingCommand>();
            _commandQueueLock = new SemaphoreSlim(1, 1);

            // Configure retry policy with exponential backoff
            _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        _logger.LogWarning("Retrying request after {Delay}ms (attempt {RetryCount})", 
                            timespan.TotalMilliseconds, retryCount);
                    });
        }

        /// <summary>
        /// Create a new engineering task execution plan
        /// </summary>
        public async Task<TaskPlanResponse> CreateTaskPlanAsync(TaskPlanRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating task plan for: {Description}", request.Description);

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/v1/tasks/plan", content, cancellationToken));

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var taskPlan = JsonConvert.DeserializeObject<TaskPlanResponse>(responseJson);

                if (taskPlan == null)
                {
                    throw new InvalidOperationException("Received null task plan from server");
                }

                _logger.LogInformation("Task plan created with ID: {TaskId}", taskPlan.TaskId);
                return taskPlan;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while creating task plan");
                throw new AIOrchestrationException("Failed to communicate with AI platform", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while creating task plan");
                throw new AIOrchestrationException("Request timed out", ex);
            }
        }

        /// <summary>
        /// Execute a task plan
        /// </summary>
        public async Task<TaskExecutionResponse> ExecuteTaskAsync(string taskId, TaskExecutionRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Executing task: {TaskId}", taskId);

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync($"/api/v1/tasks/{taskId}/execute", content, cancellationToken));

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var executionResponse = JsonConvert.DeserializeObject<TaskExecutionResponse>(responseJson);

                if (executionResponse == null)
                {
                    throw new InvalidOperationException("Received null execution response from server");
                }

                _logger.LogInformation("Task execution started: {ExecutionId}", executionResponse.ExecutionId);
                return executionResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while executing task");
                throw new AIOrchestrationException("Failed to communicate with AI platform", ex);
            }
        }

        /// <summary>
        /// Get task execution status
        /// </summary>
        public async Task<TaskStatusResponse> GetTaskStatusAsync(string executionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"/api/v1/executions/{executionId}/status", cancellationToken));

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var statusResponse = JsonConvert.DeserializeObject<TaskStatusResponse>(responseJson);

                if (statusResponse == null)
                {
                    throw new InvalidOperationException("Received null status response from server");
                }

                return statusResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while getting task status");
                throw new AIOrchestrationException("Failed to communicate with AI platform", ex);
            }
        }

        /// <summary>
        /// Queue a Revit command for execution
        /// </summary>
        public async Task QueueCommandAsync(RevitCommand command)
        {
            await _commandQueueLock.WaitAsync();
            try
            {
                var pendingCommand = new PendingCommand
                {
                    Id = Guid.NewGuid().ToString(),
                    Command = command,
                    QueuedAt = DateTime.UtcNow,
                    Status = CommandStatus.Queued
                };

                _commandQueue.Enqueue(pendingCommand);
                _logger.LogInformation("Command queued: {CommandType} (ID: {CommandId})", 
                    command.CommandType, pendingCommand.Id);

                // Start processing if not already running
                if (_processingCancellation == null || _processingCancellation.IsCancellationRequested)
                {
                    StartCommandProcessing();
                }
            }
            finally
            {
                _commandQueueLock.Release();
            }
        }

        /// <summary>
        /// Get queued commands
        /// </summary>
        public async Task<IReadOnlyList<PendingCommand>> GetQueuedCommandsAsync()
        {
            await _commandQueueLock.WaitAsync();
            try
            {
                return _commandQueue.ToArray();
            }
            finally
            {
                _commandQueueLock.Release();
            }
        }

        /// <summary>
        /// Start processing queued commands
        /// </summary>
        private void StartCommandProcessing()
        {
            _processingCancellation = new CancellationTokenSource();
            var cancellationToken = _processingCancellation.Token;

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    PendingCommand? command = null;

                    await _commandQueueLock.WaitAsync(cancellationToken);
                    try
                    {
                        if (_commandQueue.Count > 0)
                        {
                            command = _commandQueue.Dequeue();
                        }
                    }
                    finally
                    {
                        _commandQueueLock.Release();
                    }

                    if (command != null)
                    {
                        await ProcessCommandAsync(command, cancellationToken);
                    }
                    else
                    {
                        // No commands to process, wait a bit
                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Process a single command
        /// </summary>
        private async Task ProcessCommandAsync(PendingCommand pendingCommand, CancellationToken cancellationToken)
        {
            try
            {
                pendingCommand.Status = CommandStatus.Processing;
                _logger.LogInformation("Processing command: {CommandId}", pendingCommand.Id);

                // Send command to platform for processing
                var request = new CommandProcessingRequest
                {
                    CommandId = pendingCommand.Id,
                    Command = pendingCommand.Command,
                    Context = GetCurrentContext()
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/v1/commands/process", content, cancellationToken));

                response.EnsureSuccessStatusCode();

                pendingCommand.Status = CommandStatus.Completed;
                pendingCommand.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Command processed successfully: {CommandId}", pendingCommand.Id);
            }
            catch (Exception ex)
            {
                pendingCommand.Status = CommandStatus.Failed;
                pendingCommand.Error = ex.Message;
                pendingCommand.CompletedAt = DateTime.UtcNow;

                _logger.LogError(ex, "Failed to process command: {CommandId}", pendingCommand.Id);
            }
        }

        /// <summary>
        /// Send a general query to the AI platform
        /// </summary>
        public async Task<QueryResponse> SendQueryAsync(string query, EngineeringContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending query to AI platform: {Query}", query);

                var request = new QueryRequest
                {
                    Query = query,
                    Context = context,
                    SessionId = Guid.NewGuid().ToString()
                };

                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.PostAsync("/api/v1/query", content, cancellationToken));

                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(responseJson);

                if (queryResponse == null)
                {
                    throw new InvalidOperationException("Received null response from server");
                }

                return queryResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while sending query");
                throw new AIOrchestrationException("Failed to communicate with AI platform", ex);
            }
        }

        /// <summary>
        /// Get current Revit context
        /// </summary>
        private EngineeringContext GetCurrentContext()
        {
            // This would be populated from the RevitContextExtractor service
            return new EngineeringContext
            {
                ProjectName = "Current Project",
                Discipline = "MEP",
                Phase = "Design Development",
                Standards = new List<string> { "NEC 2020", "ASHRAE 90.1" }
            };
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            _processingCancellation?.Cancel();
            _processingCancellation?.Dispose();
            _commandQueueLock?.Dispose();
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Exception thrown by AI orchestration operations
    /// </summary>
    public class AIOrchestrationException : Exception
    {
        public AIOrchestrationException(string message) : base(message) { }
        public AIOrchestrationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Represents a command pending execution
    /// </summary>
    public class PendingCommand
    {
        public string Id { get; set; } = string.Empty;
        public RevitCommand Command { get; set; } = new();
        public DateTime QueuedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public CommandStatus Status { get; set; }
        public string? Error { get; set; }
    }

    /// <summary>
    /// Command execution status
    /// </summary>
    public enum CommandStatus
    {
        Queued,
        Processing,
        Completed,
        Failed
    }
}