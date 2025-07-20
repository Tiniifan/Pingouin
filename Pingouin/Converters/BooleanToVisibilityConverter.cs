using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace Pingouin.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility enumeration.
    /// True returns Visible; false returns Collapsed.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to Visibility.Visible or Visibility.Collapsed.
        /// </summary>
        /// <param name="value">Input value expected to be a boolean.</param>
        /// <param name="targetType">Target type of the binding (ignored).</param>
        /// <param name="parameter">Optional parameter (ignored).</param>
        /// <param name="culture">Culture info (ignored).</param>
        /// <returns>Visible if true; otherwise Collapsed.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool b && b) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// ConvertBack is not implemented.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}