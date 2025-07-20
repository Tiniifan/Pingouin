using System.Windows;
using System.Windows.Input;
using StudioElevenLib.Level5.Archive;
using Pingouin.Models;
using Pingouin.Services;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel : BaseViewModel
    {
        #region Private Fields
        private IArchive _archiveOpened;
        private string _archiveFileName;
        private readonly DialogService _dialogService;
        private readonly ThemeService _themeService;
        private IFileSystemItem _selectedItem;
        #endregion

        #region Public Properties
        private string _title = "Pingouin";
        /// <summary>
        /// Application window title
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _currentArchiveName = "No Archive";

        /// <summary>
        /// Display name of the currently opened archive
        /// </summary>
        public string CurrentArchiveName
        {
            get => _currentArchiveName;
            set => SetProperty(ref _currentArchiveName, value);
        }

        /// <summary>
        /// Indicates whether an archive is currently loaded
        /// </summary>
        public bool IsArchiveOpen => _archiveOpened != null;
        #endregion

        #region Commands

        // Archive Commands (Implementation in MainViewModel.Archive.cs)
        public ICommand OpenArchiveCommand { get; }
        public ICommand SaveArchiveCommand { get; }
        public ICommand NewFolderCommand { get; }
        public ICommand ImportFilesCommand { get; }
        public ICommand ExportCommand { get; }

        // Theme Commands (Implementation in MainViewModel.Theme.cs)
        public ICommand PickAccentColorCommand { get; }
        public ICommand ResetAccentColorCommand { get; }

        public ICommand NotImplementedCommand { get; }

        // Navigation Commands (Implementation in MainViewModel.Navigation.cs)
        public ICommand NavigateBackCommand { get; }
        public ICommand NavigateIntoSelectedItemCommand { get; }
        public ICommand NavigateToFolderCommand { get; }

        // Search Commands (Implementation in MainViewModel.Search.cs)
        public ICommand SearchGotFocusCommand { get; }
        public ICommand SearchLostFocusCommand { get; }

        // Context Menu Commands (Implementation in MainViewModel.ContextMenu.cs)
        public ICommand RenameItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ReplaceItemCommand { get; }
        public ICommand OpenItemInNewTabCommand { get; }
        public ICommand ExportCurrentFolderCommand { get; }
        public ICommand ChangeFilePrefixesCommand { get; }
        public ICommand DeleteMultipleCommand { get; }
        public ICommand ExportMultipleCommand { get; }
        #endregion

        public MainViewModel()
        {
            _dialogService = new DialogService();
            _themeService = new ThemeService();

            // Initialize commands with their execution methods and availability conditions
            OpenArchiveCommand = new RelayCommand(ExecuteOpenArchive);
            SaveArchiveCommand = new RelayCommand(ExecuteSaveArchive, _ => IsArchiveOpen);
            NewFolderCommand = new RelayCommand(ExecuteNewFolder, _ => IsArchiveOpen);
            ImportFilesCommand = new RelayCommand(ExecuteImportFiles, _ => IsArchiveOpen);
            ExportCommand = new RelayCommand(ExecuteExport, _ => IsArchiveOpen);

            PickAccentColorCommand = new RelayCommand(ExecutePickAccentColor);
            ResetAccentColorCommand = new RelayCommand(ExecuteResetAccentColor);

            NotImplementedCommand = new RelayCommand(p => _dialogService.ShowMessage($"'{p}' is not implemented yet.", "Info"));

            NavigateBackCommand = new RelayCommand(ExecuteNavigateBack, _ => CanNavigateBack);
            NavigateIntoSelectedItemCommand = new RelayCommand(ExecuteNavigateIntoSelectedItem);
            NavigateToFolderCommand = new RelayCommand(ExecuteNavigateToFolder);

            SearchGotFocusCommand = new RelayCommand(ExecuteSearchGotFocus);
            SearchLostFocusCommand = new RelayCommand(ExecuteSearchLostFocus);

            RenameItemCommand = new RelayCommand(ExecuteRenameItem, CanExecuteOnSelectedItem);
            DeleteItemCommand = new RelayCommand(ExecuteDeleteItem, CanExecuteOnSelectedItem);
            ReplaceItemCommand = new RelayCommand(ExecuteReplaceItem, CanExecuteOnSelectedItem);
            OpenItemInNewTabCommand = new RelayCommand(ExecuteOpenItemInNewTab, CanOpenItemInNewTab);
            ExportCurrentFolderCommand = new RelayCommand(ExecuteExportCurrentFolder, _ => IsArchiveOpen);
            ChangeFilePrefixesCommand = new RelayCommand(ExecuteChangeFilePrefixes, _ => IsArchiveOpen);
            DeleteMultipleCommand = new RelayCommand(ExecuteDeleteMultiple, CanExecuteOnMultiple);
            ExportMultipleCommand = new RelayCommand(ExecuteExportMultiple, CanExecuteOnMultiple);

            // Initialize search text with the default placeholder
            _searchText = PlaceholderText;

            // Initialize Theme properties
            InitializeTheme();

            // Initialize Tab system
            InitializeTabs();
        }
    }
}