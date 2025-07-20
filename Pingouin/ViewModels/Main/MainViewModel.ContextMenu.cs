using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using StudioElevenLib.Tools;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        #region Context Menu Command Implementations

        /// <summary>
        /// Determines if a context menu command can be executed on the selected item.
        /// </summary>
        /// <param name="parameter">The command parameter, expected to be an IFileSystemItem.</param>
        /// <returns>True if the parameter is a valid file system item; otherwise, false.</returns>
        private bool CanExecuteOnSelectedItem(object parameter)
        {
            return parameter is IFileSystemItem;
        }

        /// <summary>
        /// Determines if the selected item can be opened in a new tab.
        /// </summary>
        /// <param name="parameter">The item to check.</param>
        /// <returns>True if the item is a folder or a supported archive type; otherwise, false.</returns>
        private bool CanOpenItemInNewTab(object parameter)
        {
            var item = (parameter as IFileSystemItem) ?? SelectedItem;

            if (item == null)
            {
                return false;
            }

            // Folders can always be opened in a new tab
            if (item.Type == "Folder")
            {
                return true;
            }

            // Check if the file is a recognized archive type that can be opened
            string typeName = _fileTypeService.GetTypeName(item.Name);

            return typeName == "Archive File";
        }

        /// <summary>
        /// Executes the rename operation for a selected file or folder
        /// </summary>
        /// <param name="parameter">The IFileSystemItem to rename.</param>
        private void ExecuteRenameItem(object parameter)
        {
            var itemToRename = parameter as IFileSystemItem;
            if (itemToRename == null) return;

            // Prompt the user for a new name using an input dialog
            string newName = _dialogService.ShowInputBox(
                $"Enter new name for '{itemToRename.Name}':",
                "Rename",
                itemToRename.Name);

            // Exit if the user cancelled or provided the same name
            if (string.IsNullOrWhiteSpace(newName) || newName.Equals(itemToRename.Name, StringComparison.Ordinal))
                return;

            VirtualDirectory currentDirectory =
                SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

            if (itemToRename.Type == "Folder")
            {
                var folder = itemToRename.Tag as VirtualDirectory;

                if (folder == null) return;

                // Check for name collision with other folders
                if (currentDirectory.Folders.Any(f => f != folder && f.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                {
                    _dialogService.ShowMessage($"A folder named '{newName}' already exists.", "Error");
                    return;
                }

                folder.Name = newName;

                // Mark as modified
                folder.Color = System.Drawing.Color.Orange;
            }
            else
            {
                // Check for name collision with other files.
                if (currentDirectory.Files.ContainsKey(newName))
                {
                    _dialogService.ShowMessage($"A file named '{newName}' already exists.", "Error");

                    return;
                }

                var fileEntry = (KeyValuePair<string, SubMemoryStream>)itemToRename.Tag;

                // Rename by removing the old entry and adding a new one.
                currentDirectory.Files.Remove(fileEntry.Key);

                // Mark as modified
                fileEntry.Value.Color = System.Drawing.Color.Orange; 
                currentDirectory.Files.Add(newName, fileEntry.Value);
            }

            // Refresh the UI to show the changes.
            PopulateTreeView();
            PopulateListView(GetCurrentPath());
        }

        /// <summary>
        /// Executes the deletion of a selected file or folder.
        /// </summary>
        /// <param name="parameter">The IFileSystemItem to delete.</param>
        private void ExecuteDeleteItem(object parameter)
        {
            var itemToDelete = parameter as IFileSystemItem;
            if (itemToDelete == null) return;

            // Confirm the action with the user.
            bool confirmed = _dialogService.ShowConfirmation(
                $"Are you sure you want to delete '{itemToDelete.Name}'?",
                "Confirm Deletion");

            if (!confirmed) return;

            VirtualDirectory currentDirectory =
                SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

            if (itemToDelete.Type == "Folder")
            {
                var folder = itemToDelete.Tag as VirtualDirectory;
                if (folder != null)
                {
                    currentDirectory.Folders.Remove(folder);

                    // Find any open tabs that are displaying the content of this deleted folder and mark them as invalid.
                    var dependentTabs = Tabs
                        .Where(t => t.RootDirectory == folder)
                        .ToList();

                    foreach (var tab in dependentTabs)
                    {
                        tab.IsInvalidated = true;
                    }
                }
            }
            else
            {
                var fileEntry = (KeyValuePair<string, SubMemoryStream>)itemToDelete.Tag;
                currentDirectory.Files.Remove(fileEntry.Key);
            }

            PopulateTreeView();
            PopulateListView(GetCurrentPath());
            SelectedItem = null;
        }

        /// <summary>
        /// Replaces the content of a selected item with content from the physical file system.
        /// </summary>
        /// <param name="parameter">The IFileSystemItem to replace.</param>
        private void ExecuteReplaceItem(object parameter)
        {
            var selectedItem = parameter as IFileSystemItem;
            if (selectedItem == null) return;

            VirtualDirectory parentDirectory =
                SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

            if (selectedItem.Type == "Folder")
            {
                // Let user select a physical folder to replace the virtual one
                string folderPath = _dialogService.ShowSelectFolderDialog("Select a folder to replace with");

                if (string.IsNullOrEmpty(folderPath)) return;

                var folderToReplace = selectedItem.Tag as VirtualDirectory;
                if (folderToReplace == null) return;

                // Clear the existing content and import the new one
                folderToReplace.Files.Clear();
                folderToReplace.Folders.Clear();
                ImportPhysicalFolder(folderToReplace, folderPath);

                // Mark as modified
                folderToReplace.Color = System.Drawing.Color.Orange; 
            }
            else
            {
                // Let user select a physical file to replace the virtual one.
                string filePath = _dialogService.ShowOpenFileDialog("Select a file to replace with");

                if (string.IsNullOrEmpty(filePath)) return;

                // Create a new stream for the replacement file.
                var fileData = new SubMemoryStream(
                    new FileStream(filePath, FileMode.Open, FileAccess.Read),
                    0,
                    new FileInfo(filePath).Length);

                // Mark as modified
                fileData.Color = System.Drawing.Color.Orange; 

                parentDirectory.Files[selectedItem.Name] = fileData;
            }

            PopulateTreeView();
            PopulateListView(GetCurrentPath());
        }

        /// <summary>
        /// Opens a selected folder in a new tab.
        /// </summary>
        /// <param name="parameter">The folder to open.</param>
        private void ExecuteOpenItemInNewTab(object parameter)
        {
            var itemToOpen = parameter as IFileSystemItem;

            // This action is only for folders.
            if (itemToOpen?.Type != "Folder") return;

            // Get the VirtualDirectory object associated with the item.
            var folderToOpen = itemToOpen.Tag as VirtualDirectory;

            if (folderToOpen == null) return;

            // Create and configure a new tab.
            var newTab = new TabViewModel
            {
                Header = itemToOpen.Name,
                ParentArchive = SelectedTab.ParentArchive,
                RootDirectory = folderToOpen,
                ParentTabId = SelectedTab.Id
            };

            newTab.NavigationHistory.Add("/");

            Tabs.Add(newTab);

            SelectedTab = newTab; 
        }

        /// <summary>
        /// Exports the entire content of the current folder to a physical directory.
        /// </summary>
        private void ExecuteExportCurrentFolder(object parameter)
        {
            string exportPath = _dialogService.ShowSelectFolderDialog("Select destination folder to export to");

            if (string.IsNullOrEmpty(exportPath)) return;

            try
            {
                VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());
                ExportVirtualDirectory(currentDirectory, exportPath);
                _dialogService.ShowMessage($"Folder '{currentDirectory.Name ?? "root"}' exported successfully!", "Export Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to export folder: {ex.Message}", "Export Error");
            }
        }

        /// <summary>
        /// Changes the prefix of all files in the current directory that have a prefix.
        /// A prefix is defined as the part of the filename before an underscore '_'.
        /// </summary>
        private void ExecuteChangeFilePrefixes(object parameter)
        {
            VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());
            string newPrefix = _dialogService.ShowInputBox("Enter the new prefix for files:", "Change Prefix");

            if (string.IsNullOrWhiteSpace(newPrefix)) return;

            var filesToRename = currentDirectory.Files.Where(f => f.Key.Contains("_")).ToList();
            bool changed = false;

            foreach (var fileEntry in filesToRename)
            {
                int separatorIndex = fileEntry.Key.IndexOf('_');
                if (separatorIndex >= 0)
                {
                    // Construct the new filename with the new prefix.
                    string suffix = fileEntry.Key.Substring(separatorIndex);
                    string newFilename = newPrefix + suffix;

                    // Proceed only if the new filename doesn't already exist.
                    if (!currentDirectory.Files.ContainsKey(newFilename))
                    {
                        var oldItem = fileEntry.Value;

                        // Mark as modified
                        oldItem.Color = System.Drawing.Color.Orange; 

                        currentDirectory.Files.Remove(fileEntry.Key);
                        currentDirectory.Files.Add(newFilename, oldItem);
                        changed = true;
                    }
                }
            }

            // Refresh the UI only if changes were made.
            if (changed) PopulateListView(GetCurrentPath());
        }

        #endregion

        #region Multi-Select Context Menu Implementations

        /// <summary>
        /// Determines if a context menu command can be executed on multiple selected items.
        /// </summary>
        /// <param name="parameter">The command parameter, expected to be a list of items.</param>
        /// <returns>True if the parameter is a non-empty list; otherwise, false.</returns>
        private bool CanExecuteOnMultiple(object parameter)
        {
            return parameter is System.Collections.IList list && list.Count > 0;
        }

        /// <summary>
        /// Deletes multiple selected files and/or folders.
        /// </summary>
        /// <param name="parameter">A list of IFileSystemItem objects to delete.</param>
        private void ExecuteDeleteMultiple(object parameter)
        {
            if (!(parameter is System.Collections.IList selectedItemsList) || selectedItemsList.Count == 0) return;
            var itemsToDelete = selectedItemsList.Cast<IFileSystemItem>().ToList();

            bool confirmed = _dialogService.ShowConfirmation($"Are you sure you want to delete {itemsToDelete.Count} selected items?", "Confirm Deletion");
            if (!confirmed) return;

            VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

            foreach (var item in itemsToDelete)
            {
                if (item.Type == "Folder")
                {
                    var folder = item.Tag as VirtualDirectory;
                    if (folder != null)
                    {
                        currentDirectory.Folders.Remove(folder);

                        // Invalidate any tabs that are displaying the deleted folder's content.
                        var dependentTabs = Tabs.Where(t => t.RootDirectory == folder).ToList();

                        foreach (var tab in dependentTabs)
                        {
                            tab.IsInvalidated = true;
                        }           
                    }
                }
                else
                {
                    var fileEntry = (KeyValuePair<string, SubMemoryStream>)item.Tag;
                    currentDirectory.Files.Remove(fileEntry.Key);
                }
            }

            PopulateTreeView();
            PopulateListView(GetCurrentPath());
            SelectedItem = null;
        }

        /// <summary>
        /// Exports multiple selected files and/or folders to a physical directory.
        /// </summary>
        /// <param name="parameter">A list of IFileSystemItem objects to export.</param>
        private void ExecuteExportMultiple(object parameter)
        {
            if (!(parameter is System.Collections.IList selectedItemsList) || selectedItemsList.Count == 0) return;

            var itemsToExport = selectedItemsList.Cast<IFileSystemItem>().ToList();

            string exportPath = _dialogService.ShowSelectFolderDialog($"Select destination folder for {itemsToExport.Count} items");

            if (string.IsNullOrEmpty(exportPath)) return;

            try
            {
                foreach (var item in itemsToExport)
                {
                    if (item.Type == "Folder")
                    {
                        // Use the recursive helper to export the entire folder.
                        var folder = item.Tag as VirtualDirectory;

                        if (folder != null)
                        {
                            ExportVirtualDirectory(folder, exportPath);
                        }                
                    }
                    else
                    {
                        // Export the individual file.
                        var fileEntry = (KeyValuePair<string, SubMemoryStream>)item.Tag;

                        string filePath = Path.Combine(exportPath, fileEntry.Key);

                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            fileEntry.Value.Seek();
                            fileEntry.Value.CopyTo(fileStream);
                        }
                    }
                }

                _dialogService.ShowMessage($"{itemsToExport.Count} items exported successfully!", "Export Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to export items: {ex.Message}", "Export Error");
            }
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// Recursively imports a physical folder structure into a virtual directory.
        /// </summary>
        /// <param name="virtualTarget">The virtual directory to import into.</param>
        /// <param name="physicalSourcePath">The path of the physical folder to import from.</param>
        private void ImportPhysicalFolder(VirtualDirectory virtualTarget, string physicalSourcePath)
        {
            // Import all files from the current physical directory.
            foreach (string filePath in Directory.GetFiles(physicalSourcePath))
            {
                string fileName = Path.GetFileName(filePath);

                var fileData = new SubMemoryStream(new FileStream(filePath, FileMode.Open, FileAccess.Read), 0, new FileInfo(filePath).Length);

                // Mark as new/modified
                fileData.Color = System.Drawing.Color.Orange; 

                virtualTarget.Files[fileName] = fileData;
            }

            // Recursively import all subdirectories.
            foreach (string dirPath in Directory.GetDirectories(physicalSourcePath))
            {
                string dirName = Path.GetFileName(dirPath);
                var subFolder = new VirtualDirectory(dirName) { Color = System.Drawing.Color.Orange };
                virtualTarget.Folders.Add(subFolder);
                ImportPhysicalFolder(subFolder, dirPath);
            }
        }

        /// <summary>
        /// Recursively exports a virtual directory structure to a physical location.
        /// </summary>
        /// <param name="virtualSource">The virtual directory to export.</param>
        /// <param name="physicalDestinationPath">The physical path where the directory will be created.</param>
        private void ExportVirtualDirectory(VirtualDirectory virtualSource, string physicalDestinationPath)
        {
            // Create the corresponding physical folder.
            string newFolderPath = Path.Combine(physicalDestinationPath, virtualSource.Name ?? "root");
            Directory.CreateDirectory(newFolderPath);

            // Export all files within the virtual directory to the new physical folder.
            foreach (var fileEntry in virtualSource.Files)
            {
                string filePath = Path.Combine(newFolderPath, fileEntry.Key);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fileEntry.Value.Seek();
                    fileEntry.Value.CopyTo(fileStream);
                }
            }

            // Recursively export all subdirectories.
            foreach (var subFolder in virtualSource.Folders)
            {
                ExportVirtualDirectory(subFolder, newFolderPath);
            }
        }
        #endregion
    }
}