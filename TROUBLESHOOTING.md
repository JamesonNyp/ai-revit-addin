# Troubleshooting Build Issues

## Duplicate 'Page' Items Error (NETSDK1022)

If you encounter this error when building:
```
error NETSDK1022: Duplicate 'Page' items were included
```

**Solution:** The project file has been updated to disable default page items. Pull the latest changes:
```bash
git pull origin main
```

## Alternative Build Methods

### Method 1: Simple Batch File (Windows)
Use the simplified batch file that doesn't require PowerShell:
```cmd
build-simple.bat
```

### Method 2: Direct .NET CLI Build
Build directly from the project directory:
```bash
cd src\RevitAIAssistant
dotnet build -c Debug /p:Platform=x64
```

Then manually copy files from `bin\Debug\` to `%APPDATA%\Autodesk\Revit\Addins\2025\`

### Method 3: Visual Studio Build
1. Open `src\RevitAIAssistant\RevitAIAssistant.sln` in Visual Studio
2. Right-click the solution and select "Restore NuGet Packages"
3. Set configuration to `Debug | x64`
4. Build (F6 or Ctrl+Shift+B)

## Common Build Issues

### .NET SDK Version Mismatch
The project targets .NET Framework 4.8 (not .NET Core/5/6/7/8/9).

**Solution:** Ensure you have .NET Framework 4.8 Developer Pack installed:
- Download from: https://dotnet.microsoft.com/download/dotnet-framework/net48

### Missing Revit API References
If you see errors about missing RevitAPI or RevitAPIUI:

**Solution:** Ensure Revit 2025 is installed. The project references:
- `C:\Program Files\Autodesk\Revit 2025\RevitAPI.dll`
- `C:\Program Files\Autodesk\Revit 2025\RevitAPIUI.dll`

### Platform Target Issues
Ensure you're building for x64 platform:
```bash
dotnet build -c Debug /p:Platform=x64
```

## Manual Installation Steps

If automated build fails, you can manually install:

1. **Download Pre-built Files** (if available) or build locally
2. **Copy to Revit Add-ins Folder:**
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
3. **Required Files:**
   - `RevitAIAssistant.addin`
   - `RevitAIAssistant.dll`
   - All dependency DLLs from build output
   - `appsettings.json`

## Verifying Installation

1. **Check Files Exist:**
   ```cmd
   dir "%APPDATA%\Autodesk\Revit\Addins\2025\RevitAI*"
   ```

2. **Check Revit Journal:**
   After starting Revit, check the journal file for loading errors:
   ```
   %LOCALAPPDATA%\Autodesk\Revit\Autodesk Revit 2025\Journals\
   ```
   Search for "RevitAIAssistant"

3. **Enable Add-in Loading:**
   If Revit blocks the add-in:
   - Go to Revit Options → User Interface → Check "Enable loading of unsigned add-ins"

## Getting Help

If issues persist:
1. Check the [GitHub Issues](https://github.com/JamesonNyp/ai-revit-addin/issues)
2. Provide:
   - Exact error message
   - Build output
   - .NET SDK version: `dotnet --version`
   - Revit version
   - Windows version