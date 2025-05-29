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

    /// <summary>
    /// Template selector for different message types
    /// </summary>
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? AssistantTemplate { get; set; }
        public DataTemplate? SystemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message)
            {
                return message.Role switch
                {
                    MessageRole.User => UserTemplate,
                    MessageRole.Assistant => AssistantTemplate,
                    MessageRole.System => SystemTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }
            
            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Converter from Color to SolidColorBrush with optional opacity
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color color)
            {
                // Apply opacity if parameter is provided
                if (parameter is double opacity)
                {
                    color = Color.FromArgb((byte)(color.A * opacity), color.R, color.G, color.B);
                }
                else if (parameter is string opacityStr && double.TryParse(opacityStr, out var opacityValue))
                {
                    color = Color.FromArgb((byte)(color.A * opacityValue), color.R, color.G, color.B);
                }
                
                return new SolidColorBrush(color);
            }
            
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}