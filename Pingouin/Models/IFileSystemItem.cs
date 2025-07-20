namespace Pingouin.Models
{
    /// <summary>
    /// Represents a file system item abstraction.
    /// Provides basic properties common to files and folders.
    /// </summary>
    public interface IFileSystemItem
    {
        /// <summary>
        /// Gets the display name of the item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type description of the item (e.g., "Folder", "Text Document").
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the size of the item as a formatted string.
        /// </summary>
        string Size { get; }

        /// <summary>
        /// Gets the full absolute path of the item.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Gets optional additional data or metadata related to the item.
        /// </summary>
        object Tag { get; }
    }
}