param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    
    [Parameter(Mandatory=$false)]
    [string]$Platform = 'x64',
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean,
    
    [Parameter(Mandatory=$false)]
    [switch]$RunTests
)

$ErrorActionPreference = 'Stop'

# Get the solution root directory
$SolutionRoot = Split-Path -Parent $PSScriptRoot
$SolutionFile = Join-Path $SolutionRoot "RevitAIAssistant.sln"

# Function to write colored output
function Write-ColorOutput($ForegroundColor) {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = $ForegroundColor
    if ($args) {
        Write-Output $args
    }
    $host.UI.RawUI.ForegroundColor = $fc
}

Write-ColorOutput Green "========================================"
Write-ColorOutput Green "Building RevitAIAssistant Solution"
Write-ColorOutput Green "Configuration: $Configuration"
Write-ColorOutput Green "Platform: $Platform"
Write-ColorOutput Green "========================================"

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..."
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK version: $dotnetVersion"
} catch {
    Write-ColorOutput Red "ERROR: .NET SDK not found. Please install .NET 6.0 SDK or later."
    exit 1
}

# Clean if requested
if ($Clean) {
    Write-Host "Cleaning solution..."
    dotnet clean $SolutionFile -c $Configuration
    
    # Remove bin and obj directories
    Get-ChildItem -Path $SolutionRoot -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
    Write-ColorOutput Green "Clean completed."
}

# Restore NuGet packages
Write-Host "Restoring NuGet packages..."
dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput Red "ERROR: Package restore failed."
    exit 1
}
Write-ColorOutput Green "Package restore completed."

# Build the solution
Write-Host "Building solution..."
$buildArgs = @(
    "build",
    $SolutionFile,
    "-c", $Configuration,
    "-p:Platform=$Platform",
    "--no-restore"
)

if ($Configuration -eq 'Release') {
    $buildArgs += "-p:Optimize=true"
}

& dotnet $buildArgs
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput Red "ERROR: Build failed."
    exit 1
}
Write-ColorOutput Green "Build completed successfully."

# Run tests if requested
if ($RunTests) {
    Write-Host "Running tests..."
    $testProject = Join-Path $SolutionRoot "src\RevitAIAssistant.Tests\RevitAIAssistant.Tests.csproj"
    
    dotnet test $testProject -c $Configuration --no-build --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -ne 0) {
        Write-ColorOutput Red "ERROR: Tests failed."
        exit 1
    }
    Write-ColorOutput Green "All tests passed."
}

# Copy output to Revit addins folder (Debug only)
if ($Configuration -eq 'Debug') {
    Write-Host "Copying to Revit addins folder..."
    
    $revitAddinsPath = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2025\RevitAIAssistant"
    if (-not (Test-Path $revitAddinsPath)) {
        New-Item -ItemType Directory -Path $revitAddinsPath -Force | Out-Null
    }
    
    $outputPath = Join-Path $SolutionRoot "src\RevitAIAssistant\bin\Debug"
    if (Test-Path $outputPath) {
        Copy-Item -Path "$outputPath\*" -Destination $revitAddinsPath -Recurse -Force
        Write-ColorOutput Green "Files copied to: $revitAddinsPath"
    }
}

Write-ColorOutput Green "========================================"
Write-ColorOutput Green "Build completed successfully!"
Write-ColorOutput Green "========================================"

# Display output location
$outputPath = Join-Path $SolutionRoot "src\RevitAIAssistant\bin\$Configuration"
Write-Host "Output location: $outputPath"