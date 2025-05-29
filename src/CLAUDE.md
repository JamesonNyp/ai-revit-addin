# Source Code Structure - Claude Guide

## Overview
This directory contains the main Revit add-in implementation. The project is structured as a WPF-based Revit external application.

## Project Structure
```
RevitAIAssistant/
├── Commands/           # IExternalCommand implementations
├── Models/            # Data models and DTOs
├── Services/          # Service layer (AI, Revit, Mock)
├── UI/                # User interface (XAML + ViewModels)
├── Utils/             # Helper classes and utilities
└── Resources/         # Icons, styles, and other resources
```

## Key Implementation Details

### Entry Point
- `App.cs` - Main IExternalApplication, handles initialization and ribbon UI

### Mock UI Components
- `MockAIService.cs` - Provides realistic engineering responses without backend
- `MockAIAssistantViewModel.cs` - Standalone view model for UI testing
- No backend dependencies for initial UX evaluation

### UI Architecture
- **MVVM Pattern**: All UI uses Model-View-ViewModel
- **Data Binding**: Two-way binding for reactive UI
- **Theme Support**: Adapts to Revit's light/dark themes
- **Async Operations**: All AI calls are async to prevent UI blocking

### Service Layer
```csharp
// Current mock implementation
services.AddSingleton<SessionManager>();

// Future real implementation (commented out)
// services.AddHttpClient<IAIService, AIService>()
// services.AddSingleton<IRevitContextService, RevitContextService>()
```

## Development Tips

### Adding New Commands
1. Create class implementing `IExternalCommand`
2. Add `[Transaction]` and `[Regeneration]` attributes
3. Register in ribbon UI in `App.cs`

### Adding New UI Views
1. Create XAML in `UI/Views/`
2. Create corresponding ViewModel in `UI/ViewModels/`
3. Use dependency injection for services
4. Follow existing naming conventions

### Working with Revit API
- Always check for null documents
- Use transactions for modifications
- Handle exceptions gracefully
- Test with different Revit versions

## Testing Scenarios

### Mock Responses
The `MockAIService` provides these response types:
- Task planning responses
- Calculation results
- Code compliance checks
- Status updates
- Documentation generation

### UI States to Test
- Loading states during async operations
- Error handling and display
- Theme switching
- Panel resizing and docking
- Message history scrolling

## Future Integration Points
- `AIService.cs` - Will connect to real AI platform
- `RevitContextService.cs` - Will extract real model data
- `TaskExecutionService.cs` - Will execute Revit operations
- `DocumentGenerationService.cs` - Will create real documents