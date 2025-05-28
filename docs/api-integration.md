# API Integration Guide

## Overview

This guide explains how the Revit add-in integrates with the AI Engineering Platform API.

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│  Revit Add-in   │────▶│  API Gateway     │────▶│  Orchestrator   │
│                 │     │  (nginx)         │     │  Service        │
│  - UI Layer     │     │                  │     │                 │
│  - Services     │     │  Port: 80/443    │     │  Port: 8001     │
│  - HTTP Client  │     │                  │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

## Configuration

### appsettings.json

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8001",
    "Timeout": 30,
    "RetryCount": 3,
    "RetryDelay": 1000,
    "MaxConcurrentRequests": 5
  },
  "Authentication": {
    "Type": "ApiKey",
    "ApiKey": "your-api-key-here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "AIOrchestrationClient": "Debug"
    }
  }
}
```

## Authentication

### API Key Authentication

```csharp
// Configure HTTP client with API key
services.AddHttpClient<AIOrchestrationClient>(client =>
{
    client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
    client.DefaultRequestHeaders.Add("X-API-Key", configuration["Authentication:ApiKey"]);
});
```

### OAuth 2.0 (Enterprise)

```csharp
// Configure OAuth authentication
services.AddAuthentication("OAuth")
    .AddOAuth2("OAuth", options =>
    {
        options.ClientId = configuration["OAuth:ClientId"];
        options.ClientSecret = configuration["OAuth:ClientSecret"];
        options.AuthorizationEndpoint = "https://auth.company.com/oauth/authorize";
        options.TokenEndpoint = "https://auth.company.com/oauth/token";
    });
```

## API Client Usage

### Creating a Task Plan

```csharp
public async Task<TaskPlanResponse> CreateEngineeringTaskPlan(
    string description, 
    EngineeringContext context)
{
    var request = new TaskPlanRequest
    {
        Description = description,
        Context = context,
        Priority = "high",
        Constraints = new TaskConstraints
        {
            MaxExecutionTime = 300,
            RequiresApproval = true
        }
    };

    try
    {
        var response = await _orchestrationClient.CreateTaskPlanAsync(request);
        _logger.LogInformation("Task plan created: {TaskId}", response.TaskId);
        return response;
    }
    catch (AIOrchestrationException ex)
    {
        _logger.LogError(ex, "Failed to create task plan");
        throw;
    }
}
```

### Executing a Task

```csharp
public async Task<TaskExecutionResponse> ExecuteTask(
    string taskId, 
    ExecutionMode mode = ExecutionMode.Supervised)
{
    var request = new TaskExecutionRequest
    {
        Mode = mode,
        Parameters = new Dictionary<string, object>
        {
            ["autoApprove"] = false,
            ["notifyOnComplete"] = true
        }
    };

    var response = await _orchestrationClient.ExecuteTaskAsync(taskId, request);
    
    // Start monitoring execution
    _ = Task.Run(() => MonitorExecution(response.ExecutionId));
    
    return response;
}
```

### Command Queue Pattern

```csharp
// Queue a command for processing
public async Task QueueRevitCommand(RevitCommand command)
{
    await _orchestrationClient.QueueCommandAsync(command);
}

// Process command results
public async Task ProcessCommandResult(CommandResult result)
{
    if (result.Success)
    {
        // Apply changes to Revit model
        using (var transaction = new Transaction(_document, result.TransactionName))
        {
            transaction.Start();
            
            foreach (var modification in result.ModifiedElements)
            {
                ApplyModification(modification);
            }
            
            transaction.Commit();
        }
    }
    else
    {
        // Handle errors
        foreach (var error in result.Errors)
        {
            _logger.LogError("Command error: {Code} - {Message}", 
                error.Code, error.Message);
        }
    }
}
```

## Error Handling

### Retry Policy

```csharp
// Configure Polly retry policy
private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => !msg.IsSuccessStatusCode)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = context.Values["logger"] as ILogger;
                logger?.LogWarning(
                    "Retrying after {delay}ms, attempt {retry}",
                    timespan.TotalMilliseconds,
                    retryCount);
            });
}
```

### Exception Handling

```csharp
try
{
    var result = await _orchestrationClient.CreateTaskPlanAsync(request);
}
catch (HttpRequestException ex)
{
    // Network or HTTP errors
    _logger.LogError(ex, "Network error occurred");
    ShowUserNotification("Unable to connect to AI platform. Please check your connection.");
}
catch (TaskCanceledException ex)
{
    // Timeout
    _logger.LogError(ex, "Request timed out");
    ShowUserNotification("Request timed out. Please try again.");
}
catch (AIOrchestrationException ex)
{
    // API-specific errors
    _logger.LogError(ex, "API error: {Message}", ex.Message);
    ShowUserNotification($"API Error: {ex.Message}");
}
```

## Performance Optimization

### Connection Pooling

```csharp
// Configure HttpClient with connection pooling
services.AddHttpClient<AIOrchestrationClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        MaxConnectionsPerServer = 10,
        UseProxy = false,
        AllowAutoRedirect = false
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

### Request Batching

```csharp
public async Task<List<TaskPlanResponse>> CreateMultipleTaskPlans(
    List<TaskPlanRequest> requests)
{
    // Batch requests to avoid overwhelming the API
    const int batchSize = 5;
    var results = new List<TaskPlanResponse>();
    
    for (int i = 0; i < requests.Count; i += batchSize)
    {
        var batch = requests.Skip(i).Take(batchSize);
        var tasks = batch.Select(r => _orchestrationClient.CreateTaskPlanAsync(r));
        
        var batchResults = await Task.WhenAll(tasks);
        results.AddRange(batchResults);
        
        // Add delay between batches
        if (i + batchSize < requests.Count)
        {
            await Task.Delay(1000);
        }
    }
    
    return results;
}
```

## Monitoring and Logging

### Request Logging

```csharp
public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();
        _logger.LogInformation(
            "API Request [{RequestId}]: {Method} {Uri}",
            requestId,
            request.Method,
            request.RequestUri);
        
        var stopwatch = Stopwatch.StartNew();
        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();
        
        _logger.LogInformation(
            "API Response [{RequestId}]: {StatusCode} ({Duration}ms)",
            requestId,
            response.StatusCode,
            stopwatch.ElapsedMilliseconds);
        
        return response;
    }
}
```

### Metrics Collection

```csharp
public class MetricsHandler : DelegatingHandler
{
    private readonly IMetricsCollector _metrics;
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var timer = _metrics.StartTimer("api_request_duration");
        
        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            
            _metrics.IncrementCounter(
                "api_requests_total",
                new[] { 
                    ("method", request.Method.ToString()),
                    ("status", ((int)response.StatusCode).ToString())
                });
            
            return response;
        }
        finally
        {
            timer.Dispose();
        }
    }
}
```

## Testing

### Mock API Client

```csharp
public class MockAIOrchestrationClient : IAIOrchestrationClient
{
    private readonly List<TaskPlanResponse> _mockResponses = new();
    
    public Task<TaskPlanResponse> CreateTaskPlanAsync(
        TaskPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new TaskPlanResponse
        {
            TaskId = Guid.NewGuid().ToString(),
            Status = "ready",
            Plan = GenerateMockPlan(request),
            EstimatedDuration = 120
        };
        
        _mockResponses.Add(response);
        return Task.FromResult(response);
    }
    
    private ExecutionPlan GenerateMockPlan(TaskPlanRequest request)
    {
        // Generate mock execution plan based on request
        return new ExecutionPlan
        {
            Summary = "Mock execution plan",
            Steps = new List<ExecutionStep>
            {
                new ExecutionStep
                {
                    Id = "step1",
                    Name = "Analyze requirements",
                    Action = "analyze"
                }
            }
        };
    }
}
```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Check if the API server is running
   - Verify the BaseUrl in configuration
   - Check firewall settings

2. **401 Unauthorized**
   - Verify API key is correct
   - Check if API key has required permissions
   - Ensure authentication header is being sent

3. **Timeout Errors**
   - Increase timeout in configuration
   - Check network latency
   - Consider implementing request pagination

4. **Rate Limiting**
   - Implement exponential backoff
   - Add request throttling
   - Cache frequently accessed data

### Debug Mode

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System.Net.Http": "Trace"
    }
  }
}
```

## Best Practices

1. **Always use async/await** for API calls
2. **Implement proper error handling** with user-friendly messages
3. **Use cancellation tokens** for long-running operations
4. **Cache API responses** when appropriate
5. **Monitor API usage** and performance metrics
6. **Version your API calls** for backward compatibility
7. **Secure API keys** using configuration encryption
8. **Implement circuit breakers** for fault tolerance