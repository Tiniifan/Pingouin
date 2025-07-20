using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using GongSolutions.Wpf.DragDrop;
using StudioElevenLib.Tools;
using StudioElevenLib.Level5.Archive.XPCK;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel : IDropTarget
    {
        #region Private Fields for Drag & Drop
        /// <summary>
        /// Stores the path to the temporary directory created during a drag-out operation.
        /// </summary>
        private string _dragDropTempPath;
        #endregion

        #region IDropTarget Implementation (Drop into Application)
        void IDropTarget.DragEnter(IDropInfo dropInfo)
        {
            // Simply delegate to DragOver for validation logic.
            ((IDropTarget)this).DragOver(dropInfo);
        }

        void IDropTarget.DragLeave(IDropInfo dropInfo)
        {
            // No action needed when cursor leaves the drop zone.
        }

        /// <summary>
        /// Called continuously while an item is being dragged over the drop target.
        /// Determines if a drop is allowed and sets visual feedback (cursor icon).
        /// </summary>
        void IDropTarget.DragOver(IDropInfo dropInfo)
        {
            // Handle tab reordering first - if the dragged object is one of our TabViewModel, we're moving a tab.
            if (dropInfo.Data is TabViewModel)
            {
                // Allow the move operation. Cursor will show a "move" icon.
                dropInfo.Effects = DragDropEffects.Move;

                // Use "Insertion" adorner which displays a vertical line
                // to indicate where the tab will be inserted.
                dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);

                return;
            }

            // Only interested in files dropped from the OS.
            var data = dropInfo.Data as IDataObject;
            if (data == null || !data.GetDataPresent(DataFormats.FileDrop))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            // No archive open. Only accept a single archive file.
            if (!IsArchiveOpen)
            {
                var files = (string[])data.GetData(DataFormats.FileDrop);

                // Allow drop only if a single file is being dragged.
                if (files != null && files.Length == 1 && File.Exists(files[0]))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    dropInfo.DropTargetAdorner = typeof(DropTargetHighlightAdorner);
                }
                else
                {
                    dropInfo.Effects = DragDropEffects.None;
                }

                return;
            }

            // Archive is open. Can import files and folders.
            var droppedPaths = (string[])data.GetData(DataFormats.FileDrop);
            if (droppedPaths == null || !droppedPaths.Any())
            {
                dropInfo.Effects = DragDropEffects.None;

                return;
            }

            // Specific constraint: XPCK archives don't support adding folders.
            if (SelectedTab.ParentArchive is XPCK)
            {
                if (droppedPaths.Any(path => Directory.Exists(path)))
                {
                    dropInfo.Effects = DragDropEffects.None;

                    return;
                }
            }

            // If all checks pass, allow the copy operation.
            dropInfo.Effects = DragDropEffects.Copy;
            dropInfo.DropTargetAdorner = typeof(DropTargetHighlightAdorner);
        }

        /// <summary>
        /// Called when an item is dropped onto the target.
        /// Contains the main logic for handling the dropped data.
        /// </summary>
        void IDropTarget.Drop(IDropInfo dropInfo)
        {
            // Handle tab drop first - using pattern matching to check type and assign variable in one line.
            if (dropInfo.Data is TabViewModel tabToMove)
            {
                int oldIndex = Tabs.IndexOf(tabToMove);
                int newIndex = dropInfo.InsertIndex;

                // GongSolutions may return an index that needs adjustment when moving an element
                // from left to right in the same list. In this case, the destination index is
                // calculated before the source element is removed.
                if (oldIndex < newIndex)
                {
                    newIndex--;
                }

                // Move the element in the ObservableCollection.
                // The Move method is optimized for this and efficiently notifies the UI.
                Tabs.Move(oldIndex, newIndex);

                // Optional but good practice: ensure the tab that was just moved remains selected.
                SelectedTab = tabToMove;

                return;
            }

            // File drop
            var data = dropInfo.Data as IDataObject;
            if (data == null || !data.GetDataPresent(DataFormats.FileDrop)) return;

            var droppedItems = (string[])data.GetData(DataFormats.FileDrop);
            if (droppedItems == null || !droppedItems.Any()) return;

            // No archive is open.
            if (!IsArchiveOpen)
            {
                string filePath = droppedItems.First();
                if (File.Exists(filePath))
                {
                    try
                    {
                        if (SelectedTab == null)
                        {
                            ExecuteAddNewTab(null);
                        }
                        OpenArchive(filePath);
                    }
                    catch (Exception m)
                    {
                        // Fail silently if it's not a valid archive.
                        Console.WriteLine(m);            
                    }
                }
                return;
            }

            // Archive is open. Import the files/folders.
            VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());
            bool changed = false;

            foreach (string path in droppedItems)
            {
                if (File.Exists(path))
                {
                    ImportFile(path, currentDirectory);
                    changed = true;
                }
                else if (Directory.Exists(path))
                {
                    if (SelectedTab.ParentArchive is XPCK)
                    {
                        _dialogService.ShowMessage("Dropping folders into an XPCK archive is not allowed.", "Operation Not Allowed", MessageBoxImage.Error);
                        continue;
                    }
                    ImportDirectoryRecursively(path, currentDirectory);
                    changed = true;
                }
            }

            if (changed)
            {
                PopulateListView(GetCurrentPath());
                PopulateTreeView();
            }
        }

        void IDropTarget.DropHint(IDropHintInfo dropHintInfo)
        {
            // No implementation needed
        }

        /// <summary>
        /// Imports a single file into the specified virtual directory.
        /// </summary>
        private void ImportFile(string filePath, VirtualDirectory targetDirectory)
        {
            string fileName = Path.GetFileName(filePath);

            var fileData = new SubMemoryStream(File.ReadAllBytes(filePath))
            {
                // Mark as new/modified
                Color = Color.Orange
            };

            targetDirectory.Files[fileName] = fileData;
        }

        /// <summary>
        /// Recursively imports a directory and all its contents into the specified virtual directory.
        /// </summary>
        private void ImportDirectoryRecursively(string sourcePath, VirtualDirectory parentTargetDirectory)
        {
            string dirName = Path.GetFileName(sourcePath);
            var newVirtualDir = parentTargetDirectory.GetFolder(dirName);

            if (newVirtualDir == null)
            {
                newVirtualDir = new VirtualDirectory(dirName) { Color = Color.Orange };
                parentTargetDirectory.AddFolder(newVirtualDir);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
                ImportFile(file, newVirtualDir);
            }

            foreach (string subDir in Directory.GetDirectories(sourcePath))
            {
                ImportDirectoryRecursively(subDir, newVirtualDir);
            }
        }
        #endregion

        #region Public Methods for Drag-Out (Called from View's Code-Behind)
        /// <summary>
        /// Prepares the IDataObject for a drag-out operation by extracting selected items to a temporary location.
        /// </summary>
        public IDataObject PrepareDragOutData(IList selectedItems)
        {
            var itemsToDrag = selectedItems.Cast<FileItemViewModel>().ToList();
            if (!itemsToDrag.Any()) return null;

            _dragDropTempPath = Path.Combine(Path.GetTempPath(), "Pingouin_DragDrop_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_dragDropTempPath);

            var extractedPaths = new StringCollection();

            try
            {
                foreach (var item in itemsToDrag)
                {
                    if (item.Type == "Folder")
                    {
                        if (item.Tag is VirtualDirectory folderNode)
                        {
                            string targetFolderPath = Path.Combine(_dragDropTempPath, folderNode.Name);
                            Directory.CreateDirectory(targetFolderPath);
                            ExtractFolderRecursively(folderNode, targetFolderPath);
                            extractedPaths.Add(targetFolderPath);
                        }
                    }
                    else
                    {
                        if (item.Tag is KeyValuePair<string, SubMemoryStream> fileEntry)
                        {
                            string targetFilePath = Path.Combine(_dragDropTempPath, fileEntry.Key);
                            fileEntry.Value.Read();
                            File.WriteAllBytes(targetFilePath, fileEntry.Value.ByteContent);
                            extractedPaths.Add(targetFilePath);
                        }
                    }
                }

                if (extractedPaths.Count > 0)
                {
                    var dataObject = new DataObject();
                    dataObject.SetFileDropList(extractedPaths);
                    return dataObject;
                }

                return null;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"An error occurred while preparing files for drag-and-drop: {ex.Message}", "Drag & Drop Error", MessageBoxImage.Error);
                CleanupDragOutData();
                return null;
            }
        }

        /// <summary>
        /// Recursively extracts a virtual folder and its contents to a physical disk location.
        /// </summary>
        private void ExtractFolderRecursively(VirtualDirectory sourceFolder, string destinationPath)
        {
            foreach (var fileEntry in sourceFolder.Files)
            {
                string filePath = Path.Combine(destinationPath, fileEntry.Key);
                fileEntry.Value.Read();
                File.WriteAllBytes(filePath, fileEntry.Value.ByteContent);
            }

            foreach (var subFolder in sourceFolder.Folders)
            {
                string subFolderPath = Path.Combine(destinationPath, subFolder.Name);
                Directory.CreateDirectory(subFolderPath);
                ExtractFolderRecursively(subFolder, subFolderPath);
            }
        }

        /// <summary>
        /// Safely deletes the temporary directory used for the drag-out operation.
        /// </summary>
        public void CleanupDragOutData()
        {
            if (!string.IsNullOrEmpty(_dragDropTempPath) && Directory.Exists(_dragDropTempPath))
            {
                try
                {
                    Directory.Delete(_dragDropTempPath, true);
                }
                catch (Exception)
                {
                    // Ignore exceptions during cleanup.
                }
            }

            _dragDropTempPath = null;
        }
        #endregion
    }
}