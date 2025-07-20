using System;
using System.IO;

namespace Pingouin.Services
{
    /// <summary>
    /// Provides methods to get icon and descriptive type name for a given file based on its extension.
    /// </summary>
    public class FileTypeService
    {
        /// <summary>
        /// Returns a string icon representing the type of file based on its extension or name.
        /// </summary>
        /// <param name="fileName">The name or path of the file.</param>
        /// <returns>A string containing an emoji icon corresponding to the file type.</returns>
        public string GetIconForFile(string fileName)
        {
            if (fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                return "💾"; // Level 5 Binary Data
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".xq":
                case ".nut":
                case ".nutb":
                    return "📜"; // Script files
                case ".xi":
                    return "🖼️"; // Image files
                case ".xc":
                case ".fa":
                case ".xb":
                case ".xr":
                case ".zip":
                case ".pck":
                case ".xa":
                    return "📦"; // Archive files
                case ".mbn":
                    return "⚙️"; // Bone files
                case ".cmr":
                    return "📹"; // Camera data files
                case ".prm":
                    return "🧊"; // Model parameter files
                case ".mtn":
                    return "🎞️"; // Animation data files
                default:
                    return "📄"; // Generic file icon
            }
        }

        /// <summary>
        /// Returns a descriptive type name for a file based on its extension or name.
        /// </summary>
        /// <param name="fileName">The name or path of the file.</param>
        /// <returns>A string describing the type of the file.</returns>
        public string GetTypeName(string fileName)
        {
            if (fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase))
            {
                return "Level 5 Binary Data";
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".xq":
                    return "Level 5 Script";
                case ".nut":
                case ".nutb":
                    return "Squirrel Script";
                case ".xi":
                    return "Image File";
                case ".xc":
                case ".fa":
                case ".xb":
                case ".xr":
                case ".zip":
                case ".pck":
                case ".xa":
                    return "Archive File";
                case ".mbn":
                    return "Bone File";
                case ".cmr":
                    return "Camera Data";
                case ".prm":
                    return "Model Parameters";
                case ".mtn":
                    return "Animation Data";
                default:
                    return "File";
            }
        }
    }
}