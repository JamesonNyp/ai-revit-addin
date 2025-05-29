# Tests - Claude Guide

## Overview
This directory will contain unit tests and integration tests for the AI Revit Add-in. Currently focused on planning test structure for future implementation.

## Test Structure (Planned)
```
tests/
├── Unit/               # Unit tests for individual components
├── Integration/        # Integration tests with Revit API
├── UI/                # UI automation tests
└── Mocks/             # Shared mock objects
```

## Testing Strategy

### Unit Tests
Focus areas for unit testing:
- View models logic
- Service layer methods
- Data model validation
- Utility functions
- Command execution logic

### Integration Tests
Key integration points to test:
- Revit API interactions
- Add-in loading process
- Ribbon UI creation
- Dockable pane registration
- Document event handling

### UI Tests
UI elements requiring testing:
- Chat message flow
- Theme switching
- Async operation feedback
- Error message display
- Panel docking/undocking

## Test Implementation Examples

### ViewModel Test
```csharp
[Test]
public async Task SendMessage_ShouldAddUserAndAIMessages()
{
    // Arrange
    var mockService = new Mock<IAIService>();
    var viewModel = new AIAssistantViewModel(mockService.Object);
    
    // Act
    await viewModel.SendMessageCommand.ExecuteAsync("Test query");
    
    // Assert
    Assert.AreEqual(2, viewModel.Messages.Count);
    Assert.AreEqual("Test query", viewModel.Messages[0].Content);
}
```

### Mock Service Test
```csharp
[Test]
public async Task MockAIService_ShouldReturnValidResponse()
{
    // Arrange
    var service = new MockAIService();
    
    // Act
    var response = await service.ProcessQueryAsync("Calculate load");
    
    // Assert
    Assert.IsNotNull(response);
    Assert.IsNotEmpty(response.Response);
}
```

## Testing Best Practices
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Mock external dependencies
- Test edge cases and error conditions
- Keep tests isolated and independent

## Future Testing Needs
- Performance benchmarks
- Load testing for concurrent operations
- Revit version compatibility tests
- Memory leak detection
- Security testing for API calls