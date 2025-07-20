using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DrawingColor = System.Drawing.Color;
using StudioElevenLib.Tools;
using StudioElevenLib.Level5.Archive;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        #region Archive Command Implementations

        /// <summary>
        /// Opens an archive file through file dialog and loads it into the current or new tab
        /// </summary>
        private void ExecuteOpenArchive(object parameter)
        {
            string filePath = _dialogService.ShowOpenFileDialog();

            if (string.IsNullOrEmpty(filePath)) return;

            // Use current tab if empty, otherwise create a new one
            if (SelectedTab == null || SelectedTab.ParentArchive != null)
            {
                ExecuteAddNewTab(null);
            }

            OpenArchive(filePath);
        }

        /// <summary>
        /// Saves the current archive with progress tracking and performance metrics
        /// </summary>
        private async void ExecuteSaveArchive(object parameter)
        {
            if (SelectedTab?.ParentArchive == null) return;

            string defaultFileName = Path.GetFileName(SelectedTab.ArchiveFilePath);
            string savePath = _dialogService.ShowSaveFileDialog(defaultFileName);

            if (string.IsNullOrEmpty(savePath)) return;

            // Enable loading overlay
            IsSaving = true;
            SaveProgressPercentage = 0;

            // Start performance timer
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var progress = new Progress<int>(p => SaveProgressPercentage = p);

                // Check if saving to the same file (requires special handling)
                bool isSameFile = string.Equals(SelectedTab.ArchiveFilePath, savePath, StringComparison.OrdinalIgnoreCase);

                if (isSameFile)
                {
                    // Save to same file (uses temporary file)
                    await SaveToSameFile(savePath, progress);
                }
                else
                {
                    // Save to new file
                    await Task.Run(() => SelectedTab.ParentArchive.Save(savePath, progress));
                }

                // Calculate and format elapsed time
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;

                string timeFormatted;
                if (elapsedTime.TotalSeconds < 1)
                {
                    timeFormatted = $"{elapsedTime.Milliseconds} ms";
                }
                else if (elapsedTime.TotalMinutes < 1)
                {
                    timeFormatted = $"{elapsedTime.TotalSeconds:F1} s";
                }
                else
                {
                    timeFormatted = $"{elapsedTime.Minutes}m {elapsedTime.Seconds}s";
                }

                _dialogService.ShowMessage($"Archive saved successfully!\nSave time: {timeFormatted}", "Save Complete");

                // Reload tab with the saved file
                OpenArchive(savePath);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _dialogService.ShowMessage($"Failed to save archive: {ex.Message}", "Save Error", System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Creates a new folder in the current directory with duplicate name validation
        /// </summary>
        private void ExecuteNewFolder(object parameter)
        {
            string folderName = _dialogService.ShowInputBox("Enter folder name:", "New Folder");

            if (string.IsNullOrWhiteSpace(folderName)) return;

            VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());
            if (currentDirectory.Folders.Any(f => f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase)))
            {
                _dialogService.ShowMessage($"The folder '{folderName}' already exists.", "Error", System.Windows.MessageBoxImage.Error);
            }
            else
            {
                currentDirectory.AddFolder(folderName);

                // Mark as modified
                currentDirectory.Color = DrawingColor.Orange; 
                PopulateTreeView();

                PopulateListView(GetCurrentPath());
            }
        }

        /// <summary>
        /// Imports multiple files into the current directory
        /// </summary>
        private void ExecuteImportFiles(object parameter)
        {
            var filesToImport = _dialogService.ShowImportFilesDialog();

            if (!filesToImport.Any()) return;

            VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

            bool changed = false;

            foreach (string file in filesToImport)
            {
                string fileName = Path.GetFileName(file);
                var fileData = new SubMemoryStream(new FileStream(file, FileMode.Open, FileAccess.Read), 0, new FileInfo(file).Length);

                // Mark as modified
                fileData.Color = DrawingColor.Orange; 

                currentDirectory.Files[fileName] = fileData;
                changed = true;
            }

            if (changed)
            {
                PopulateListView(GetCurrentPath());
            }
        }

        /// <summary>
        /// Exports selected item (file/folder) or current directory if nothing selected
        /// </summary>
        private void ExecuteExport(object parameter)
        {
            if (SelectedTab?.ParentArchive == null) return;

            if (SelectedItem == null)
            {
                // Nothing selected -> Export current directory

                VirtualDirectory currentDir = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());

                // Root directory name is null, use archive filename as fallback
                string dirName = currentDir.Name ?? Path.GetFileNameWithoutExtension(SelectedTab.ArchiveFilePath);

                ExportDirectory(currentDir, dirName);
            }
            else if (SelectedItem.Type == "Folder")
            {
                // Folder selected -> Export this folder

                var folderToExport = (VirtualDirectory)SelectedItem.Tag;
                ExportDirectory(folderToExport, SelectedItem.Name);
            }
            else
            {
                // File selected -> Export this file

                var fileEntry = (System.Collections.Generic.KeyValuePair<string, SubMemoryStream>)SelectedItem.Tag;

                string savePath = _dialogService.ShowExportFileDialog(fileEntry.Key);

                if (!string.IsNullOrEmpty(savePath))
                {
                    try
                    {
                        using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                        {
                            // Reset stream position
                            fileEntry.Value.Seek();
                            fileEntry.Value.CopyTo(fileStream);
                        }

                        _dialogService.ShowMessage($"File '{fileEntry.Key}' exported successfully!", "Export Complete");
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowMessage($"Failed to export file: {ex.Message}", "Export Error", System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        #region Archive Helper Methods

        /// <summary>
        /// Opens an archive from file path, handles file stream creation and error management
        /// </summary>
        private void OpenArchive(string filename)
        {
            try
            {
                // Close previous archive if exists
                if (SelectedTab?.ParentArchive != null)
                {
                    SelectedTab.ParentArchive.Close();
                }

                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var archive = Archiver.GetArchive(fileStream);

                if (archive == null)
                {
                    _dialogService.ShowMessage("Unsupported file type.", "Error", System.Windows.MessageBoxImage.Error);

                    fileStream.Close();

                    return;
                }

                OpenArchive(archive, filename);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to open archive: {ex.Message}", "Opening Error", System.Windows.MessageBoxImage.Error);

                if (SelectedTab != null && SelectedTab.ParentArchive == null)
                {
                    CloseTabCommand.Execute(SelectedTab);
                }
            }
        }

        /// <summary>
        /// Configures tab with archive data and initializes UI state
        /// </summary>
        /// <param name="archive">Archive instance to load</param>
        /// <param name="displayNameOrPath">File path or memory identifier (e.g. "memory://file.fa")</param>
        private void OpenArchive(IArchive archive, string displayNameOrPath)
        {
            // Build search index in background
            Task.Run(() => archive.Directory.BuildSearchIndex());

            // Configure selected tab with new archive
            SelectedTab.Header = Path.GetFileName(displayNameOrPath);
            SelectedTab.ArchiveFilePath = displayNameOrPath;
            SelectedTab.ParentArchive = archive;
            SelectedTab.RootDirectory = archive.Directory;
            SelectedTab.NavigationHistory.Clear();
            SelectedTab.NavigationHistory.Add("/");

            // Reload main view state from configured tab
            LoadStateFromSelectedTab();
        }

        /// <summary>
        /// Saves archive to same file using temporary file to avoid locking issues
        /// </summary>
        private async Task SaveToSameFile(string savePath, IProgress<int> progress)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "PingouinTemp");
            string tempFile = Path.Combine(tempDir, Path.GetFileName(savePath));

            try
            {
                // Create temp directory if needed
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                // Save to temporary file
                await Task.Run(() => SelectedTab.ParentArchive.Save(tempFile, progress));

                // Close current archive to release file lock
                SelectedTab.ParentArchive?.Close();
                SelectedTab.ParentArchive = null;

                // Wait for file to be released
                await Task.Delay(100);

                // Replace original file with temp file
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                File.Move(tempFile, savePath);
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        /// <summary>
        /// Exports a virtual directory to physical file system
        /// </summary>
        private void ExportDirectory(VirtualDirectory directoryToExport, string defaultFolderName)
        {
            string destinationFolder = _dialogService.ShowSelectFolderDialog("Select a destination folder");

            if (string.IsNullOrEmpty(destinationFolder)) return;

            string exportPath = Path.Combine(destinationFolder, defaultFolderName);

            try
            {
                // Create root export directory
                Directory.CreateDirectory(exportPath);

                // Recursively write directory contents
                WriteDirectoryContentsRecursive(directoryToExport, exportPath);

                _dialogService.ShowMessage($"Folder '{defaultFolderName}' exported successfully!", "Export Complete");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to export folder: {ex.Message}", "Export Error", System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Recursively writes virtual directory structure to physical file system
        /// </summary>
        private void WriteDirectoryContentsRecursive(VirtualDirectory virtualDir, string physicalPath)
        {
            // Export all files in current directory
            foreach (var fileEntry in virtualDir.Files)
            {
                string filePath = Path.Combine(physicalPath, fileEntry.Key);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    // Reset stream position before copying
                    fileEntry.Value.Seek(); 
                    fileEntry.Value.CopyTo(fileStream);
                }
            }

            // Recursively export all subdirectories
            foreach (var subDir in virtualDir.Folders)
            {
                string subDirPath = Path.Combine(physicalPath, subDir.Name);
                Directory.CreateDirectory(subDirPath);
                WriteDirectoryContentsRecursive(subDir, subDirPath);
            }
        }

        #endregion
    }
}