using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RevitAIAssistant.Services;
using RevitAIAssistant.UI.Views;

namespace RevitAIAssistant
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class App : IExternalApplication
    {
        private static IServiceProvider? _serviceProvider;
        private const string DockablePaneGuid = "7F8B8C5D-4A9E-4B8C-9D7E-6F8A9B5C7D4E";

        public static IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized");

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Initialize dependency injection
                InitializeServices();

                // Create ribbon tab and panels
                CreateRibbonUI(application);

                // Register dockable pane
                RegisterDockablePane(application);

                // Subscribe to application events
                SubscribeToEvents(application);

                var logger = ServiceProvider.GetRequiredService<ILogger<App>>();
                logger.LogInformation("RevitAIAssistant successfully loaded");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("AI Assistant Error", $"Failed to initialize AI Assistant: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                // Cleanup services
                if (_serviceProvider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("AI Assistant Error", $"Error during shutdown: {ex.Message}");
                return Result.Failed;
            }
        }

        private void InitializeServices()
        {
            var services = new ServiceCollection();

            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // For mock UI, just register minimal services
            services.AddSingleton<SessionManager>();

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();
        }

        private void CreateRibbonUI(UIControlledApplication application)
        {
            string tabName = "AI Assistant";
            
            // Create ribbon tab
            application.CreateRibbonTab(tabName);

            // Create ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "AI Tools");

            // Get assembly path
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;

            // Create Start AI Assistant button
            PushButtonData startButtonData = new PushButtonData(
                "StartAIAssistant",
                "Start AI\nAssistant",
                thisAssemblyPath,
                "RevitAIAssistant.Commands.StartAIAssistantCommand")
            {
                ToolTip = "Launch the AI Engineering Assistant",
                LongDescription = "Opens the AI Assistant panel for intelligent engineering task planning and execution.",
                Image = LoadImage("ai_assistant_16.png"),
                LargeImage = LoadImage("ai_assistant_32.png")
            };

            // Create Manage Tasks button
            PushButtonData manageTasksButtonData = new PushButtonData(
                "ManageTasks",
                "Manage\nTasks",
                thisAssemblyPath,
                "RevitAIAssistant.Commands.ManageTasksCommand")
            {
                ToolTip = "View and manage AI-generated tasks",
                LongDescription = "Opens the task management dialog to view, modify, and execute engineering tasks.",
                Image = LoadImage("tasks_16.png"),
                LargeImage = LoadImage("tasks_32.png")
            };

            // Add buttons to panel
            ribbonPanel.AddItem(startButtonData);
            ribbonPanel.AddItem(manageTasksButtonData);
        }

        private void RegisterDockablePane(UIControlledApplication application)
        {
            var dpid = new DockablePaneId(new Guid(DockablePaneGuid));
            
            var dockablePaneProvider = new AIAssistantPaneProvider();
            
            application.RegisterDockablePane(dpid, "AI Engineering Assistant", dockablePaneProvider);
        }

        private void SubscribeToEvents(UIControlledApplication application)
        {
            // Subscribe to document events for context awareness
            application.ControlledApplication.DocumentOpened += OnDocumentOpened;
            application.ControlledApplication.DocumentClosed += OnDocumentClosed;
        }

        private void OnDocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            if (ServiceProvider != null)
            {
                var logger = ServiceProvider.GetService<ILogger<App>>();
                logger?.LogInformation("Document opened: {DocumentTitle}", e.Document?.Title ?? "Unknown");
                
                // Update context in session manager
                var sessionManager = ServiceProvider.GetService<SessionManager>();
                sessionManager?.UpdateDocumentContext(e.Document);
            }
        }

        private void OnDocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
        {
            if (ServiceProvider != null)
            {
                var logger = ServiceProvider.GetService<ILogger<App>>();
                logger?.LogInformation("Document closed");
                
                // Clear context in session manager
                var sessionManager = ServiceProvider.GetService<SessionManager>();
                sessionManager?.ClearDocumentContext();
            }
        }

        // Removed OnViewActivated for simplicity in mock

        private BitmapImage LoadImage(string imageName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"RevitAIAssistant.Resources.Icons.{imageName}";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = stream;
                        image.EndInit();
                        return image;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider?.GetService<ILogger<App>>();
                logger?.LogWarning("Failed to load image {ImageName}: {Error}", imageName, ex.Message);
            }

            // Return empty image if loading fails
            return new BitmapImage();
        }

        private string GetApiBaseUrl()
        {
            // TODO: Load from configuration
            return "http://localhost:8001";
        }
    }
}