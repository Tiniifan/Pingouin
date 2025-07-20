using System;
using System.Collections.ObjectModel;
using StudioElevenLib.Level5.Archive;
using StudioElevenLib.Tools;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    /// <summary>
    /// Represents the state and data of a single tab in the interface.
    /// </summary>
    public class TabViewModel : BaseViewModel
    {
        /// <summary>
        /// Unique identifier for the tab instance.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Optional reference to the parent tab (if this tab is a child).
        /// </summary>
        public Guid? ParentTabId { get; set; }

        private string _header;

        /// <summary>
        /// Displayed title of the tab.
        /// </summary>
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Stores the file path or display name of the archive (not provided by IArchive).
        /// </summary>
        public string ArchiveFilePath { get; set; }

        /// <summary>
        /// Reference to the archive loaded in this tab.
        /// </summary>
        public IArchive ParentArchive { get; set; }

        /// <summary>
        /// Virtual root directory inside the archive.
        /// </summary>
        public VirtualDirectory RootDirectory { get; set; }

        /// <summary>
        /// Stack used to keep track of navigation history within the tab.
        /// </summary>
        public ObservableCollection<string> NavigationHistory { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Folder tree structure specific to this tab.
        /// </summary>
        public ObservableCollection<FolderNodeViewModel> RootFolders { get; set; } = new ObservableCollection<FolderNodeViewModel>();

        /// <summary>
        /// List view of files and folders currently displayed in the tab.
        /// </summary>
        public ObservableCollection<IFileSystemItem> CurrentFilesAndFolders { get; set; } = new ObservableCollection<IFileSystemItem>();

        private IFileSystemItem _selectedItem;

        /// <summary>
        /// Currently selected item in the file list.
        /// </summary>
        public IFileSystemItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private bool _isInvalidated;

        /// <summary>
        /// Indicates whether the content of the tab needs to be refreshed.
        /// </summary>
        public bool IsInvalidated
        {
            get => _isInvalidated;
            set => SetProperty(ref _isInvalidated, value);
        }

        /// <summary>
        /// True if the tab depends on a parent tab (child tab).
        /// </summary>
        public bool IsDependent => ParentTabId.HasValue;
    }
}