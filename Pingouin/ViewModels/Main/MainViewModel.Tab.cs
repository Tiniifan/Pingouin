using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using StudioElevenLib.Level5.Archive;
using StudioElevenLib.Tools;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        #region Tab Properties

        /// <summary>
        /// Gets the collection of all open tabs.
        /// Using an ObservableCollection ensures the UI updates automatically when tabs are added or removed.
        /// </summary>
        public ObservableCollection<TabViewModel> Tabs { get; } = new ObservableCollection<TabViewModel>();

        private TabViewModel _selectedTab;
        /// <summary>
        /// Gets or sets the currently active tab.
        /// The setter is responsible for triggering a UI update to reflect the selected tab's state.
        /// </summary>
        public TabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab == value) return;

                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));

                // Update the entire UI state to reflect the newly selected tab.
                LoadStateFromSelectedTab();
            }
        }

        #endregion

        #region Tab Commands
        /// <summary>
        /// Gets the command to add a new, empty tab.
        /// </summary>
        public ICommand AddNewTabCommand { get; private set; }

        /// <summary>
        /// Gets the command to close a specified tab.
        /// </summary>
        public ICommand CloseTabCommand { get; private set; }

        /// <summary>
        /// Gets the command to open a nested archive in a new tab.
        /// </summary>
        public ICommand OpenArchiveInNewTabCommand { get; private set; }
        #endregion

        /// <summary>
        /// Initializes tab-related commands and state.
        /// This is called from the main constructor.
        /// </summary>
        private void InitializeTabs()
        {
            AddNewTabCommand = new RelayCommand(ExecuteAddNewTab);
            CloseTabCommand = new RelayCommand(ExecuteCloseTab);
            OpenArchiveInNewTabCommand = new RelayCommand(ExecuteOpenArchiveInNewTab, CanOpenItemInNewTab);
        }

        /// <summary>
        /// Command execution logic to create and select a new blank tab.
        /// </summary>
        private void ExecuteAddNewTab(object parameter)
        {
            var newTab = new TabViewModel
            {
                Header = "New Tab",
                ArchiveFilePath = null,
                ParentArchive = null,
                RootDirectory = null,
            };
            newTab.NavigationHistory.Add("/");

            Tabs.Add(newTab);
            SelectedTab = newTab;
        }

        /// <summary>
        /// Command execution logic to close a tab.
        /// </summary>
        private void ExecuteCloseTab(object parameter)
        {
            if (!(parameter is TabViewModel tabToClose)) return;

            int closingTabIndex = Tabs.IndexOf(tabToClose);

            // Also close any tabs that were opened from this one (dependent tabs).
            var dependentTabs = Tabs.Where(t => t.ParentTabId == tabToClose.Id).ToList();
            foreach (var dependentTab in dependentTabs)
            {
                Tabs.Remove(dependentTab);
            }

            Tabs.Remove(tabToClose);

            // If the closed tab was the selected one, select another one.
            // If no tabs remain, SelectedTab will become null, which is the desired behavior.
            if (SelectedTab == tabToClose)
            {
                if (closingTabIndex > 0 && Tabs.Count > 0)
                {
                    // Select the previous tab.
                    SelectedTab = Tabs[closingTabIndex - 1];
                }
                else
                {
                    // Select the first available tab, or null if none.
                    SelectedTab = Tabs.FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// Opens a file (like a nested archive) in a new, independent tab.
        /// Typically used when double-clicking an archive file within another archive.
        /// </summary>
        private void ExecuteOpenArchiveInNewTab(object parameter)
        {
            var itemToOpen = (parameter as IFileSystemItem) ?? SelectedItem;
            if (itemToOpen == null || itemToOpen.Type == "Folder") return;

            try
            {
                // Extract the file content from the currently open archive in memory.
                VirtualDirectory currentDirectory = SelectedTab.ParentArchive.Directory.GetFolderFromFullPath(GetCurrentPath());
                byte[] fileContent = currentDirectory.GetFileFromFullPath(itemToOpen.Name);

                // Attempt to parse the file content as a new archive.
                IArchive newArchive = Archiver.GetArchive(fileContent);

                if (newArchive != null)
                {
                    // If successful, create and switch to a new tab to host the archive.
                    var newTab = new TabViewModel { Header = "Loading..." };
                    Tabs.Add(newTab);
                    SelectedTab = newTab;
                    OpenArchive(newArchive, $"memory://{itemToOpen.Name}"); // The path indicates it's from memory.
                }
                else
                {
                    _dialogService.ShowMessage($"'{itemToOpen.Name}' is not a supported archive type.", "Cannot Open");
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Failed to open '{itemToOpen.Name}' as archive: {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Loads the MainViewModel's state from the currently selected TabViewModel.
        /// This is the core synchronization mechanism between tabs and the main view.
        /// </summary>
        private void LoadStateFromSelectedTab()
        {
            if (SelectedTab == null)
            {
                // Reset the UI to its default, empty state.
                _archiveOpened = null;
                Title = "Pingouin";
                CurrentArchiveName = "No Archive";
                RootFolders.Clear();
                CurrentFilesAndFolders.Clear();
                AddressBarText = "/";
                OnPropertyChanged(nameof(IsArchiveOpen)); // Notify that the archive is now closed.
            }
            else
            {
                // Handle cases where the source file/folder for a tab might have been moved or deleted.
                if (SelectedTab.IsInvalidated)
                {
                    _dialogService.ShowMessage("The source folder for this tab has been deleted.", "Error", MessageBoxImage.Error);
                    CloseTabCommand.Execute(SelectedTab);
                    return;
                }

                _archiveOpened = SelectedTab.ParentArchive;
                OnPropertyChanged(nameof(IsArchiveOpen));

                if (IsArchiveOpen)
                {
                    // Update UI elements with data from the selected tab.
                    var archiveName = Path.GetFileName(SelectedTab.ArchiveFilePath);
                    Title = $"Pingouin - {archiveName}";
                    CurrentArchiveName = SelectedTab.IsDependent ? $"{archiveName} - {SelectedTab.Header}" : archiveName;
                    AddressBarText = GetCurrentPath();
                    PopulateTreeView();
                    PopulateListView(GetCurrentPath());
                }
                else
                {
                    // If the tab doesn't have an archive, reset to the default state.
                    Title = "Pingouin";
                    CurrentArchiveName = "No Archive";
                    RootFolders.Clear();
                    CurrentFilesAndFolders.Clear();
                    AddressBarText = "/";
                }
            }

            // After any state change, force commands to re-evaluate their CanExecute status.
            ((RelayCommand)SaveArchiveCommand).RaiseCanExecuteChanged();
            ((RelayCommand)NewFolderCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ImportFilesCommand).RaiseCanExecuteChanged();
            ((RelayCommand)NavigateBackCommand).RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Gets the current navigation path from the selected tab's history.
        /// </summary>
        private string GetCurrentPath()
        {
            if (SelectedTab == null || !SelectedTab.NavigationHistory.Any())
            {
                return "/"; // Default path if no tab or history exists.
            }
            return SelectedTab.NavigationHistory.Last();
        }
    }
}