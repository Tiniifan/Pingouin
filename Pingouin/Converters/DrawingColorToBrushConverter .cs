using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.Windows;

namespace Pingouin.Converters
{
    public class DrawingColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color drawingColor)
            {
                // If color is empty or default, do nothing so that it inherits the parent style
                if (drawingColor.IsEmpty)
                {
                    return Binding.DoNothing;
                }

                // Check if the color is orange (exact or close shades)
                if (IsOrangeColor(drawingColor))
                {
                    // Convert System.Drawing.Color to System.Windows.Media.Color
                    var mediaColor = System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
                    return new SolidColorBrush(mediaColor);
                }

                // If it's not orange, return the primary text brush
                return Application.Current.FindResource("Theme.Text.PrimaryBrush") as SolidColorBrush ??
                       new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
            }

            // Return the primary text brush as default
            return Application.Current.FindResource("Theme.Text.PrimaryBrush") as SolidColorBrush ??
                   new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
        }

        private bool IsOrangeColor(System.Drawing.Color color)
        {
            // Exact check for Orange
            if (color == System.Drawing.Color.Orange)
                return true;

            // Or check for other orange shades
            return color == System.Drawing.Color.DarkOrange ||
                   color == System.Drawing.Color.OrangeRed ||
                   color.Name.ToLower().Contains("orange");

            // Alternative: RGB values check for an orange range
            // return color.R > 200 && color.G > 100 && color.G < 200 && color.B < 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}