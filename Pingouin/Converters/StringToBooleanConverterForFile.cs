using System;
using System.Windows.Data;
using System.Globalization;

namespace Pingouin.Converters
{
    /// <summary>
    /// Converts a string representing a type ("Folder" or other) to a boolean indicating if it's a file.
    /// Returns true if the type is not "Folder", meaning it's considered a file.
    /// </summary>
    public class StringToBooleanConverterForFile : IValueConverter
    {
        /// <summary>
        /// Converts a string value from the ViewModel to a boolean for the View.
        /// Returns true if the input is not "Folder" (i.e., it's a file).
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string typeName)
            {
                // Return true if the item is not a folder
                return !typeName.Equals("Folder", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// Not implemented. ConvertBack is not used in this scenario.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
