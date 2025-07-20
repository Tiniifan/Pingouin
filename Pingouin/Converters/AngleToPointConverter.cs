using System;
using System.Windows;
using System.Windows.Data;

namespace Pingouin.Converters
{
    /// <summary>
    /// Converts an angle (in degrees) to a Point representing the end position on a circle's circumference.
    /// Used to calculate the endpoint of an arc based on the angle.
    /// </summary>
    public class AngleToPointConverter : IValueConverter
    {
        /// <summary>
        /// Converts an angle in degrees to a Point on a circle of fixed radius 50 (diameter 100).
        /// The 0° angle corresponds to the top of the circle (shifted by -90°).
        /// </summary>
        /// <param name="value">Angle in degrees (double).</param>
        /// <param name="targetType">Expected target type (ignored).</param>
        /// <param name="parameter">Optional parameter (ignored).</param>
        /// <param name="culture">Culture info (ignored).</param>
        /// <returns>Point on the circle corresponding to the angle.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double angle = (double)value;
            double radius = 50; // Radius based on 100x100 size

            // Shift starting point by -90 degrees to start at the top
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius + radius * Math.Cos(angleRad);
            double y = radius + radius * Math.Sin(angleRad);

            return new Point(x, y);
        }

        /// <summary>
        /// ConvertBack is not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}