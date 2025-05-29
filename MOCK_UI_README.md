# Revit AI Assistant - Mock UI Testing Guide

This guide will help you build and test the mock UI for the Revit AI Assistant add-in.

## Prerequisites

- Autodesk Revit 2025 installed
- .NET Framework 4.8 SDK or Visual Studio 2022
- PowerShell (for build script)

## Building the Add-in

### Option 1: Using PowerShell Build Script
```powershell
# From the ai-revit-addin directory
.\build.ps1 -Configuration Debug -RevitVersion 2025
```

### Option 2: Using Visual Studio
1. Open `src\RevitAIAssistant\RevitAIAssistant.sln` in Visual Studio 2022
2. Set configuration to Debug | x64
3. Build the solution (Ctrl+Shift+B)
4. The post-build event will automatically copy files to the Revit add-ins folder

### Option 3: Using .NET CLI
```bash
cd src/RevitAIAssistant
dotnet build -c Debug /p:Platform=x64
```

## Installation Location

The add-in files will be automatically copied to:
```
%APPDATA%\Autodesk\Revit\Addins\2025\
```

Files installed:
- `RevitAIAssistant.addin` - Add-in manifest
- `RevitAIAssistant.dll` - Main add-in assembly
- Various dependency DLLs
- `appsettings.json` - Configuration file

## Testing the Mock UI

1. **Start Revit 2025**
   - The add-in should load automatically
   - Check for any error messages during startup

2. **Access the AI Assistant**
   - Look for the "AI Assistant" tab in the Revit ribbon
   - Click "Start AI Assistant" button to open the dockable panel

3. **Mock UI Features to Test**
   - Chat interface with message input
   - Mock responses simulating AI engineering assistant
   - Theme switching (follows Revit's theme)
   - Command queue display
   - Task status indicators
   - Professional styling with company colors

4. **Test Scenarios**
   - Type various engineering queries:
     - "Calculate electrical load for the selected panel"
     - "Size mechanical equipment for this space"
     - "Check code compliance"
   - Observe mock responses and UI behavior
   - Test scrolling in chat history
   - Try resizing the dockable panel
   - Test with different Revit themes (light/dark)

## Known Limitations (Mock UI)

- No actual backend connectivity
- Mock responses are randomized from predefined list
- Task execution is simulated only
- No real Revit model interaction
- Icons are placeholder (empty images)

## Troubleshooting

### Add-in doesn't appear in Revit
1. Check if files are in the correct folder: `%APPDATA%\Autodesk\Revit\Addins\2025\`
2. Verify `RevitAIAssistant.addin` is present
3. Check Revit's journal file for loading errors

### Build errors
1. Ensure Revit 2025 is installed (needed for API references)
2. Verify .NET Framework 4.8 is installed
3. Run as Administrator if permission errors occur

### UI doesn't display correctly
1. Check for XAML parsing errors in Revit journal
2. Verify all dependency DLLs were copied
3. Try with both light and dark Revit themes

## Next Steps

Once you've evaluated the mock UI:
1. Provide feedback on UX/UI design
2. Identify any workflow improvements
3. List features to prioritize for backend integration
4. Note any Revit-specific UI conventions to follow

## Development Notes

The mock implementation includes:
- `MockAIService.cs` - Provides realistic engineering responses
- `MockAIAssistantViewModel.cs` - Standalone view model without backend dependencies
- Simplified service registration in `App.cs`
- All UI components are functional with mock data