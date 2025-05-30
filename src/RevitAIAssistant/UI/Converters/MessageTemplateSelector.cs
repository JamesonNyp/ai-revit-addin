using System.Windows;
using System.Windows.Controls;
using RevitAIAssistant.Models;
using static RevitAIAssistant.UI.ViewModels.AIAssistantViewModel;

namespace RevitAIAssistant.UI.Converters
{
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
}