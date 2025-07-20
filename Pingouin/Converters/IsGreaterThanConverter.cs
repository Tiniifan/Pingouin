using System;
using System.Globalization;
using System.Windows.Data;

namespace Pingouin.Converters
{
    /// <summary>
    /// MultiValueConverter that compares the first integer value against a threshold.
    /// Returns true if the first value is greater than the threshold specified in the parameter.
    /// </summary>
    public class IsGreaterThanConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts an array of values, comparing the first integer value to a threshold.
        /// The threshold is parsed from the converter parameter.
        /// Returns true if the first value is greater than the threshold; otherwise, false.
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length > 0 && values[0] is int count && parameter is string stringValue)
            {
                if (int.TryParse(stringValue, out int threshold))
                {
                    return count > threshold;
                }
            }
            return false;
        }

        /// <summary>
        /// ConvertBack is not implemented.
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}