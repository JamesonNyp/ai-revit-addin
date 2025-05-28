using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RevitAIAssistant.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StartAIAssistantCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var logger = App.ServiceProvider.GetRequiredService<ILogger<StartAIAssistantCommand>>();
                logger.LogInformation("Starting AI Assistant command executed");

                // Get the UIApplication
                UIApplication uiApp = commandData.Application;
                UIDocument uiDoc = uiApp.ActiveUIDocument;

                if (uiDoc == null)
                {
                    TaskDialog.Show("AI Assistant", "Please open a Revit project first.");
                    return Result.Cancelled;
                }

                // Show the dockable pane
                var dpid = new DockablePaneId(new Guid("7F8B8C5D-4A9E-4B8C-9D7E-6F8A9B5C7D4E"));
                DockablePane dockablePane = uiApp.GetDockablePane(dpid);

                if (dockablePane != null)
                {
                    if (!dockablePane.IsShown())
                    {
                        dockablePane.Show();
                        logger.LogInformation("AI Assistant panel shown");
                    }
                    else
                    {
                        // If already shown, just bring it to focus
                        dockablePane.Hide();
                        dockablePane.Show();
                        logger.LogInformation("AI Assistant panel brought to focus");
                    }
                }
                else
                {
                    TaskDialog.Show("AI Assistant Error", "Failed to find AI Assistant panel. Please restart Revit.");
                    return Result.Failed;
                }

                // Update context with current document
                var sessionManager = App.ServiceProvider.GetRequiredService<SessionManager>();
                sessionManager.UpdateDocumentContext(uiDoc.Document);
                sessionManager.UpdateViewContext(uiDoc.ActiveView);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = $"Error starting AI Assistant: {ex.Message}";
                
                var logger = App.ServiceProvider?.GetService<ILogger<StartAIAssistantCommand>>();
                logger?.LogError(ex, "Failed to start AI Assistant");
                
                TaskDialog.Show("AI Assistant Error", message);
                return Result.Failed;
            }
        }
    }
}