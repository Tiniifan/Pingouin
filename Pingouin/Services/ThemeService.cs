using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Text.RegularExpressions;
using DrawingColor = System.Drawing.Color;

namespace Pingouin.Services
{
    /// <summary>
    /// Manages loading and applying application themes, including dynamic accent color adjustments.
    /// </summary>
    public class ThemeService
    {
        /// <summary>
        /// A key used to identify the current theme dictionary within the application's resources.
        /// </summary>
        private const string ThemeDictionaryKey = "CurrentAppTheme";

        /// <summary>
        /// Loads and applies a theme from an embedded resource URI.
        /// </summary>
        /// <param name="resourceUri">The pack URI of the theme resource (e.g., "pack://application:,,,/YourAssembly;component/Themes/Dark.xaml").</param>
        /// <param name="accentColor">An optional accent color to apply to the theme. If null, the theme's default accent is used.</param>
        /// <returns>The final accent color that was applied.</returns>
        public Color ApplyThemeFromResource(string resourceUri, Color? accentColor)
        {
            // Do not attempt to apply a theme if the application is not running or is shutting down.
            if (Application.Current == null || Application.Current.ShutdownMode == ShutdownMode.OnExplicitShutdown && Application.Current.Windows.Count == 0)
            {
                return Colors.Transparent;
            }

            try
            {
                string xamlContent = ReadEmbeddedResourceContent(resourceUri);

                return ProcessXamlContentAndApplyTheme(xamlContent, accentColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply theme from resource '{resourceUri}'.\nError: {ex.Message}",
                    "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return Colors.Red;
            }
        }

        /// <summary>
        /// Loads and applies a theme from an external XAML file.
        /// </summary>
        /// <param name="themePath">The file path to the XAML theme file.</param>
        /// <param name="accentColor">An optional accent color to apply to the theme. If null, the theme's default accent is used.</param>
        /// <returns>The final accent color that was applied.</returns>
        public Color ApplyThemeFromFile(string themePath, Color? accentColor)
        {
            // Do not attempt to apply a theme if the application is not running or is shutting down.
            if (Application.Current == null || Application.Current.ShutdownMode == ShutdownMode.OnExplicitShutdown && Application.Current.Windows.Count == 0)
            {
                return Colors.Transparent;
            }

            try
            {
                if (!File.Exists(themePath))
                    throw new FileNotFoundException("Theme file not found.", themePath);

                string xamlContent = File.ReadAllText(themePath);

                return ProcessXamlContentAndApplyTheme(xamlContent, accentColor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply theme '{Path.GetFileName(themePath)}'.\nError: {ex.Message}",
                    "Theme Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return Colors.Red;
            }
        }

        /// <summary>
        /// Reads the content of an embedded resource XAML file into a string.
        /// </summary>
        private string ReadEmbeddedResourceContent(string resourceUri)
        {
            var uri = new Uri(resourceUri, UriKind.Absolute);
            var resourceInfo = Application.GetResourceStream(uri);

            if (resourceInfo == null)
                throw new InvalidOperationException($"Resource not found: {resourceUri}");

            using (var reader = new StreamReader(resourceInfo.Stream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Processes the XAML content of a theme, applies an optional accent color, and updates the application's resources.
        /// </summary>
        /// <param name="xamlContent">The raw XAML of the theme as a string.</param>
        /// <param name="accentColor">The accent color to apply. If null, the default is extracted from the XAML.</param>
        /// <returns>The final accent color used.</returns>
        private Color ProcessXamlContentAndApplyTheme(string xamlContent, Color? accentColor)
        {
            Color finalAccentColor;

            // If a specific accent color is provided, we use it to tint all SolidColorBrush resources in the theme.
            if (accentColor.HasValue)
            {
                finalAccentColor = accentColor.Value;

                // This regex finds all SolidColorBrush definitions to apply the tint.
                var regex = new Regex("(<SolidColorBrush\\s+x:Key=\"([^\"]+)\"\\s+Color=\")([^\"]+)(\"\\s*/>)");

                xamlContent = regex.Replace(xamlContent, match =>
                {
                    string key = match.Groups[2].Value;
                    string originalColorStr = match.Groups[3].Value;

                    try
                    {
                        Color originalColor = (Color)ColorConverter.ConvertFromString(originalColorStr);

                        // Blend the original brush color with the new accent color to create a tinted version.
                        Color tinted = BlendColors(originalColor, finalAccentColor, 0.4); // 40% tint
                        string newColorStr = ColorToHex(tinted);

                        // Reconstruct the XAML tag with the new tinted color.
                        return $"{match.Groups[1].Value}{newColorStr}{match.Groups[4].Value}";
                    }
                    catch
                    {
                        // If color conversion fails, return the original match to avoid breaking the XAML.
                        return match.Value;
                    }
                });
            }
            else // If no accent color is provided, extract the default accent color from the theme itself.
            {
                // This regex specifically looks for the 'Theme.Accent.Brush' to determine the default accent color.
                var regex = new Regex("<SolidColorBrush\\s+x:Key=\"Theme.Accent.Brush\"\\s+Color=\"([^\"]*)\"");
                var match = regex.Match(xamlContent);

                if (match.Success)
                {
                    finalAccentColor = (Color)ColorConverter.ConvertFromString(match.Groups[1].Value);
                }
                else
                {
                    // Fallback to a default color if the accent brush is not found in the theme file.
                    finalAccentColor = Colors.DodgerBlue;
                }
            }

            // Parse the modified (or original) XAML string into a ResourceDictionary object.
            var newThemeDictionary = (ResourceDictionary)XamlReader.Parse(xamlContent);

            // Tag the new dictionary so we can identify it later for removal.
            newThemeDictionary[ThemeDictionaryKey] = true;

            ApplyThemeDictionary(newThemeDictionary);

            return finalAccentColor;
        }

        /// <summary>
        /// Replaces the existing theme dictionary in the application's resources with a new one.
        /// </summary>
        private void ApplyThemeDictionary(ResourceDictionary newTheme)
        {
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries;

            // Find and remove the old theme dictionary using the key we added.
            var oldTheme = mergedDictionaries.FirstOrDefault(dict => dict.Contains(ThemeDictionaryKey));
            if (oldTheme != null)
                mergedDictionaries.Remove(oldTheme);

            // Add the new theme to the application's resources.
            mergedDictionaries.Add(newTheme);
        }

        /// <summary>
        /// Opens a color picker dialog for the user to select an accent color.
        /// </summary>
        /// <param name="initialColor">The color to pre-select in the dialog.</param>
        /// <returns>The selected color, or the initial color if the dialog is cancelled.</returns>
        public Color PickAccentColor(Color initialColor)
        {
            // Using Windows.Forms.ColorDialog as WPF does not have a built-in one.
            using (var colorDialog = new System.Windows.Forms.ColorDialog())
            {
                // Convert WPF color to Drawing.Color
                colorDialog.Color = DrawingColor.FromArgb(initialColor.A, initialColor.R, initialColor.G, initialColor.B);
                colorDialog.FullOpen = true;

                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var selected = colorDialog.Color;

                    // Convert Drawing.Color back to WPF color
                    return Color.FromArgb(selected.A, selected.R, selected.G, selected.B);
                }
            }

            return initialColor;
        }

        #region Helpers

        /// <summary>
        /// Converts a WPF Color object to its hexadecimal string representation (e.g., #AARRGGBB).
        /// </summary>
        private string ColorToHex(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Linearly interpolates between two colors.
        /// </summary>
        /// <param name="baseColor">The starting color.</param>
        /// <param name="tintColor">The color to blend with.</param>
        /// <param name="amount">The amount of tintColor to apply (0.0 to 1.0).</param>
        /// <returns>The resulting blended color.</returns>
        private Color BlendColors(Color baseColor, Color tintColor, double amount)
        {
            byte a = (byte)(baseColor.A * (1 - amount) + tintColor.A * amount);
            byte r = (byte)(baseColor.R * (1 - amount) + tintColor.R * amount);
            byte g = (byte)(baseColor.G * (1 - amount) + tintColor.G * amount);
            byte b = (byte)(baseColor.B * (1 - amount) + tintColor.B * amount);
            return Color.FromArgb(a, r, g, b);
        }

        #endregion
    }
}