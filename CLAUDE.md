# AI Revit Add-in - Claude Development Guide

## Project Overview
This is a Revit add-in that provides an AI-powered engineering assistant interface. It connects to a 3-layer agentic AI platform for intelligent engineering task orchestration.

## Current Development Status
- ✅ Mock UI implementation complete for UX evaluation
- ✅ Dockable panel with chat interface
- ✅ Theme integration with Revit (light/dark)
- ✅ Mock AI service with realistic engineering responses
- 🔄 Backend integration pending
- 🔄 Real task execution pending

## Key Features
- **Chat Interface**: Claude.ai-style interface adapted for engineering workflows
- **Task Orchestration**: Visual command queue and task status tracking
- **Engineering Focus**: Electrical load calculations, equipment sizing, code compliance, QA/QC
- **PE Integration**: Professional Engineer approval workflow built-in
- **Company Branding**: Uses company colors (#006F97, #1E4488, #7AA5BA)

## Architecture
```
├── src/RevitAIAssistant/     # Main add-in project
│   ├── Commands/             # Revit external commands
│   ├── Services/             # AI service integration
│   ├── UI/                   # WPF views and view models
│   ├── Models/               # Data models
│   └── Utils/                # Helper utilities
├── docs/                     # Documentation
└── tests/                    # Unit tests
```

## Development Workflow

### Building the Add-in
```powershell
# Use the build script
.\build.ps1 -Configuration Debug -RevitVersion 2025
```

### Testing Mock UI
1. Build and install the add-in
2. Start Revit 2025
3. Click "AI Assistant" tab → "Start AI Assistant"
4. Test with engineering queries

### Key Files
- `App.cs` - Main application entry point
- `AIAssistantPanel.xaml` - Main UI panel
- `MockAIService.cs` - Mock responses for testing
- `RevitAIAssistant.addin` - Add-in manifest

## Testing Commands
When testing the mock UI, try these queries:
- "Calculate electrical load for the selected panel"
- "Size the mechanical equipment for this space" 
- "Check code compliance for this electrical room"
- "Generate documentation for the selected system"
- "Review and validate the electrical calculations"

## Backend Integration Notes
The add-in expects the AI platform at `http://localhost:8001`. When ready to integrate:
1. Uncomment real service registration in `App.cs`
2. Ensure the AI platform is running
3. Update `appsettings.json` with correct endpoints

## Important Conventions
- Follow existing code style and patterns
- Use MVVM pattern for all UI components
- Maintain theme consistency with Revit
- Always check for null references with Revit API
- Use transactions for any model modifications

## Common Issues
- **Add-in not loading**: Check `%APPDATA%\Autodesk\Revit\Addins\2025\`
- **UI not displaying**: Verify all DLLs were copied during build
- **Theme issues**: Test with both light and dark Revit themes
- **Build errors**: Ensure Revit 2025 is installed for API references