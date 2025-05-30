# AI Revit Add-in

## Overview

The AI Revit Add-in is a sophisticated engineering assistant that integrates directly with Autodesk Revit 2025. It connects to the AI Engineering Platform to provide intelligent task planning, automated calculations, and engineering workflow optimization.

### Key Features

- **Intelligent Task Planning**: AI-powered analysis of engineering requirements
- **Automated Calculations**: Electrical and mechanical system calculations
- **Real-time Collaboration**: Integration with cross-disciplinary coordination
- **Command Queue Pattern**: Reliable task execution and status tracking
- **Context-Aware Assistance**: Understands current Revit model state

## Architecture

```
┌─────────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│   Revit 2025    │     │  AI Revit Add-in    │     │  AI Engineering  │
│                 │────▶│                     │────▶│    Platform      │
│  - Model Data   │     │  - UI/Commands      │     │                  │
│  - User Actions │     │  - Context Extract  │     │  - Orchestrator  │
│  - API Events   │     │  - Task Execution   │     │  - Specialists   │
└─────────────────┘     └─────────────────────┘     └──────────────────┘
```

### Technology Stack

- **Framework**: .NET 8.0 Windows
- **UI**: WPF (Windows Presentation Foundation)
- **Revit API**: Autodesk Revit 2025 SDK
- **HTTP Client**: Microsoft.Extensions.Http
- **Serialization**: Newtonsoft.Json / System.Text.Json
- **Testing**: xUnit, Moq

## Project Structure

```
ai-revit-addin/
├── src/
│   ├── RevitAIAssistant/          # Main Revit add-in
│   ├── RevitAIAssistant.API/      # HTTP client library
│   └── RevitAIAssistant.Tests/    # Unit and integration tests
├── docs/                          # Documentation
├── scripts/                       # Build and deployment scripts
└── tools/                         # Development utilities
```

## Development Setup

### Prerequisites

1. **Visual Studio 2022** or later
   - .NET desktop development workload
   - .NET 8.0 SDK

2. **Autodesk Revit 2025**
   - Developer mode enabled
   - Valid license

3. **Revit SDK 2025**
   - Download from [Autodesk Developer Network](https://www.autodesk.com/developer-network/platform-technologies/revit)

### Initial Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/[your-org]/ai-revit-addin.git
   cd ai-revit-addin
   ```

2. Run the development setup script:
   ```powershell
   .\scripts\dev-setup.ps1
   ```

3. Open the solution in Visual Studio:
   ```bash
   start RevitAIAssistant.sln
   ```

4. Configure API endpoint:
   - Copy `appsettings.example.json` to `appsettings.json`
   - Update the `ApiBaseUrl` to point to your AI Engineering Platform instance

### Building the Project

```powershell
# Build for development
.\scripts\build.ps1 -Configuration Debug

# Build for release
.\scripts\build.ps1 -Configuration Release
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

## Deployment

### Local Development Deployment

1. Build the project in Debug mode
2. The add-in will be automatically copied to:
   ```
   %APPDATA%\Autodesk\Revit\Addins\2025\
   ```

3. Start Revit 2025 - the add-in will load automatically

### Production Deployment

1. Build the project in Release mode:
   ```powershell
   .\scripts\build.ps1 -Configuration Release
   ```

2. Create the installer package:
   ```powershell
   .\scripts\package.ps1
   ```

3. The MSI installer will be created in `dist/`

### Manual Installation

1. Copy the built assemblies to the Revit addins folder
2. Copy the `.addin` manifest file
3. Restart Revit

## API Integration

### Configuration

The add-in uses a configuration file for API settings:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:8001",
    "Timeout": 30,
    "RetryCount": 3,
    "ApiKey": "your-api-key-here"
  },
  "Features": {
    "EnableAutoSync": true,
    "EnableOfflineMode": false,
    "CacheTimeout": 300
  }
}
```

### Authentication

The add-in supports multiple authentication methods:
- API Key authentication (default)
- OAuth 2.0 (enterprise deployments)
- Windows Integrated Authentication

### Command Queue Pattern

The add-in implements a reliable command queue pattern:

1. User actions generate commands
2. Commands are queued locally
3. Commands are sent to the platform
4. Platform processes and returns results
5. Add-in executes Revit modifications
6. Status updates are synchronized

## Usage Guide

### Starting the AI Assistant

1. Open a Revit project
2. Navigate to the AI Assistant tab in the ribbon
3. Click "Start AI Assistant"
4. The assistant panel will dock to the side

### Creating Tasks

1. Click "New Task" or use the chat interface
2. Describe your engineering requirement
3. The AI will analyze and create an execution plan
4. Review and approve the plan
5. Monitor execution progress

### Task Types Supported

- **Electrical Design**: Load calculations, panel schedules, circuit routing
- **Mechanical Design**: HVAC calculations, duct sizing, equipment selection
- **Coordination**: Clash detection, system integration, change management
- **Documentation**: Drawing generation, specification updates, report creation

## Development Guidelines

### Code Style

- Follow C# coding conventions
- Use meaningful names for all identifiers
- Document public APIs with XML comments
- Implement proper error handling

### Git Workflow

1. Create feature branch from `develop`
2. Make changes and test thoroughly
3. Submit pull request with description
4. Ensure CI/CD passes
5. Merge after code review

### Testing Requirements

- Unit test coverage > 80%
- Integration tests for all API endpoints
- UI automation tests for critical workflows
- Performance benchmarks for large models

## Troubleshooting

### Common Issues

1. **Add-in not loading**
   - Check Revit version compatibility
   - Verify .addin file is in correct location
   - Check assembly dependencies

2. **API connection failures**
   - Verify network connectivity
   - Check API endpoint configuration
   - Review firewall settings

3. **Performance issues**
   - Enable caching in settings
   - Check model complexity
   - Review log files for bottlenecks

### Debug Mode

Enable debug logging:
```xml
<configuration>
  <system.diagnostics>
    <sources>
      <source name="RevitAIAssistant" switchValue="Verbose">
        <listeners>
          <add name="file" />
        </listeners>
      </source>
    </sources>
  </system.diagnostics>
</configuration>
```

## Support

- **Documentation**: See `/docs` folder
- **Issues**: GitHub Issues
- **Email**: support@engineering-ai.com
- **Wiki**: [Internal Wiki](https://wiki.company.com/ai-revit-addin)

## License

MIT License - See LICENSE file for details

## Contributors

- Engineering AI Team
- Revit Integration Team
- QA and Testing Team

---

**Version**: 1.0.0  
**Last Updated**: December 2024