using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Collections.Generic;
using Pingouin.Properties;

namespace Pingouin.ViewModels
{
    /// <summary>
    /// Represents a theme option in the UI, such as in a ComboBox.
    /// </summary>
    public class ThemeOption
    {
        /// <summary>
        /// The name displayed in the UI (e.g., "Blue Acrylic").
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The pack URI for embedded resources or the file path for external files.
        /// </summary>
        public string ResourcePath { get; set; }

        /// <summary>
        /// Differentiates between a theme compiled into the app and an external file.
        /// </summary>
        public bool IsEmbeddedResource { get; set; }
    }

    public partial class MainViewModel
    {
        #region Theme Properties

        /// <summary>
        /// Gets the list of all detected themes, used to populate a selector in the UI.
        /// </summary>
        public List<ThemeOption> AvailableThemes { get; private set; }

        private ThemeOption _selectedTheme;
        /// <summary>
        /// Gets or sets the currently selected theme.
        /// The setter contains the core logic for applying a new theme.
        /// </summary>
        public ThemeOption SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value) && value != null)
                {
                    // Read the accent color previously saved by the user.
                    string savedColorString = Settings.Default.ThemeColor;
                    Color? accentColor = null;

                    // If a color was saved, try to convert it.
                    if (!string.IsNullOrEmpty(savedColorString))
                    {
                        try
                        {
                            accentColor = (Color)ColorConverter.ConvertFromString(savedColorString);
                        }
                        catch 
                        { 
                        }
                    }

                    // Apply the theme, differentiating between embedded resources and external files.
                    Color appliedColor;
                    if (value.IsEmbeddedResource)
                    {
                        appliedColor = _themeService.ApplyThemeFromResource(value.ResourcePath, accentColor);
                    }
                    else
                    {
                        appliedColor = _themeService.ApplyThemeFromFile(value.ResourcePath, accentColor);
                    }

                    // Update the accent color property's backing field directly.
                    _currentAccentColor = appliedColor;
                    OnPropertyChanged(nameof(CurrentAccentColor));

                    // Save the selected theme choice for the next application launch.
                    Settings.Default.ThemeName = value.ResourcePath;
                    Settings.Default.ThemeIsEmbedded = value.IsEmbeddedResource;
                    Settings.Default.Save();
                }
            }
        }

        private Color _currentAccentColor;
        /// <summary>
        /// Gets or sets the current accent color.
        /// The setter re-applies the current theme using this new color.
        /// </summary>
        public Color CurrentAccentColor
        {
            get => _currentAccentColor;
            set
            {
                // SetProperty handles checking for actual changes, updating the field, and notifying the UI.
                if (SetProperty(ref _currentAccentColor, value))
                {
                    if (this.SelectedTheme != null)
                    {
                        // Re-apply the currently selected theme, but with the new accent color.
                        if (SelectedTheme.IsEmbeddedResource)
                        {
                            _themeService.ApplyThemeFromResource(this.SelectedTheme.ResourcePath, value);
                        }
                        else
                        {
                            _themeService.ApplyThemeFromFile(this.SelectedTheme.ResourcePath, value);
                        }
                    }

                    // Save the new accent color choice for persistence.
                    Settings.Default.ThemeColor = value.ToString();
                    Settings.Default.Save();
                }
            }
        }
        #endregion

        #region Theme Command Implementations

        /// <summary>
        /// Command logic for opening the color picker dialog.
        /// </summary>
        private void ExecutePickAccentColor(object parameter)
        {
            CurrentAccentColor = _themeService.PickAccentColor(CurrentAccentColor);
        }

        /// <summary>
        /// Command logic for resetting the accent color to the theme's default.
        /// </summary>
        private void ExecuteResetAccentColor(object parameter)
        {
            if (SelectedTheme == null) return;

            // Clear the custom accent color from the user settings.
            Settings.Default.ThemeColor = "";
            Settings.Default.Save();

            // Re-apply the current theme, passing 'null' as the accent color.
            Color defaultColor;
            if (SelectedTheme.IsEmbeddedResource)
            {
                defaultColor = _themeService.ApplyThemeFromResource(SelectedTheme.ResourcePath, null);
            }
            else
            {
                defaultColor = _themeService.ApplyThemeFromFile(SelectedTheme.ResourcePath, null);
            }

            // Update the property's backing field directly to reflect the change in the UI
            _currentAccentColor = defaultColor;
            OnPropertyChanged(nameof(CurrentAccentColor));
        }
        #endregion

        #region Theme Helper Methods

        /// <summary>
        /// Initializes the theme system on application startup.
        /// </summary>
        private void InitializeTheme()
        {
            // Discover all available themes.
            AvailableThemes = new List<ThemeOption>();
            LoadEmbeddedThemes();
            LoadThemesFromDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes"), "Custom");

            if (!AvailableThemes.Any())
            {
                _dialogService.ShowMessage("No themes found. The application may not display correctly.", "Theme Error", System.Windows.MessageBoxImage.Error);
                return;
            }

            // Load the user's saved preferences.
            string savedThemePath = Settings.Default.ThemeName;
            bool savedIsEmbedded = Settings.Default.ThemeIsEmbedded;
            string savedColorString = Settings.Default.ThemeColor;

            // Find the theme to load: either the one from settings or the first available theme as a fallback.
            var themeToLoad = AvailableThemes.FirstOrDefault(t =>
                t.ResourcePath == savedThemePath && t.IsEmbeddedResource == savedIsEmbedded)
                ?? AvailableThemes.First();

            // Determine the accent color to load.
            Color? colorToLoad = null;
            if (!string.IsNullOrEmpty(savedColorString))
            {
                try
                {
                    colorToLoad = (Color)ColorConverter.ConvertFromString(savedColorString);
                }
                catch 
                { 
                }
            }

            // Apply the determined theme and color.
            Color appliedColor;
            if (themeToLoad.IsEmbeddedResource)
            {
                appliedColor = _themeService.ApplyThemeFromResource(themeToLoad.ResourcePath, colorToLoad);
            }
            else
            {
                appliedColor = _themeService.ApplyThemeFromFile(themeToLoad.ResourcePath, colorToLoad);
            }

            // Set the ViewModel properties directly to avoid re-triggering logic during initialization.
            _selectedTheme = themeToLoad;
            _currentAccentColor = appliedColor;

            // Notify the UI that the initial properties are ready for binding.
            OnPropertyChanged(nameof(SelectedTheme));
            OnPropertyChanged(nameof(CurrentAccentColor));
        }

        /// <summary>
        /// Loads themes that are compiled as resources within the application assembly.
        /// </summary>
        private void LoadEmbeddedThemes()
        {
            var embeddedThemes = new[]
            {
                new { Name = "White", Resource = "pack://application:,,,/Styles/Colors/WhiteTheme.xaml" },
                new { Name = "Dark", Resource = "pack://application:,,,/Styles/Colors/DarkTheme.xaml" },
                new { Name = "Blue Acrylic", Resource = "pack://application:,,,/Styles/Colors/BlueAcrylicTheme.xaml" },
                new { Name = "White Transparent", Resource = "pack://application:,,,/Styles/Colors/WhiteTransparentTheme.xaml" },
                new { Name = "Dark Transparent", Resource = "pack://application:,,,/Styles/Colors/DarkTransparentTheme.xaml" },
                new { Name = "Blue Acrylic Transparent", Resource = "pack://application:,,,/Styles/Colors/BlueAcrylicTransparentTheme.xaml" },
            };

            foreach (var theme in embeddedThemes)
            {
                AvailableThemes.Add(new ThemeOption
                {
                    DisplayName = theme.Name,
                    ResourcePath = theme.Resource,
                    IsEmbeddedResource = true
                });
            }
        }

        /// <summary>
        /// Loads custom themes from a specified directory.
        /// </summary>
        private void LoadThemesFromDirectory(string directoryPath, string category)
        {
            if (!Directory.Exists(directoryPath)) return;

            var themeFiles = Directory.EnumerateFiles(directoryPath, "*.xaml");
            foreach (var file in themeFiles)
            {
                // Add the " (Custom)" suffix to indicate it's a user-provided theme.
                string themeName = Path.GetFileNameWithoutExtension(file) + " (Custom)";

                AvailableThemes.Add(new ThemeOption
                {
                    DisplayName = themeName,
                    ResourcePath = file,
                    IsEmbeddedResource = false
                });
            }
        }
        #endregion
    }
}