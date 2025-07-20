using System.Collections.ObjectModel;
using Pingouin.ViewModels;

namespace Pingouin.Models
{
    /// <summary>
    /// ViewModel representing a folder node in a hierarchical file system view.
    /// Implements IFileSystemItem and supports selection and expansion state.
    /// </summary>
    public class FolderNodeViewModel : BaseViewModel, IFileSystemItem
    {
        /// <summary>
        /// Gets or sets the display name of the folder.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the full absolute path of the folder.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets the type of the item, always "Folder" for this class.
        /// </summary>
        public string Type => "Folder";

        /// <summary>
        /// Gets or sets the size of the folder as a formatted string.
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets an optional tag, e.g. to store a VirtualDirectory reference.
        /// </summary>
        public object Tag { get; set; }

        private bool _isSelected;

        /// <summary>
        /// Gets or sets whether the folder node is currently selected in the UI.
        /// Raises PropertyChanged when modified.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private bool _isExpanded;

        /// <summary>
        /// Gets or sets whether the folder node is expanded to show children.
        /// Raises PropertyChanged when modified.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        /// <summary>
        /// Collection of child folder nodes representing subfolders.
        /// </summary>
        public ObservableCollection<FolderNodeViewModel> Children { get; set; } = new ObservableCollection<FolderNodeViewModel>();
    }
}