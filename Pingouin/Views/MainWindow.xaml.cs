using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Pingouin.Models;
using Pingouin.ViewModels;

namespace Pingouin.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel viewModel && viewModel.SelectedItem != null)
            {
                if (viewModel.SelectedItem.Type == "Folder")
                {
                    viewModel.NavigateIntoSelectedItemCommand.Execute(viewModel.SelectedItem);
                }
                else
                {
                    string[] archiveExtensions = { ".fa", ".xc", ".xb", ".pck", ".zip" };

                    string fileExtension = Path.GetExtension(viewModel.SelectedItem.Name).ToLowerInvariant();

                    if (System.Array.IndexOf(archiveExtensions, fileExtension) >= 0)
                    {
                        if (viewModel.OpenArchiveInNewTabCommand.CanExecute(viewModel.SelectedItem))
                        {
                            viewModel.OpenArchiveInNewTabCommand.Execute(viewModel.SelectedItem);
                        }
                    }
                }
            }
        }

        private void ListViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;

            if (item != null && !item.IsSelected)
            {
                item.IsSelected = true;
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;

            if (item != null && !item.IsSelected)
            {
                item.IsSelected = true;
            }
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var viewModel = DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.SelectedItem = e.NewValue as IFileSystemItem;
            }
        }

        private void WelcomeArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null && viewModel.OpenArchiveCommand.CanExecute(null))
            {
                viewModel.OpenArchiveCommand.Execute(null);
            }
        }
    }
}
