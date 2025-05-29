using System;
using System.Windows;
using Autodesk.Revit.UI;

namespace RevitAIAssistant.UI.Views
{
    /// <summary>
    /// Provider for the AI Assistant dockable pane
    /// </summary>
    public class AIAssistantPaneProvider : IDockablePaneProvider
    {
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = new AIAssistantPanel();
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Right,
                MinimumWidth = 400,
                MinimumHeight = 300
            };
        }
    }
}