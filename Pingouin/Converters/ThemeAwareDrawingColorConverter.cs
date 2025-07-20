using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace Pingouin.Converters
{
    // Boring class, should be deleted in next update
    public class ThemeAwareDrawingColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // If the data is not ready, we do nothing
            if (values == null || values.Length < 1 || !(values[0] is System.Drawing.Color drawingColor))
            {
                return DependencyProperty.UnsetValue;
            }

            // If the color is empty/default, let the parent style decide
            if (drawingColor.IsEmpty)
            {
                // Return the current theme brush
                return Application.Current.FindResource("Theme.Text.PrimaryBrush");
            }

            // Check if the color is a shade of orange
            if (IsOrangeColor(drawingColor))
            {
                // Create a brush from the specific orange color
                var mediaColor = Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);

                return new SolidColorBrush(mediaColor);
            }

            return Application.Current.FindResource("Theme.Text.PrimaryBrush");
        }

        private bool IsOrangeColor(System.Drawing.Color color)
        {
            if (color == System.Drawing.Color.Orange) return true;

            return color == System.Drawing.Color.DarkOrange ||
                   color == System.Drawing.Color.OrangeRed ||
                   color.Name.ToLower().Contains("orange");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}