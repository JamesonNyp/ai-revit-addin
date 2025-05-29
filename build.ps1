# Build script for RevitAIAssistant
param(
    [string]$Configuration = "Debug",
    [string]$RevitVersion = "2025"
)

Write-Host "Building RevitAIAssistant for Revit $RevitVersion..." -ForegroundColor Cyan

# Set paths
$projectPath = "$PSScriptRoot\src\RevitAIAssistant\RevitAIAssistant.csproj"
$outputPath = "$PSScriptRoot\src\RevitAIAssistant\bin\$Configuration"
$addinsPath = "$env:APPDATA\Autodesk\Revit\Addins\$RevitVersion"

# Create addins directory if it doesn't exist
if (!(Test-Path $addinsPath)) {
    New-Item -ItemType Directory -Path $addinsPath -Force | Out-Null
    Write-Host "Created Revit addins directory: $addinsPath" -ForegroundColor Yellow
}

# Build the project
Write-Host "Building project..." -ForegroundColor Green
dotnet build $projectPath -c $Configuration /p:Platform=x64

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build succeeded!" -ForegroundColor Green

# Copy files to Revit addins folder
Write-Host "Installing add-in to Revit..." -ForegroundColor Green

# Copy addin manifest
Copy-Item "$PSScriptRoot\RevitAIAssistant.addin" -Destination $addinsPath -Force
Write-Host "  Copied RevitAIAssistant.addin"

# Copy DLL and dependencies
Copy-Item "$outputPath\RevitAIAssistant.dll" -Destination $addinsPath -Force
Write-Host "  Copied RevitAIAssistant.dll"

# Copy all other DLLs except Revit API DLLs
Get-ChildItem "$outputPath\*.dll" | Where-Object { 
    $_.Name -ne "RevitAPI.dll" -and 
    $_.Name -ne "RevitAPIUI.dll" -and
    $_.Name -ne "RevitAIAssistant.dll"
} | ForEach-Object {
    Copy-Item $_.FullName -Destination $addinsPath -Force
    Write-Host "  Copied $($_.Name)"
}

# Copy config files if they exist
if (Test-Path "$outputPath\appsettings.json") {
    Copy-Item "$outputPath\appsettings.json" -Destination $addinsPath -Force
    Write-Host "  Copied appsettings.json"
}

Write-Host "`nInstallation complete!" -ForegroundColor Green
Write-Host "The AI Engineering Assistant add-in has been installed to:" -ForegroundColor Cyan
Write-Host "  $addinsPath" -ForegroundColor White
Write-Host "`nRestart Revit $RevitVersion to load the add-in." -ForegroundColor Yellow