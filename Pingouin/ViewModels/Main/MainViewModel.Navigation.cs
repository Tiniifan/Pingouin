using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using Pingouin.Models;
using StudioElevenLib.Tools;
using Pingouin.Services;
using System;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        #region Navigation Properties
        private readonly FileTypeService _fileTypeService = new FileTypeService();

        private string _addressBarText = "/";
        /// <summary>
        /// Gets or sets the current address bar text and handles navigation history
        /// </summary>
        public string AddressBarText
        {
            get => _addressBarText;
            set
            {
                if (SetProperty(ref _addressBarText, value))
                {
                    if (SelectedTab != null)
                    {
                        // Add to navigation history if path is different from last entry
                        if (!SelectedTab.NavigationHistory.Any() || SelectedTab.NavigationHistory.Last() != value)
                        {
                            SelectedTab.NavigationHistory.Add(value);
                        }
                        PopulateListView(value);
                    }
                    OnPropertyChanged(nameof(CanNavigateBack));
                }
            }
        }

        /// <summary>
        /// Determines if back navigation is possible based on navigation history
        /// </summary>
        public bool CanNavigateBack => SelectedTab != null && SelectedTab.NavigationHistory.Count > 1;

        /// <summary>
        /// Indicates whether an item is currently selected in the file list
        /// </summary>
        public bool IsItemSelected => SelectedItem != null;

        public ObservableCollection<FolderNodeViewModel> RootFolders { get; } = new ObservableCollection<FolderNodeViewModel>();
        public ObservableCollection<IFileSystemItem> CurrentFilesAndFolders { get; } = new ObservableCollection<IFileSystemItem>();

        /// <summary>
        /// Gets or sets the currently selected file or folder item
        /// </summary>
        public IFileSystemItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    OnPropertyChanged(nameof(IsItemSelected));

                    // Update command states when selection changes
                    ((RelayCommand)ExportCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)RenameItemCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteItemCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ReplaceItemCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)OpenItemInNewTabCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)OpenArchiveInNewTabCommand).RaiseCanExecuteChanged();

                    _ = UpdatePreviewImageAsync();
                }
            }
        }
        #endregion

        #region Navigation Command Implementations
        /// <summary>
        /// Navigates back to the previous location in history
        /// </summary>
        private void ExecuteNavigateBack(object parameter)
        {
            if (!CanNavigateBack) return;

            // Remove current page from history
            SelectedTab.NavigationHistory.RemoveAt(SelectedTab.NavigationHistory.Count - 1);

            // The new "current page" is now the last in the list
            string newPath = GetCurrentPath();

            // Update address bar and view WITHOUT adding to new history
            _addressBarText = newPath;
            OnPropertyChanged(nameof(AddressBarText));
            PopulateListView(newPath);
            OnPropertyChanged(nameof(CanNavigateBack));
        }

        /// <summary>
        /// Navigates into the selected folder item
        /// </summary>
        private void ExecuteNavigateIntoSelectedItem(object parameter)
        {
            if (parameter is IFileSystemItem item && item.Type == "Folder")
            {
                AddressBarText = GetCurrentPath() + item.Name + "/";
            }
        }

        /// <summary>
        /// Navigates to a specific folder from the tree view
        /// </summary>
        private void ExecuteNavigateToFolder(object parameter)
        {
            if (parameter is FolderNodeViewModel folderNode)
            {
                // For dependent tabs, FullPath is relative to archive root
                // but we want to navigate relative to tab root
                if (SelectedTab.IsDependent)
                {
                    // Remove tab root portion from full path
                    var rootPath = SelectedTab.RootDirectory.GetFullPath(_archiveOpened.Directory) + "/";
                    var relativePath = folderNode.FullPath.StartsWith(rootPath)
                        ? "/" + folderNode.FullPath.Substring(rootPath.Length)
                        : folderNode.FullPath;
                    AddressBarText = relativePath;
                }
                else
                {
                    AddressBarText = folderNode.FullPath;
                }
            }
        }
        #endregion

        #region View Population Methods
        /// <summary>
        /// Populates the tree view with folder hierarchy
        /// </summary>
        private void PopulateTreeView()
        {
            RootFolders.Clear();
            if (_archiveOpened == null) return;

            var rootToShow = SelectedTab.IsDependent ? SelectedTab.RootDirectory : _archiveOpened.Directory;
            var rootPath = SelectedTab.IsDependent ? SelectedTab.RootDirectory.GetFullPath(_archiveOpened.Directory) + "/" : "/";

            var rootNode = CreateFolderNodeViewModel(rootToShow, rootPath);
            rootNode.IsExpanded = true;
            RootFolders.Add(rootNode);
        }

        /// <summary>
        /// Creates a folder node view model with calculated size and child folders
        /// </summary>
        private FolderNodeViewModel CreateFolderNodeViewModel(VirtualDirectory folder, string fullPath)
        {
            string defaultName = (fullPath == "/" && SelectedTab != null) ? Path.GetFileName(SelectedTab.ArchiveFilePath) : "root";

            var node = new FolderNodeViewModel
            {
                Name = folder.Name ?? defaultName,
                FullPath = fullPath,
                Size = FormatSize(folder.GetSize()),
                Tag = folder
            };

            // Recursively create child folder nodes
            foreach (var subFolder in folder.Folders.OrderBy(f => f.Name))
            {
                string subFolderPath = (fullPath == "/" ? "/" : fullPath) + subFolder.Name + "/";
                node.Children.Add(CreateFolderNodeViewModel(subFolder, subFolderPath));
            }

            return node;
        }

        /// <summary>
        /// Populates the list view with files and folders at the specified path
        /// </summary>
        private void PopulateListView(string path)
        {
            CurrentFilesAndFolders.Clear();
            if (_archiveOpened == null || string.IsNullOrEmpty(path)) return;

            // Path is relative to tab root, combine with tab root
            string fullPathInArchive;

            if (SelectedTab.IsDependent)
            {
                var rootPath = SelectedTab.RootDirectory.GetFullPath(_archiveOpened.Directory);

                // Handle case where rootPath is empty (RootDirectory is archive root)
                if (string.IsNullOrEmpty(rootPath))
                {
                    fullPathInArchive = path;
                }
                else
                {
                    fullPathInArchive = Path.Combine(rootPath, path.TrimStart('/')).Replace('\\', '/') + "/";
                    if (path == "/") fullPathInArchive = rootPath + "/";
                }
            }
            else
            {
                fullPathInArchive = path;
            }

            VirtualDirectory currentDirectory;
            try
            {
                currentDirectory = _archiveOpened.Directory.GetFolderFromFullPath(fullPathInArchive);
            }
            catch (DirectoryNotFoundException)
            {
                // Handle case where path no longer exists (may happen if another tab deleted it)
                _dialogService.ShowMessage($"Path not found: {fullPathInArchive}", "Navigation Error");

                // Return to root path
                AddressBarText = "/";

                return;
            }

            if (currentDirectory == null) return;

            // Add folders to the list
            foreach (var folder in currentDirectory.Folders.OrderBy(f => f.Name))
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

            // Add files to the list
            foreach (var file in currentDirectory.Files.OrderBy(f => f.Key))
            {
                if (file.Value.Color == System.Drawing.Color.Orange)
                {
                    Console.WriteLine("test");
                }
                CurrentFilesAndFolders.Add(new FileItemViewModel
                {
                    Icon = _fileTypeService.GetIconForFile(file.Key),
                    Name = file.Key,
                    Size = FormatSize(file.Value.Size),
                    Type = _fileTypeService.GetTypeName(file.Key),
                    ForeColor = file.Value.Color,
                    Tag = file
                });
            }
        }

        /// <summary>
        /// Formats file size in bytes to human-readable format (B, KB, MB, GB)
        /// </summary>
        private string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1024) return $"{sizeInBytes} B";
            if (sizeInBytes < 1024 * 1024) return $"{(double)sizeInBytes / 1024:0.00} KB";
            if (sizeInBytes < 1024 * 1024 * 1024) return $"{(double)sizeInBytes / (1024 * 1024):0.00} MB";
            return $"{(double)sizeInBytes / (1024 * 1024 * 1024):0.00} GB";
        }
        #endregion
    }
}