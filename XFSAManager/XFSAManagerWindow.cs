using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.IO;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using XFSAManager.Level5.Archive.ARC0;
using XFSAManager.Tools;

namespace XFSAManager
{
    public partial class XFSAManagerWindow : Form
    {
        private ARC0 ArchiveOpened;

        private bool IsLeftClick = false;

        private ListViewItem SelectedItemContextMenuStrip;

        public XFSAManagerWindow()
        {
            InitializeComponent();
        }

        private string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1024)
            {
                return sizeInBytes.ToString() + " B";
            }
            else if (sizeInBytes < 1024 * 1024)
            {
                double sizeInKB = (double)sizeInBytes / 1024;
                return sizeInKB.ToString("0.00") + " KB";
            }
            else if (sizeInBytes < 1024 * 1024 * 1024)
            {
                double sizeInMB = (double)sizeInBytes / (1024 * 1024);
                return sizeInMB.ToString("0.00") + " MB";
            }
            else
            {
                double sizeInGB = (double)sizeInBytes / (1024 * 1024 * 1024);
                return sizeInGB.ToString("0.00") + " GB";
            }
        }

        private void DrawFolder(string path)
        {
            directoryListView.Items.Clear();
            backButton.Enabled = path != "/";

            VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(path);
            long fullsize = 0;

            foreach (VirtualDirectory folder in currentDirectory.Folders)
            {
                ListViewItem listViewItem = new ListViewItem(folder.Name, 0);
                listViewItem.SubItems.Add(FormatSize(folder.GetSize()));
                listViewItem.Tag = "Folder";
                directoryListView.Items.Add(listViewItem);

                fullsize += folder.GetSize();
            }

            foreach (KeyValuePair<string, SubMemoryStream> file in currentDirectory.Files)
            {
                ListViewItem listViewItem = new ListViewItem(file.Key, 1);
                listViewItem.SubItems.Add(FormatSize(file.Value.Size));
                listViewItem.Tag = "File";
                directoryListView.Items.Add(listViewItem);

                fullsize += file.Value.Size;
            }

            directoryInfoTextBox.Text = "Folder(s): " + currentDirectory.Folders.Count
                + "; File(s): " + currentDirectory.Files.Count
                + "; Size: " + FormatSize(fullsize);
        }

        private void ImportFolder(VirtualDirectory currentDirectory, string selectedDirectory)
        {
            string directoryName = Path.GetFileName(selectedDirectory);

            if (currentDirectory.Folders.Any(x => x.Name == directoryName))
            {
                currentDirectory = currentDirectory.GetFolder(directoryName);
            } else
            {
                currentDirectory.AddFolder(directoryName);
                currentDirectory = currentDirectory.GetFolder(directoryName);
            }

            string[] files = Directory.GetFiles(selectedDirectory);
            foreach (string file in files)
            {
                SubMemoryStream fileData;
                string fileName = Path.GetFileName(file);

                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                long fileOffset = 0;
                long fileSize = fileStream.Length;
                fileData = new SubMemoryStream(fileStream, fileOffset, fileSize);

                if (currentDirectory.Files.ContainsKey(fileName))
                {
                    // Replace
                    currentDirectory.Files[fileName] = fileData;
                } else
                {
                    // Insert new
                    currentDirectory.AddFile(Path.GetFileName(file), fileData);
                }
            }

            string[] subDirectories = Directory.GetDirectories(selectedDirectory);
            foreach (string subDir in subDirectories)
            {
                ImportFolder(currentDirectory, subDir);
            }
        }

        private void ExportFolder(VirtualDirectory currentDirectory, string exportPath)
        {
            string directoryName = currentDirectory.Name;
            string fullPath = Path.Combine(exportPath, directoryName);

            Directory.CreateDirectory(fullPath);

            foreach (var file in currentDirectory.Files)
            {
                string filePath = Path.Combine(fullPath, file.Key);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    // Set the position in the base stream
                    file.Value.Seek();

                    while ((bytesRead = file.Value.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }

            foreach (var subDirectory in currentDirectory.Folders)
            {
                ExportFolder(subDirectory, fullPath);
            }
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Level 5 Archive files (*.fa)|*.fa";
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                directoryListView.Items.Clear();

                ArchiveOpened = new ARC0(new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.Read));

                directoryTextBox.Text = "/";

                saveToolStripMenuItem.Enabled = true;
                backButton.Enabled = true;
                directoryTextBox.Enabled = true;
                directoryListView.Enabled = true;
                directoryInfoTextBox.Enabled = true;

                directoryInfoTextBox.Visible = true;
                progressBar1.Visible = false;
            }
        }

        private void DirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            DrawFolder(directoryTextBox.Text);
        }

        private void DirectoryListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!IsLeftClick) return;

            if (directoryListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = directoryListView.SelectedItems[0];
                string selectedItemType = selectedItem.Tag.ToString();

                if (selectedItemType == "Folder")
                {
                    directoryTextBox.Text += selectedItem.Text + "/";
                }

                IsLeftClick = false;
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            string currentPathName = directoryTextBox.Text;

            int lastSlashIndex = currentPathName.LastIndexOf('/');
            if (lastSlashIndex >= 0)
            {
                int secondToLastSlashIndex = currentPathName.LastIndexOf('/', lastSlashIndex - 1);
                if (secondToLastSlashIndex >= 0)
                {
                    directoryTextBox.Text = currentPathName.Substring(0, secondToLastSlashIndex + 1);
                }
            }
        }

        private void directoryContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (directoryListView.SelectedItems.Count > 0)
            {
                SelectedItemContextMenuStrip = directoryListView.SelectedItems[0];
                string selectedItemType = SelectedItemContextMenuStrip.Tag.ToString();

                folderToolStripMenuItem.Enabled = selectedItemType == "Folder";
                filesToolStripMenuItem.Enabled = selectedItemType == "Folder";

                renameToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
                replaceToolStripMenuItem.Enabled = true;
            }
            else
            {
                SelectedItemContextMenuStrip = null;

                folderToolStripMenuItem.Enabled = true;
                filesToolStripMenuItem.Enabled = true;

                renameToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Enabled = true;
            }
        }

        private void DirectoryListView_MouseDown(object sender, MouseEventArgs e)
        {
            IsLeftClick = e.Button == MouseButtons.Left;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;
            
            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }
            
            string folderName = Interaction.InputBox("Enter folder name:", "New Folder");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                if (currentDirectory.Folders.Any(x => x.Name == folderName))
                {
                    MessageBox.Show("The folder " + folderName + " cannot be created because it already exists.");
                }
                else
                {
                    currentDirectory.AddFolder(folderName);

                    if (SelectedItemContextMenuStrip == null)
                    {
                        ListViewItem listViewItem = new ListViewItem(folderName, 0);
                        listViewItem.SubItems.Add(FormatSize(0));
                        listViewItem.Tag = "Folder";
                        directoryListView.Items.Add(listViewItem);
                    }
                }
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string newFolderName = Path.GetFileName(dialog.FileName);
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                ImportFolder(currentDirectory, dialog.FileName);

                if (SelectedItemContextMenuStrip == null)
                {
                    ListViewItem listViewItem = new ListViewItem(newFolderName, 0);
                    listViewItem.SubItems.Add(FormatSize(currentDirectory.GetSize()));
                    listViewItem.Tag = "Folder";
                    directoryListView.Items.Add(listViewItem);
                }
            }
        }

        private void ImportFilesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            openFileDialog2.Filter = "All files|*.*";
            openFileDialog2.Multiselect = true;
            openFileDialog2.RestoreDirectory = true;

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string[] selectedFiles = openFileDialog2.FileNames;
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                foreach (string file in selectedFiles)
                {
                    SubMemoryStream fileData;
                    string fileName = Path.GetFileName(file);

                    FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    long fileOffset = 0;
                    long fileSize = fileStream.Length;
                    fileData = new SubMemoryStream(fileStream, fileOffset, fileSize);

                    if (currentDirectory.Files.ContainsKey(fileName))
                    {
                        // Replace
                        currentDirectory.Files[fileName] = fileData;
                    }
                    else
                    {
                        // Insert new
                        currentDirectory.AddFile(Path.GetFileName(file), fileData);
                    }
                }

                if (SelectedItemContextMenuStrip == null)
                {
                    DrawFolder(directoryTextBox.Text);
                }                    
            }
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip == null) return;

            if (SelectedItemContextMenuStrip.Tag.ToString() == "Folder")
            {
                directoryPath += SelectedItemContextMenuStrip.Text;

                string folderName = Interaction.InputBox("Enter folder name:", "Rename Folder");

                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                    if (currentDirectory.Folders.Any(x => x.Name == folderName))
                    {
                        MessageBox.Show("The folder name " + folderName + " cannot be selected because it already used by another folder.");
                    }
                    else
                    {
                        currentDirectory.Name = folderName;
                        SelectedItemContextMenuStrip.Text = folderName;
                    }
                }
            } else
            {
                string fileName = Interaction.InputBox("Enter file name:", "Rename File");

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                    if (currentDirectory.Files.ContainsKey(fileName))
                    {
                        MessageBox.Show("The file name " + fileName + " cannot be selected because it already used by another file.");
                    }
                    else
                    {
                        var oldItem = currentDirectory.Files[SelectedItemContextMenuStrip.Text];
                        currentDirectory.Files.Remove(SelectedItemContextMenuStrip.Text);
                        currentDirectory.Files.Add(fileName, oldItem);
                        SelectedItemContextMenuStrip.Text = fileName;
                    }
                }           
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip == null) return;

            if (SelectedItemContextMenuStrip.Tag.ToString() == "Folder")
            {
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);
                VirtualDirectory selectedDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath + SelectedItemContextMenuStrip.Text);
                currentDirectory.Folders.Remove(selectedDirectory);
            }
            else
            {
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);
                currentDirectory.Files.Remove(SelectedItemContextMenuStrip.Text);             
            }

            MessageBox.Show(SelectedItemContextMenuStrip.Text + " has been deleted.");
            DrawFolder(directoryTextBox.Text);
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportToolStripMenuItem_Click(sender, e);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = Path.GetFileName(openFileDialog1.FileName);
            saveFileDialog.Title = "Save Level 5 Archive file";
            saveFileDialog.Filter = "Level 5 Archive files (*.fa)|*.fa";
            saveFileDialog.InitialDirectory = openFileDialog1.InitialDirectory;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                directoryInfoTextBox.Visible = false;
                progressBar1.Visible = true;

                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;
                progressBar1.Value = 0;

                if (openFileDialog1.FileName == saveFileDialog.FileName)
                {
                    string tempPath = @"./temp";
                    string fileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);

                    if (!Directory.Exists(tempPath))
                    {
                        Directory.CreateDirectory(tempPath);
                    }

                    // Save
                    ArchiveOpened.Save(tempPath + @"\" + fileName, progressBar1);

                    // Close File
                    ArchiveOpened.Close();

                    if (File.Exists(openFileDialog1.FileName))
                    {
                        File.Delete(openFileDialog1.FileName);
                    }

                    File.Move(tempPath + @"\" + fileName, saveFileDialog.FileName);

                    // Re Open
                    ArchiveOpened = new ARC0(new FileStream(saveFileDialog.FileName, FileMode.Open));

                    directoryTextBox.Text = "/";
                }
                else
                {
                    ArchiveOpened.Save(saveFileDialog.FileName, progressBar1);
                }

                directoryInfoTextBox.Visible = true;
                progressBar1.Visible = false;
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip == null || SelectedItemContextMenuStrip.Tag.ToString() == "Folder")
            {
                if (SelectedItemContextMenuStrip != null)
                {
                    directoryPath += SelectedItemContextMenuStrip.Text;
                }

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string newFolderName = Path.GetFileName(dialog.FileName);
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                    ExportFolder(currentDirectory, dialog.FileName);

                    MessageBox.Show(directoryPath + " exported!");
                }
            }
            else
            {
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                saveFileDialog1.Filter = "All files|*.*";
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = SelectedItemContextMenuStrip.Text;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK) 
                {
                    using (FileStream fileStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        // Set the position in the base stream
                        currentDirectory.Files[SelectedItemContextMenuStrip.Text].Seek();

                        while ((bytesRead = currentDirectory.Files[SelectedItemContextMenuStrip.Text].Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }

                        MessageBox.Show(SelectedItemContextMenuStrip.Text + " exported!");
                    }
                }
            }
        }
    }
}
