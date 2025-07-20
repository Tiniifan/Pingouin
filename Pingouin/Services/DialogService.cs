using System.Linq;
using System.Windows;
using Microsoft.Win32;
using System.Collections.Generic;
using Microsoft.WindowsAPICodePack.Dialogs;
using Interaction = Microsoft.VisualBasic.Interaction;

namespace Pingouin.Services
{
    /// <summary>
    /// Provides methods to display common dialog windows like file open/save dialogs, message boxes, and input boxes.
    /// </summary>
    public class DialogService
    {
        /// <summary>
        /// Shows a file open dialog filtered to specific archive and zip file types.
        /// </summary>
        /// <returns>The selected file path, or null if canceled.</returns>
        public string ShowOpenFileDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "L5 Archive (*.fa, *.xb, *.pck, *.xa, *.xr, *.xc, *.xv)|*.fa;*.xb;*.pck;*.xa;*.xr;*.xc;*.xv|" +
                         "Zip archive (*.zip)|*.zip|" +
                         "All supported files|*.fa;*.xb;*.pck;*.xa;*.xr;*.xc;*.xv;*.zip|" +
                         "All files (*.*)|*.*",
                Title = "Open Archive File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows an open file dialog that allows multiple file selection.
        /// </summary>
        /// <returns>A collection of selected file paths, or an empty collection if canceled.</returns>
        public IEnumerable<string> ShowImportFilesDialog()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = true,
                Title = "Import Files"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileNames;
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Shows a save file dialog with a default file name and filters for archive files.
        /// </summary>
        /// <param name="defaultFileName">Suggested default file name.</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        public string ShowSaveFileDialog(string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "Archive Files (*.fa, *.xpck)|*.fa;*.xpck|All Files (*.*)|*.*",
                Title = "Save Archive"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows a save file dialog for exporting files with no specific filter.
        /// </summary>
        /// <param name="defaultFileName">Suggested default file name.</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        public string ShowExportFileDialog(string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                Filter = "All files|*.*",
                Title = "Export File"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows a simple message box with specified message, title, and icon.
        /// </summary>
        /// <param name="message">Message text to display.</param>
        /// <param name="title">Window title.</param>
        /// <param name="icon">Icon to display (default is Information).</param>
        public void ShowMessage(string message, string title, MessageBoxImage icon = MessageBoxImage.Information)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, icon);
        }

        /// <summary>
        /// Shows an input box prompting the user for text input.
        /// </summary>
        /// <param name="prompt">Prompt message.</param>
        /// <param name="title">Dialog title.</param>
        /// <param name="defaultValue">Default text value.</param>
        /// <returns>The input string entered by the user.</returns>
        public string ShowInputBox(string prompt, string title, string defaultValue = "")
        {
            return Interaction.InputBox(prompt, title, defaultValue);
        }

        /// <summary>
        /// Shows a generic open file dialog with a custom title and all files filter.
        /// </summary>
        /// <param name="text">Title text for the dialog.</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        public string ShowOpenFileDialog(string text)
        {
            var dialog = new OpenFileDialog
            {
                Title = text,
                Filter = "All files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows a folder selection dialog with a custom title.
        /// </summary>
        /// <param name="text">Title text for the dialog.</param>
        /// <returns>The selected folder path, or null if canceled.</returns>
        public string ShowSelectFolderDialog(string text)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = text,
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return null;
        }

        /// <summary>
        /// Shows a confirmation message box with Yes/No buttons and Question icon.
        /// </summary>
        /// <param name="text">Confirmation message.</param>
        /// <param name="title">Window title.</param>
        /// <returns>True if user selects Yes; otherwise false.</returns>
        public bool ShowConfirmation(string text, string title)
        {
            var result = MessageBox.Show(text, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Shows a confirmation message box with customizable buttons and icon.
        /// </summary>
        /// <param name="text">Confirmation message.</param>
        /// <param name="title">Window title.</param>
        /// <param name="buttons">Buttons to display (e.g., YesNo, OKCancel).</param>
        /// <param name="icon">Icon to display.</param>
        /// <returns>True if user selects Yes; otherwise false.</returns>
        public bool ShowConfirmation(string text, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            var result = MessageBox.Show(text, title, buttons, icon);
            return result == MessageBoxResult.Yes;
        }
    }
}