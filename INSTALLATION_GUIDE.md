# Revit AI Assistant - Installation & Testing Guide

## Prerequisites

Before installing the AI Assistant add-in, ensure you have:

- **Autodesk Revit 2025** installed
- **.NET Framework 4.8** or higher
- **Windows 10/11** (64-bit)
- Administrator access (for first-time installation)

## Installation Steps

### Method 1: Using PowerShell Script (Recommended)

1. **Download or Clone the Repository**
   ```bash
   git clone https://github.com/JamesonNyp/ai-revit-addin.git
   cd ai-revit-addin
   ```

2. **Run the Build Script**
   - Open PowerShell as Administrator
   - Navigate to the project directory
   - Execute the build script:
   ```powershell
   .\build.ps1 -Configuration Debug -RevitVersion 2025
   ```

3. **Verify Installation**
   - The script will automatically copy files to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
   - You should see confirmation messages for each copied file

### Method 2: Manual Installation

1. **Build the Project**
   - Open `src\RevitAIAssistant\RevitAIAssistant.sln` in Visual Studio 2022
   - Set configuration to `Debug | x64`
   - Build the solution (Ctrl+Shift+B)

2. **Copy Files Manually**
   - Navigate to `src\RevitAIAssistant\bin\Debug\`
   - Copy the following files to `%APPDATA%\Autodesk\Revit\Addins\2025\`:
     - `RevitAIAssistant.dll`
     - `RevitAIAssistant.addin`
     - All dependency DLLs (except RevitAPI.dll and RevitAPIUI.dll)
     - `appsettings.json`

### Method 3: Using .NET CLI

1. **Build from Command Line**
   ```bash
   cd src/RevitAIAssistant
   dotnet build -c Debug /p:Platform=x64
   ```

2. **Copy Output**
   - Files will be in `bin\Debug\`
   - Copy to Revit add-ins folder as described above

## Testing the Mock UI

### 1. Launch Revit

- Start Autodesk Revit 2025
- Wait for all add-ins to load
- Check for any error messages

### 2. Access the AI Assistant

- Look for the **"AI Assistant"** tab in the Revit ribbon
- You should see two buttons:
  - **Start AI Assistant** - Opens the main panel
  - **Manage Tasks** - Shows task management (placeholder)

### 3. Open the AI Assistant Panel

- Click **"Start AI Assistant"**
- A dockable panel should appear on the right side
- The panel can be docked, floated, or resized

### 4. Test Basic Chat Functionality

Try these simple queries first:
- "Hello"
- "What can you help me with?"
- "Explain electrical load calculations"

### 5. Test Multi-Step Orchestration

These queries trigger the 2-4 minute orchestration visualization:

**Electrical Load Calculation:**
```
Calculate electrical load for the selected panel
```
Watch for:
- 5-step process (Context Analysis → Load Calculation → Code Compliance → QA/QC → Documentation)
- Different specialist agents handling each step
- Real-time progress updates
- Detailed results at completion

**Mechanical Equipment Sizing:**
```
Size mechanical equipment for this space
```
Watch for:
- Space analysis and load calculations
- Equipment selection process
- Energy efficiency analysis
- Model update simulation

**Code Compliance Review:**
```
Check code compliance for the electrical design
```
Watch for:
- Element identification
- Multi-code analysis (NEC, ASHRAE, local codes)
- Issue categorization
- Report generation

### 6. Test UI Features

**Theme Testing:**
- Switch Revit between Light and Dark themes
- The AI Assistant should adapt automatically

**Panel Interactions:**
- Resize the dockable panel
- Dock/undock the panel
- Minimize and restore

**Chat Features:**
- Test message scrolling with multiple messages
- Check timestamp display
- Verify message formatting

### 7. Quick Actions

Use the quick action buttons (if visible):
- Calculate Loads
- Size Service
- Check Code
- Generate Schedules
- QA Review

## Expected Behavior

### During Orchestration:
1. **Execution Plan** appears with approve/cancel buttons
2. **Progress Visualization** shows after approval:
   - Overall progress bar
   - Step-by-step status updates
   - Current sub-task display
   - Time remaining estimate
3. **Real-time Updates** every few seconds
4. **Results Summary** after 2-4 minutes

### Visual Elements:
- Company colors: #006F97 (primary), #1E4488, #7AA5BA
- Status indicators: ⏸️ pending, ⚙️ running, ✅ completed
- Agent badges showing which specialist is working
- Color-coded results: ✓ success, ⚠ warning, 🔴 critical

## Troubleshooting

### Add-in Doesn't Appear

1. **Check Installation Path**
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```
   Should contain:
   - RevitAIAssistant.addin
   - RevitAIAssistant.dll

2. **Verify .addin File**
   - Open RevitAIAssistant.addin in a text editor
   - Check that Assembly path is correct

3. **Check Revit Journal**
   - Look in: `%LOCALAPPDATA%\Autodesk\Revit\Autodesk Revit 2025\Journals\`
   - Open the latest journal file
   - Search for "RevitAIAssistant" for error messages

### UI Issues

1. **Panel Doesn't Open**
   - Try restarting Revit
   - Check for error dialogs
   - Verify all DLLs were copied

2. **Theme Not Updating**
   - Close and reopen the panel
   - Restart Revit with desired theme

3. **Missing Icons**
   - Icons are currently placeholders (empty)
   - This is expected behavior

### Build Errors

1. **Revit API Not Found**
   - Ensure Revit 2025 is installed
   - Check that RevitAPI.dll exists in Revit program folder

2. **Target Framework Issues**
   - Verify .NET Framework 4.8 is installed
   - Use Visual Studio 2022 or newer

## Uninstalling

To remove the add-in:

1. Close Revit
2. Navigate to `%APPDATA%\Autodesk\Revit\Addins\2025\`
3. Delete:
   - RevitAIAssistant.addin
   - RevitAIAssistant.dll
   - All related DLL files

## Next Steps

After successful testing:

1. **Provide Feedback** on:
   - UI/UX design
   - Workflow effectiveness
   - Feature requests
   - Any issues encountered

2. **Future Development**:
   - Backend integration with AI platform
   - Real Revit model interaction
   - Actual task execution
   - Professional icons and branding

## Support

For issues or questions:
- Check the [GitHub repository](https://github.com/JamesonNyp/ai-revit-addin)
- Review the MOCK_UI_README.md for additional details
- Report issues through GitHub Issues

---

**Note:** This is a mock UI for evaluation purposes. No actual engineering calculations or model modifications are performed.