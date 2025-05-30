using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using RevitAIAssistant.UI.ViewModels;
using static RevitAIAssistant.UI.ViewModels.AIAssistantViewModel;

namespace RevitAIAssistant.UI.Views
{
    /// <summary>
    /// Main AI Assistant panel that provides chat interface for engineering tasks
    /// </summary>
    public partial class AIAssistantPanel : UserControl
    {
        private MockAIAssistantViewModel _viewModel;

        public AIAssistantPanel()
        {
            InitializeComponent();
            
            // Use mock view model for UI testing
            _viewModel = new MockAIAssistantViewModel();
            DataContext = _viewModel;
            
            // Auto-scroll to bottom when new messages are added
            if (_viewModel != null)
            {
                _viewModel.Messages.CollectionChanged += (s, e) =>
                {
                    if (e.NewItems != null && e.NewItems.Count > 0)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            ChatScrollViewer.ScrollToEnd();
                        }));
                    }
                };
            }
            
            // Focus on input when panel loads
            Loaded += (s, e) => InputTextBox.Focus();
        }
    }
}