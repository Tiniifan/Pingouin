using System.Linq;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        #region Search Properties

        private string _searchText = "";
        /// <summary>
        /// Gets or sets the text entered by the user in the search box.
        /// The setter automatically triggers a search when the text changes.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Only execute the search if the text is not the placeholder.
                    if (value != PlaceholderText)
                    {
                        ExecuteSearch();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the dynamic placeholder text for the search box based on the current directory.
        /// </summary>
        private string PlaceholderText => $"Search in: {GetCurrentFolderName()}";

        #endregion

        #region Search Command Implementations

        /// <summary>
        /// Handles the GotFocus event of the search box.
        /// If the current text is the placeholder, it is cleared to allow for user input.
        /// </summary>
        private void ExecuteSearchGotFocus(object parameter)
        {
            if (SearchText == PlaceholderText)
            {
                SearchText = "";
            }
        }

        /// <summary>
        /// Handles the LostFocus event of the search box.
        /// If the search box is empty, the placeholder text is restored.
        /// </summary>
        private void ExecuteSearchLostFocus(object parameter)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                SearchText = PlaceholderText;
            }
        }

        /// <summary>
        /// Performs the search based on the text in SearchText.
        /// If the search text is empty, it reverts to displaying the current directory's content.
        /// Otherwise, it populates the view with search results.
        /// </summary>
        private void ExecuteSearch()
        {
            if (_archiveOpened == null) return;

            // If the search bar is empty or just whitespace, revert to the normal folder view.
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                PopulateListView(AddressBarText);
                return;
            }

            CurrentFilesAndFolders.Clear();

            // Find the base directory for the search from the current address bar path.
            var baseDirectory = _archiveOpened.Directory.GetFolderFromFullPath(AddressBarText);
            if (baseDirectory == null) return;

            // Use the archive's optimized search methods to find all matching items recursively.
            var foundFolders = _archiveOpened.Directory.SearchDirectories(SearchText, AddressBarText);
            var foundFiles = _archiveOpened.Directory.SearchFiles(SearchText, AddressBarText);

            // Add found folders to the results list.
            foreach (var folder in foundFolders.OrderBy(f => f.Name))
            {
                CurrentFilesAndFolders.Add(new FileItemViewModel
                {
                    Icon = "📁",
                    Name = folder.Name,
                    Size = FormatSize(folder.GetSize()),
                    Type = "Folder",
                    ForeColor = folder.Color,
                    Tag = folder
                });
            }

            // Add found files to the results list.
            foreach (var file in foundFiles.OrderBy(f => f.Key))
            {
                CurrentFilesAndFolders.Add(new FileItemViewModel
                {
                    Icon = _fileTypeService.GetIconForFile(file.Key),
                    Name = System.IO.Path.GetFileName(file.Key),
                    Size = FormatSize(file.Value.Size),
                    Type = _fileTypeService.GetTypeName(file.Key),
                    ForeColor = file.Value.Color,
                    Tag = file
                });
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Extracts the name of the current folder from a full path.
        /// </summary>
        /// <param name="path">The full path. If null, the current AddressBarText is used.</param>
        /// <returns>The name of the folder, or "Root" for the base directory.</returns>
        private string GetCurrentFolderName(string path = null)
        {
            // Use the address bar's text as the default path if none is provided.
            if (path == null)
            {
                path = AddressBarText;
            }

            path = path.TrimEnd('/');

            // Handle the root directory case.
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                return "Root";
            }

            // Extract the last segment of the path.
            int lastSlashIndex = path.LastIndexOf('/');

            return path.Substring(lastSlashIndex + 1);
        }

        #endregion
    }
}