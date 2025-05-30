using System;
using RevitAIAssistant.UI.Controls;
using RevitAIAssistant.UI.Converters;

namespace RevitAIAssistant.Utils
{
    /// <summary>
    /// This class exists solely to verify that all types can be resolved at compile time
    /// </summary>
    internal static class TypeVerification
    {
        static TypeVerification()
        {
            // These lines verify the types exist and can be instantiated
            _ = typeof(RichContentPresenter);
            _ = typeof(ColorToBrushConverter);
            _ = typeof(MessageTemplateSelector);
        }
    }
}