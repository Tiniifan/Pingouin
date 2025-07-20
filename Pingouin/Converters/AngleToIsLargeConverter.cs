using System;
using System.Windows.Data;

namespace Pingouin.Converters
{
    /// <summary>
    /// Converts an angle (in degrees) to a boolean indicating if the angle is greater than 180°.
    /// Used to determine if an arc is a "large arc" in graphical contexts.
    /// </summary>
    public class AngleToIsLargeConverter : IValueConverter
    {
        /// <summary>
        /// Returns true if the angle is greater than 180 degrees; otherwise false.
        /// </summary>
        /// <param name="value">Angle in degrees (double).</param>
        /// <param name="targetType">Target type (ignored).</param>
        /// <param name="parameter">Optional parameter (ignored).</param>
        /// <param name="culture">Culture info (ignored).</param>
        /// <returns>Boolean indicating if angle > 180°.</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (double)value > 180;
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
