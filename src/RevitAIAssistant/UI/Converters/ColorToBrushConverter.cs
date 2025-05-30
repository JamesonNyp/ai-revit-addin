using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace RevitAIAssistant.UI.Converters
{
    /// <summary>
    /// Converter from Color to SolidColorBrush with optional opacity
    /// </summary>
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}