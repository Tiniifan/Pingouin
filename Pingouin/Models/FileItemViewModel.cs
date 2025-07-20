using System.Drawing;
using Pingouin.ViewModels;

namespace Pingouin.Models
{
    /// <summary>
    /// Represents a file system item (file or folder) for the UI, with display properties.
    /// Implements IFileSystemItem and inherits from BaseViewModel for property notification support.
    /// </summary>
    public class FileItemViewModel : BaseViewModel, IFileSystemItem
    {
        /// <summary>
        /// Icon identifier or path to represent the file/folder.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Display name of the file or folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Human-readable file size string (e.g., "12 KB").
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// File or folder type description (e.g., "Folder", "Text Document").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Foreground color used for displaying the item.
        /// </summary>
        public Color ForeColor { get; set; }

        /// <summary>
        /// Optional additional data or metadata associated with the item.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Full absolute path of the file or folder.
        /// </summary>
        public string FullPath { get; set; }
    }
}
