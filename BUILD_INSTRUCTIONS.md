# Build Instructions for AI Revit Add-in

## Build Error Resolution

The project has been updated to resolve the assembly version conflicts between .NET Framework 4.8 and Revit 2025's .NET 8 dependencies.

### Changes Made:

1. **Removed ApplicationDefinition element** from the project file (library projects cannot have ApplicationDefinition)

2. **Added app.config** with assembly binding redirects to handle version conflicts

3. **Downgraded Microsoft.Extensions packages** to version 5.0.x for better compatibility with .NET Framework 4.8

## Building the Project

### From Windows (Recommended)

1. **Using the Simple Batch File:**
   ```cmd
   cd C:\path\to\ai-revit-addin
   build-simple.bat
   ```

2. **Using PowerShell:**
   ```powershell
   cd C:\path\to\ai-revit-addin
   .\build.ps1 -Configuration Debug -RevitVersion 2025
   ```

3. **Using Visual Studio:**
   - Open `src\RevitAIAssistant\RevitAIAssistant.sln` in Visual Studio 2022
   - Set configuration to `Debug | x64`
   - Build the solution (Ctrl+Shift+B)

4. **Using .NET CLI (from Windows):**
   ```cmd
   cd src\RevitAIAssistant
   dotnet build -c Debug /p:Platform=x64
   ```

### After Building

The build process will automatically copy the necessary files to:
```
%APPDATA%\Autodesk\Revit\Addins\2025\
```

Files that will be copied:
- RevitAIAssistant.dll
- RevitAIAssistant.addin
- app.config
- appsettings.json
- All dependency DLLs (except RevitAPI.dll and RevitAPIUI.dll)

## Verifying the Build

1. Check that all files were copied to the Revit add-ins folder
2. Start Revit 2025
3. Look for the "AI Assistant" tab in the ribbon
4. Click "Start AI Assistant" to open the mock UI

## Troubleshooting

If you still encounter build errors:

1. **Clean the solution first:**
   ```cmd
   dotnet clean
   ```

2. **Delete the bin and obj folders:**
   ```cmd
   rmdir /s /q src\RevitAIAssistant\bin
   rmdir /s /q src\RevitAIAssistant\obj
   ```

3. **Restore NuGet packages:**
   ```cmd
   dotnet restore
   ```

4. **Build again with verbose output:**
   ```cmd
   dotnet build -c Debug /p:Platform=x64 -v detailed > build.log 2>&1
   ```

## Testing the Mock UI

Once built successfully, follow the instructions in INSTALLATION_GUIDE.md to test the mock UI with orchestration visualization.