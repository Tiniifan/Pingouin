using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.WindowsAPICodePack.Dialogs;
using Pingouin.Level5.Archive;
using Pingouin.Level5.Archive.ARC0;
using Pingouin.Level5.Archive.XPCK;
using Pingouin.Tools;
using System.Text;

namespace Pingouin
{
    public partial class PingouinWindow : Form
    {
        private IArchive ArchiveOpened;

        private TreeNode TreeNodeRightClick;

        private ListViewItem SelectedItemContextMenuStrip;

        private List<VirtualDirectory> FiltredDirectoris;
        private List<KeyValuePair<string, SubMemoryStream>> FiltredFiles;

        private List<(IArchive, string, string, IArchive)> OpenedArchives = new List<(IArchive, string, string, IArchive)>();

        private int OpenedArchiveIndex = 0;

        private string LastParent = "";

        public PingouinWindow()
        {
            InitializeComponent();
        }

        private long GetFullsize(VirtualDirectory currentDirectory)
        {
            long fullsize = 0;

            foreach (VirtualDirectory folder in currentDirectory.Folders)
            {
                fullsize += folder.GetSize();
            }

            foreach (KeyValuePair<string, SubMemoryStream> file in currentDirectory.Files)
            {
                fullsize += file.Value.Size;
            }

            return fullsize;
        }

        private void UpdateDirectoryInfo(VirtualDirectory currentDirectory, long fullSize)
        {
            directoryInfoTextBox.Text = "Folder(s): " + currentDirectory.Folders.Count
                + "; File(s): " + currentDirectory.Files.Count
                + "; Size: " + FormatSize(fullSize);
        }

        private void UpdateDirectoryInfo(int directoryCount, int fileCount, long fullSize)
        {
            directoryInfoTextBox.Text = "Folder(s): " + directoryCount
                + "; File(s): " + fileCount
                + "; Size: " + FormatSize(fullSize);
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

        private void CollapseAllExpandedNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded && node.Text != LastParent && (node.Parent == null || (node.Parent != null && node.Parent.Text != LastParent)))
                {
                    node.Collapse();
                }

                CollapseAllExpandedNodes(node.Nodes);
            }
        }

        private void ExpandNodeAndParents(TreeNode node)
        {
            TreeNode currentNode = node;
            while (currentNode != null)
            {
                currentNode.Expand();
                currentNode = currentNode.Parent;
            }
        }

        private TreeNode CreateTreeNode(VirtualDirectory folder)
        {
            TreeNode entryNode = new TreeNode(folder.Name, 0, 0);

            foreach (VirtualDirectory subFolder in folder.Folders)
            {
                entryNode.Nodes.Add(CreateTreeNode(subFolder));
            }

            return entryNode;
        }

        private void DrawFolder(string path)
        {
            directoryListView.Items.Clear();
            backButton.Enabled = path != "/";
            searchTextBox.ForeColor = Color.Gray;
            searchTextBox.Text = "Search on : " + path;

            VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(path);

            if (!OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item2.StartsWith("memory:/"))
            {
                archiveOpenedTabControl.TabPages[archiveOpenedTabControl.SelectedIndex].Text = Path.GetFileName(openFileDialog1.FileName) + " - " + path;
                OpenedArchives[archiveOpenedTabControl.SelectedIndex] = (ArchiveOpened, Path.GetFileName(openFileDialog1.FileName), path, OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item4);
            }

            long fullsize = 0;

            foreach (VirtualDirectory folder in currentDirectory.Folders)
            {
                ListViewItem listViewItem = new ListViewItem(folder.Name, 0);
                listViewItem.ForeColor = folder.Color;
                listViewItem.SubItems.Add(FormatSize(folder.GetSize()));
                listViewItem.Tag = "Folder";
                directoryListView.Items.Add(listViewItem);

                fullsize += folder.GetSize();
            }

            foreach (KeyValuePair<string, SubMemoryStream> file in currentDirectory.Files)
            {
                ListViewItem listViewItem = new ListViewItem(file.Key, 1);
                listViewItem.ForeColor = file.Value.Color;
                listViewItem.SubItems.Add(FormatSize(file.Value.Size));
                listViewItem.Tag = "File";
                directoryListView.Items.Add(listViewItem);

                fullsize += file.Value.Size;
            }

            TreeNode folderNode = GetTreeNodeFromPath(directoryTextBox.Text);
            if (folderNode != null)
            {
                if (folderNode.Parent != null)
                {
                    LastParent = folderNode.Parent.Text;
                } else
                {
                    LastParent = null;
                }

                CollapseAllExpandedNodes(folderNameTreeView.Nodes);
                ExpandNodeAndParents(folderNode);
            }

            UpdateDirectoryInfo(currentDirectory, fullsize);
        }

        private void DrawRootFolder()
        {
            TreeNode rootNode = new TreeNode("/");
            rootNode.Expand();

            foreach (VirtualDirectory folder in ArchiveOpened.Directory.GetFolderFromFullPath("/").Folders)
            {
                rootNode.Nodes.Add(CreateTreeNode(folder));
            }

            folderNameTreeView.Nodes.Clear();
            folderNameTreeView.Nodes.Add(rootNode);
        }

        private string GetNodeFullPath(TreeNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            StringBuilder fullPath = new StringBuilder(node.Text);

            if (node.Text == "/")
            {
                return fullPath.ToString();
            } else
            {
                while (node.Parent != null && node.Parent.Text != "/")
                {
                    node = node.Parent;
                    fullPath.Insert(0, node.Text + "/");
                }

                return $"/{fullPath.ToString()}/";
            }
        }

        private TreeNode GetTreeNodeFromPath(string path)
        {
            string[] pathSegments = path.Trim('/').Split('/');
            TreeNode currentNode = folderNameTreeView.Nodes[0];

            if (path == "/")
            {
                return currentNode;
            } else
            {
                foreach (string segment in pathSegments)
                {
                    bool found = false;
                    foreach (TreeNode child in currentNode.Nodes)
                    {
                        if (child.Text == segment)
                        {
                            currentNode = child;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        return null;
                    }
                }

                return currentNode;
            }
        }

        private void AddNewFolder(string directoryPath, bool draw)
        {
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
                    currentDirectory.Color = Color.Orange;

                    TreeNode baseNode = folderNameTreeView.Nodes[0];
                    if (directoryPath.Length > 1)
                    {
                        baseNode = GetTreeNodeFromPath(directoryPath);
                        baseNode = baseNode.Parent;
                    }
                    baseNode.Nodes.Add(new TreeNode(folderName, 0, 0));

                    if (draw)
                    {
                        ListViewItem listViewItem = new ListViewItem(folderName, 0);
                        listViewItem.ForeColor = Color.Orange;
                        listViewItem.SubItems.Add(FormatSize(0));
                        listViewItem.Tag = "Folder";
                        directoryListView.Items.Add(listViewItem);
                        UpdateDirectoryInfo(currentDirectory, GetFullsize(currentDirectory));
                    }
                }
            }
        }

        private void ImportFolder(string directoryPath, bool draw = false)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string newFolderName = Path.GetFileName(dialog.FileName);
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                ImportFolder(currentDirectory, dialog.FileName);

                if (draw)
                {
                    ListViewItem existingItem = directoryListView.FindItemWithText(newFolderName);

                    if (existingItem != null)
                    {
                        directoryListView.Items.Remove(existingItem);
                    }

                    ListViewItem listViewItem = new ListViewItem(newFolderName, 0);
                    listViewItem.ForeColor = Color.Orange;
                    listViewItem.SubItems.Add(FormatSize(currentDirectory.GetFolder(newFolderName).GetSize()));
                    listViewItem.Tag = "Folder";
                    directoryListView.Items.Add(listViewItem);
                }

                DrawFolder(directoryPath);
            }
        }

        private void ImportFiles(string directoryPath, bool draw)
        {
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
                    fileData.Color = Color.Orange;

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

                if (draw)
                {
                    DrawFolder(directoryTextBox.Text);
                }

                DrawRootFolder();
            }
        }

        private void ImportFolder(VirtualDirectory currentDirectory, string selectedDirectory)
        {
            string directoryName = Path.GetFileName(selectedDirectory);

            if (currentDirectory.Folders.Any(x => x.Name == directoryName))
            {
                currentDirectory = currentDirectory.GetFolder(directoryName);
                currentDirectory.Color = Color.Orange;
            }
            else
            {
                if (ArchiveOpened.Name == "ARC0" || ArchiveOpened.Name == "XFSA")
                {
                    currentDirectory.AddFolder(directoryName);
                    currentDirectory = currentDirectory.GetFolder(directoryName);
                    currentDirectory.Color = Color.Orange;
                }
            }

            string[] files = Directory.GetFiles(selectedDirectory);
            foreach (string file in files)
            {
                SubMemoryStream fileData;
                string fileName = Path.GetFileName(file);

                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long fileOffset = 0;
                long fileSize = fileStream.Length;
                fileData = new SubMemoryStream(fileStream, fileOffset, fileSize);
                fileData.Color = Color.Orange;

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

            string[] subDirectories = Directory.GetDirectories(selectedDirectory);
            foreach (string subDir in subDirectories)
            {
                ImportFolder(currentDirectory, subDir);
            }
        }

        private void OpenInNewView(string selectedFolder)
        {
            OpenedArchives.Add((ArchiveOpened, Path.GetFileName(openFileDialog1.FileName), selectedFolder, null));

            var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
            archiveOpenedTabControl.TabPages.Insert(lastIndex, Path.GetFileName(openFileDialog1.FileName) + " - " + selectedFolder);
            archiveOpenedTabControl.SelectedIndex = lastIndex;
        }

        private void DeleteFolder(string selectedFolder = null, bool isDirectory = false)
        {
            string directoryPath = directoryTextBox.Text;
            VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

            if (isDirectory)
            {
                VirtualDirectory selectedDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath + selectedFolder);
                currentDirectory.Folders.Remove(selectedDirectory);
                DrawRootFolder();

                if (OpenedArchives.Any(x => x.Item3 == $"/{selectedFolder}")) 
                {
                    var matches = OpenedArchives.FindAll(x => x.Item3 == $"/{selectedFolder}");
                    for (int i = 0; i < matches.Count(); i++)
                    {
                        int index = OpenedArchives.IndexOf(matches[i]);
                        archiveOpenedTabControl.TabPages.RemoveAt(index);
                        OpenedArchives.RemoveAt(index);
                    }
                    
                }
            } else
            {
                currentDirectory.Files.Remove(selectedFolder);
            }

            MessageBox.Show(selectedFolder + " has been deleted.");
            DrawFolder(directoryTextBox.Text);
        }

        private void RenameItem(string directoryPath, bool isDirectory)
        {
            if (isDirectory)
            {
                string folderName = Interaction.InputBox("Enter folder name:", "Rename Folder", Path.GetFileName(directoryPath));

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
                        DrawRootFolder();
                        DrawFolder(directoryTextBox.Text);                      
                    }
                }
            }
            else
            {
                string fileName = Interaction.InputBox("Enter file name:", "Rename File", Path.GetFileName(directoryPath));

                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    string oldFileName = Path.GetFileName(directoryPath);
                    directoryPath = Path.GetDirectoryName(directoryPath).Replace("\\", "/");
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                    if (currentDirectory.Files.ContainsKey(fileName))
                    {
                        MessageBox.Show("The file name " + fileName + " cannot be selected because it already used by another file.");
                    }
                    else
                    {
                        var oldItem = currentDirectory.Files[oldFileName];
                        currentDirectory.Files.Remove(oldFileName);
                        currentDirectory.Files.Add(fileName, oldItem);
                        DrawFolder(directoryTextBox.Text);
                    }
                }
            }
        }

        private void ReplaceItem(string directoryPath, bool isDirectory)
        {
            if (isDirectory)
            {
                if (ArchiveOpened.Name == "ARC0" || ArchiveOpened.Name == "XFSA")
                {
                    ImportFolder(directoryPath, isDirectory);
                }
                else
                {
                    ImportFiles(directoryPath, true);
                }
            } else
            {
                // Import one file
                openFileDialog2.Filter = "All files|*.*";
                openFileDialog2.Multiselect = false;
                openFileDialog2.RestoreDirectory = true;

                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    // Get Gile data
                    SubMemoryStream fileData;

                    FileStream fileStream = new FileStream(openFileDialog2.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    long fileOffset = 0;
                    long fileSize = fileStream.Length;
                    fileData = new SubMemoryStream(fileStream, fileOffset, fileSize);
                    fileData.Color = Color.Orange;

                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryTextBox.Text);

                    currentDirectory.Files[SelectedItemContextMenuStrip.Text] = fileData;

                    DrawFolder(directoryTextBox.Text);
                }
            }
        }

        private void ExportItem(string directoryPath, bool isDirectory)
        {
            if (isDirectory)
            {
                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.IsFolderPicker = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                    if (currentDirectory.Name == null)
                    {
                        directoryPath = Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + Path.GetExtension(openFileDialog1.FileName).Replace(".", "_");
                    }

                    ExportFolder(currentDirectory, dialog.FileName);

                    MessageBox.Show(directoryPath + " exported!");
                }
            }
            else
            {
                string fileName = Path.GetFileName(directoryPath);
                directoryPath = Path.GetDirectoryName(directoryPath).Replace("\\", "/");
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);

                saveFileDialog1.Filter = "All files|*.*";
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.FileName = fileName;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (FileStream fileStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;

                        // Set the position in the base stream
                        currentDirectory.Files[fileName].Seek();

                        while ((bytesRead = currentDirectory.Files[fileName].Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                        }

                        MessageBox.Show(fileName + " exported!");
                    }
                }
            }
        }

        private void ImportFile(VirtualDirectory currentDirectory, string selectedFile)
        {
            SubMemoryStream fileData;
            string fileName = Path.GetFileName(selectedFile);

            FileStream fileStream = new FileStream(selectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long fileOffset = 0;
            long fileSize = fileStream.Length;
            fileData = new SubMemoryStream(fileStream, fileOffset, fileSize);
            fileData.Color = Color.Orange;

            if (currentDirectory.Files.ContainsKey(fileName))
            {
                // Replace
                currentDirectory.Files[fileName] = fileData;
            }
            else
            {
                // Insert new
                currentDirectory.AddFile(Path.GetFileName(selectedFile), fileData);
            }
        }

        private void ExportFolder(VirtualDirectory currentDirectory, string exportPath)
        {
            string directoryName = currentDirectory.Name;

            if (directoryName == null)
            {
                directoryName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName) + Path.GetExtension(openFileDialog1.FileName).Replace(".", "_");
            }

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

        private void OpenArchiveL5(string filename)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            ArchiveOpened = Archiver.GetArchive(fileStream);
            if (ArchiveOpened == null)
            {
                MessageBox.Show("Unsupported file type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            OpenedArchives.Add((ArchiveOpened, Path.GetFileName(openFileDialog1.FileName), "/", null));

            if (ArchiveOpened != null && OpenedArchives.Count != 1)
            {
                var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
                archiveOpenedTabControl.TabPages.Insert(lastIndex, Path.GetFileName(openFileDialog1.FileName) + " - " + "/");
                archiveOpenedTabControl.SelectedIndex = archiveOpenedTabControl.TabPages.Count - 2;
            }

            DrawRootFolder();
            directoryTextBox.Text = "/";

            importToolStripMenuItem2.Enabled = true;
            saveToolStripMenuItem.Enabled = true;
            backButton.Enabled = true;
            directoryTextBox.Enabled = true;
            directoryListView.Enabled = true;
            directoryInfoTextBox.Enabled = true;
            progressBar1.Visible = false;
            searchTextBox.Enabled = true;
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Supported Files (*.fa, *.xc, *.xb, *.pck)|*.fa;*.xc;*.xb;*.pck";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.FileName = null;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                directoryListView.Items.Clear();
                OpenArchiveL5(openFileDialog1.FileName);
            }
        }

        private void DirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ArchiveOpened != null)
            {
                DrawFolder(directoryTextBox.Text);
            }
        }

        private void DirectoryListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Assurez-vous que le double-clic a lieu sur un élément
            if (directoryListView.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = directoryListView.SelectedItems[0];

                if (selectedItem.Tag is ValueTuple<string, string> tuple)
                {
                    string selectedItemType = tuple.Item1;
                    string fullPath = tuple.Item2;

                    if (selectedItemType == "Folder" && FiltredDirectoris != null && FiltredDirectoris.Count > 0)
                    {
                        if (resultTextBox.Visible)
                        {
                            resultTextBox.Visible = false;
                        }

                        directoryTextBox.Text = fullPath;
                    }
                    else if (selectedItemType == "File" && FiltredFiles != null && FiltredFiles.Count > 0)
                    {
                        IArchive newArchive;
                        string directoryPath = "";
                        string fileName = Path.GetFileName(fullPath);

                        int lastSlashIndex = fullPath.LastIndexOf('/') + 1;

                        if (lastSlashIndex != -1)
                        {
                            directoryPath = fullPath.Substring(0, lastSlashIndex);
                        }
                        else
                        {
                            directoryPath = "/";
                        }

                        string fileExtension = Path.GetExtension(selectedItem.Text);

                        VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);
                        byte[] fileContent = currentDirectory.GetFileFromFullPath(fileName);

                        if (fileExtension.ToLower() == ".xc" || fileExtension.ToLower() == ".xb" || fileExtension.ToLower() == ".pck")
                        {
                            newArchive = Archiver.GetArchive(fileContent);
                        }
                        else
                        {
                            return;
                        }

                        if (newArchive == null)
                        {
                            return;
                        }

                        var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
                        OpenedArchives.Add((newArchive, "memory:" + fullPath, "/", ArchiveOpened));
                        archiveOpenedTabControl.TabPages.Insert(lastIndex, Path.GetFileName(openFileDialog1.FileName) + " - memory:" + fullPath);
                        archiveOpenedTabControl.SelectedIndex = archiveOpenedTabControl.TabPages.Count - 2;
                    }
                }
                else
                {
                    string selectedItemType = selectedItem.Tag.ToString();

                    if (selectedItemType == "Folder")
                    {
                        if (resultTextBox.Visible)
                        {
                            resultTextBox.Visible = false;
                        }

                        directoryTextBox.Text += selectedItem.Text + "/";
                    }
                    else if (selectedItemType == "File")
                    {
                        IArchive newArchive;

                        string fileExtension = Path.GetExtension(selectedItem.Text);

                        VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryTextBox.Text);
                        byte[] fileContent = currentDirectory.GetFileFromFullPath(selectedItem.Text);

                        if (fileExtension.ToLower() == ".xc" || fileExtension.ToLower() == ".xb" || fileExtension.ToLower() == ".pck")
                        {
                            newArchive = Archiver.GetArchive(fileContent);
                        }
                        else
                        {
                            return;
                        }

                        if (newArchive == null)
                        {
                            return;
                        }

                        var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
                        OpenedArchives.Add((newArchive, "memory:" + directoryTextBox.Text + selectedItem.Text, "/", ArchiveOpened));
                        archiveOpenedTabControl.TabPages.Insert(lastIndex, Path.GetFileName(openFileDialog1.FileName) + " - memory:" + directoryTextBox.Text + selectedItem.Text);
                        archiveOpenedTabControl.SelectedIndex = archiveOpenedTabControl.TabPages.Count - 2;
                    }
                }
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            string currentPathName = directoryTextBox.Text;

            int lastSlashIndex = currentPathName.LastIndexOf('/');
            if (lastSlashIndex > 0)
            {
                int secondToLastSlashIndex = currentPathName.LastIndexOf('/', lastSlashIndex - 1);
                if (secondToLastSlashIndex >= 0)
                {
                    directoryTextBox.Text = currentPathName.Substring(0, secondToLastSlashIndex + 1);
                }
            } else
            {
                directoryTextBox.Text = "/";
            }
        }

        private void DirectoryContextMenuStrip_Opening(object sender, CancelEventArgs e)
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

                changePrefixForFilesToolStripMenuItem.Enabled = selectedItemType == "Folder";
            }
            else
            {
                SelectedItemContextMenuStrip = null;

                folderToolStripMenuItem.Enabled = ArchiveOpened.Name == "ARC0" || ArchiveOpened.Name == "XFSA";
                filesToolStripMenuItem.Enabled = true;

                renameToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Enabled = true;

                changePrefixForFilesToolStripMenuItem.Enabled = true;
            }
        }

        private void DirectoryListView_MouseDown(object sender, MouseEventArgs e)
        {
            //IsLeftClick = e.Button == MouseButtons.Left;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            AddNewFolder(directoryPath, SelectedItemContextMenuStrip == null);
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            ImportFolder(directoryPath, SelectedItemContextMenuStrip != null);
        }

        private void ImportFilesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;
            bool draw = true;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
                draw = false;
            }

            ImportFiles(directoryPath, draw);
        }

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;
            bool isDirectory = true;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
                isDirectory = SelectedItemContextMenuStrip.Tag.ToString() == "Folder";
            }

            RenameItem(directoryPath, isDirectory);
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItemContextMenuStrip == null) return;

            DeleteFolder(SelectedItemContextMenuStrip.Text, SelectedItemContextMenuStrip.Tag.ToString() == "Folder");
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;
            bool isDirectory = true;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
                isDirectory = SelectedItemContextMenuStrip.Tag.ToString() == "Folder";
            }

            ReplaceItem(directoryPath, isDirectory);
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Déclaration du chronomètre
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            if (OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item2.StartsWith("memory:/"))
            {
                IArchive memoryArchive = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item1;
                IArchive linkArchive = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item4;

                if (linkArchive == null)
                {
                    MessageBox.Show("Error during save, the file is not associated with any open archive.");
                    return;
                }

                string fullDirectoryPath = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item2.Replace("memory:/", "/");
                string directoryPath = "";
                string fileName = Path.GetFileName(fullDirectoryPath);

                int lastSlashIndex = fullDirectoryPath.LastIndexOf('/') + 1;

                if (lastSlashIndex != -1)
                {
                    directoryPath = fullDirectoryPath.Substring(0, lastSlashIndex);
                }
                else
                {
                    directoryPath = "/";
                }

                VirtualDirectory currentDirectory = linkArchive.Directory.GetFolderFromFullPath(directoryPath);

                // Démarrer le chronomètre avant la sauvegarde
                stopwatch.Start();
                currentDirectory.Files[fileName].ByteContent = memoryArchive.Save();
                stopwatch.Stop();

                // Afficher le temps de sauvegarde
                MessageBox.Show($"{fileName} was saved!\nSave time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = Path.GetFileName(openFileDialog1.FileName);
                saveFileDialog.Title = "Save Level 5 Archive file";

                if (ArchiveOpened.Name == "ARC0" || ArchiveOpened.Name == "XFSA")
                {
                    saveFileDialog.Filter = "XFSA (*.fa)|*.fa";
                }
                else
                {
                    saveFileDialog.Filter = "XB Files (*.xb)|*.xb|XC Files (*.xc)|*.xc|PCK Files (*.pck)|*.pck";
                }

                saveFileDialog.InitialDirectory = openFileDialog1.InitialDirectory;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    backButton.Visible = false;
                    directoryTextBox.Visible = false;
                    searchTextBox.Visible = false;
                    tableLayoutPanel2.Visible = false;
                    progressBar1.Visible = true;

                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = 0;

                    Application.DoEvents();

                    if (openFileDialog1.FileName == saveFileDialog.FileName)
                    {
                        string tempPath = @"./temp";
                        string fileName = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);

                        if (!Directory.Exists(tempPath))
                        {
                            Directory.CreateDirectory(tempPath);
                        }

                        // Démarrer le chronomètre avant la sauvegarde
                        stopwatch.Start();
                        // Save
                        ArchiveOpened.Save(tempPath + @"\" + fileName, progressBar1);
                        stopwatch.Stop();

                        // Close File
                        Type archiveType = ArchiveOpened.GetType();
                        ArchiveOpened = ArchiveOpened.Close();

                        if (File.Exists(openFileDialog1.FileName))
                        {
                            File.Delete(openFileDialog1.FileName);
                        }

                        File.Move(tempPath + @"\" + fileName, saveFileDialog.FileName);

                        if (archiveType == typeof(ARC0))
                        {
                            ArchiveOpened = new ARC0(new FileStream(saveFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }
                        else if (archiveType == typeof(XPCK))
                        {
                            // Re Open
                            ArchiveOpened = new XPCK(new FileStream(saveFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        }
                    }
                    else
                    {
                        // Démarrer le chronomètre avant la sauvegarde
                        stopwatch.Start();
                        ArchiveOpened.Save(saveFileDialog.FileName, progressBar1);
                        stopwatch.Stop();
                    }

                    // Afficher le temps de sauvegarde
                    MessageBox.Show($"Saved!\nSave time: {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.TotalSeconds:F2} seconds)");

                    progressBar1.Visible = false;
                    backButton.Visible = true;
                    directoryTextBox.Visible = true;
                    searchTextBox.Visible = true;
                    tableLayoutPanel2.Visible = true;

                    DirectoryTextBox_TextChanged(sender, e);
                }
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;
            bool isDirectory = true;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
                isDirectory = SelectedItemContextMenuStrip.Tag.ToString() == "Folder";
            }

            ExportItem(directoryPath, isDirectory);
        }

        private void XFSAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Interaction.InputBox("Enter file name:", "New XFSA");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                ArchiveOpened = new ARC0();

                OpenedArchives.Add((ArchiveOpened, fileName, "/", null));

                if (ArchiveOpened != null && OpenedArchives.Count != 1)
                {
                    var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
                    archiveOpenedTabControl.TabPages.Insert(lastIndex, fileName + " - " + "/");
                    archiveOpenedTabControl.SelectedIndex = archiveOpenedTabControl.TabPages.Count - 2;
                }

                openFileDialog1.FileName = fileName;
                DrawRootFolder();
                directoryTextBox.Text = "/";

                importToolStripMenuItem2.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                backButton.Enabled = true;
                directoryTextBox.Enabled = true;
                directoryListView.Enabled = true;
                directoryInfoTextBox.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private void XPCKToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fileName = Interaction.InputBox("Enter file name:", "New XPCK");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                ArchiveOpened = new XPCK();

                OpenedArchives.Add((ArchiveOpened, fileName, "/", null));

                if (ArchiveOpened != null && OpenedArchives.Count != 1)
                {
                    var lastIndex = archiveOpenedTabControl.TabPages.Count - 1;
                    archiveOpenedTabControl.TabPages.Insert(lastIndex, fileName + " - " + "/");
                    archiveOpenedTabControl.SelectedIndex = archiveOpenedTabControl.TabPages.Count - 2;
                }

                openFileDialog1.FileName = fileName;
                DrawRootFolder();
                directoryTextBox.Text = "/";

                importToolStripMenuItem2.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                backButton.Enabled = true;
                directoryTextBox.Enabled = true;
                directoryListView.Enabled = true;
                directoryInfoTextBox.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private void ImportFoldersFromVirtualDirectory(VirtualDirectory currentDirectory, VirtualDirectory importDirectory)
        {
            ImportFilesFromVirtualDirectory(currentDirectory, importDirectory);

            foreach (VirtualDirectory directory in importDirectory.Folders)
            {
                VirtualDirectory subDirectory;

                if (currentDirectory.Folders.Any(x => x.Name == directory.Name))
                {
                    subDirectory = currentDirectory.GetFolder(directory.Name);
                    subDirectory.Color = Color.Orange;
                }
                else if (ArchiveOpened.Name == "ARC0" || ArchiveOpened.Name == "XFSA")
                {
                    currentDirectory.AddFolder(directory.Name);
                    subDirectory = currentDirectory.GetFolder(directory.Name);
                    subDirectory.Color = Color.Orange;
                } else
                {
                    continue;
                }

                ImportFoldersFromVirtualDirectory(subDirectory, directory);
            }
        }

        private void ImportFilesFromVirtualDirectory(VirtualDirectory currentDirectory, VirtualDirectory importDirectory)
        {
            foreach (KeyValuePair<string, SubMemoryStream> file in importDirectory.Files)
            {
                Console.WriteLine(file.Key);
                SubMemoryStream fileData = file.Value;
                fileData.Read();

                SubMemoryStream newFileData = new SubMemoryStream(fileData.ByteContent);
                newFileData.Color = Color.Orange;

                if (currentDirectory.Files.ContainsKey(file.Key))
                {
                    // Replace
                    currentDirectory.Files[file.Key] = newFileData;
                }
                else
                {
                    // Insert new
                    currentDirectory.AddFile(Path.GetFileName(file.Key), newFileData);
                }
            }
        }

        private void ImportToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            openFileDialog3.Filter = "Supported Files (*.fa, *.xc, *.xb, *.pck, *.zip)|*.fa;*.xc;*.xb;*.pck;*.zip";
            openFileDialog3.RestoreDirectory = true;

            if (openFileDialog3.ShowDialog() == DialogResult.OK)
            {
                IArchive importArchive = Archiver.GetArchive(new FileStream(openFileDialog3.FileName, FileMode.Open, FileAccess.Read));
                if (importArchive == null)
                {
                    MessageBox.Show("Unsupported file type.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryTextBox.Text);
                ImportFoldersFromVirtualDirectory(currentDirectory, importArchive.Directory);
                importArchive.Close();

                DrawFolder(directoryTextBox.Text);
            }
        }

        private void ArchiveL5Window_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get the list of dropped file and folder paths
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (ArchiveOpened == null)
                {
                    if (droppedFiles.Length > 0)
                    {
                        openFileDialog1.FileName = droppedFiles[0];
                        OpenArchiveL5(openFileDialog1.FileName);
                    }
                } else
                {
                    VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryTextBox.Text);

                    foreach (string path in droppedFiles)
                    {
                        // Check if the path corresponds to a file or a folder
                        if (File.Exists(path))
                        {
                            ImportFile(currentDirectory, path);
                        }
                        else if (Directory.Exists(path))
                        {
                            ImportFolder(currentDirectory, path);
                        }
                    }

                    DrawFolder(directoryTextBox.Text);
                    DrawRootFolder();
                }
            }
        }

        private void ArchiveL5Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OpenInANewViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            OpenInNewView(directoryPath);
        }

        private void ArchiveOpenedTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!archiveOpenedTabControl.Focused) return;

            if (archiveOpenedTabControl.TabPages[archiveOpenedTabControl.SelectedIndex] == deleteTabPage) return;

            Control[] ctrlArray = new Control[archiveOpenedTabControl.TabPages[OpenedArchiveIndex].Controls.Count];
            archiveOpenedTabControl.TabPages[OpenedArchiveIndex].Controls.CopyTo(ctrlArray, 0);
            archiveOpenedTabControl.TabPages[archiveOpenedTabControl.SelectedIndex].Controls.AddRange(ctrlArray);

            OpenedArchiveIndex = archiveOpenedTabControl.SelectedIndex;
            ArchiveOpened = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item1;
            openFileDialog1.FileName = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item2;

            if (directoryTextBox.Text == OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item3)
            {
                DirectoryTextBox_TextChanged(sender, e);
            } else
            {
                directoryTextBox.Text = OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item3;
            }

            DrawRootFolder();
        }

        private void ArchiveOpenedTabControl_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e.TabPage == deleteTabPage)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    HandleTabClosing();
                });
                e.Cancel = true;
            }
        }

        private void HandleTabClosing()
        {
            if (OpenedArchives.Count > 0)
            {
                int tabWithSameFile = OpenedArchives.Count(x => x.Item1 == ArchiveOpened);

                string message = "Closing this tab will result in the closure of " + OpenedArchives[archiveOpenedTabControl.SelectedIndex].Item2 + ".\nDo you want to continue?";

                if (tabWithSameFile - 1 == 0 || OpenedArchives.Count - 1 == 0)
                {
                    DialogResult dialogResult = MessageBox.Show(message, "Close file", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                    {
                        TabPage tabPageToDelete = archiveOpenedTabControl.TabPages[archiveOpenedTabControl.SelectedIndex];

                        // Remove linked archive
                        var archivesToRemove = OpenedArchives.Where(x => x.Item4 == ArchiveOpened).ToArray();
                        for (int i = 0; i < archivesToRemove.Length; i++)
                        {
                            var linkedArchive = archivesToRemove[i];
                            int index = OpenedArchives.IndexOf(linkedArchive);
                            archiveOpenedTabControl.TabPages.RemoveAt(index);
                            OpenedArchives.RemoveAt(index);
                        }

                        ArchiveOpened = ArchiveOpened.Close();

                        int tabPageToDeleteIndex = archiveOpenedTabControl.TabPages.IndexOf(tabPageToDelete);

                        if (OpenedArchives.Count - 1 == 0)
                        {
                            OpenedArchiveIndex = 0;

                            homeTabPage.Text = "Home";
                            saveToolStripMenuItem.Enabled = false;
                            backButton.Enabled = false;
                            directoryTextBox.Enabled = false;
                            directoryListView.Enabled = false;
                            directoryInfoTextBox.Enabled = false;
                            progressBar1.Visible = false;
                            directoryTextBox.Text = "";
                            folderNameTreeView.Nodes.Clear();
                            directoryTextBox.Clear();
                            directoryListView.Items.Clear();
                            directoryInfoTextBox.Text = "";
                        } else
                        {
                            int nextTab = tabPageToDeleteIndex == 0 ? 1 : tabPageToDeleteIndex - 1;
                            archiveOpenedTabControl.SelectedIndex = nextTab;
                            archiveOpenedTabControl.TabPages.RemoveAt(tabPageToDeleteIndex);
                        }

                        OpenedArchives.RemoveAt(tabPageToDeleteIndex);
                    }
                } else
                {
                    int removeIndex = archiveOpenedTabControl.SelectedIndex;
                    int nextTab = archiveOpenedTabControl.SelectedIndex == 0 ? 1 : archiveOpenedTabControl.SelectedIndex - 1;
                    archiveOpenedTabControl.SelectedIndex = nextTab;
                    archiveOpenedTabControl.TabPages.RemoveAt(removeIndex);
                    OpenedArchives.RemoveAt(archiveOpenedTabControl.SelectedIndex);
                }
            }
        }

        private void SearchTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (searchTextBox.ForeColor != Color.Black)
            {
                searchTextBox.ForeColor = Color.Black;
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!searchTextBox.Focused || searchTextBox.Enabled == false || searchTextBox.Text == $"Search on : {directoryTextBox.Text}") return;

            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                FiltredDirectoris = null;
                FiltredFiles = null;
                searchTextBox.ForeColor = Color.Gray;
                directoryListView.Items.Clear();
                DirectoryTextBox_TextChanged(sender, e);
                resultTextBox.Visible = false;
            }
            else
            {
                directoryListView.Items.Clear();
                resultTextBox.Visible = true;
                resultTextBox.Text = "Search results in " + directoryTextBox.Text;

                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryTextBox.Text);
                FiltredDirectoris = currentDirectory.SearchDirectories(searchTextBox.Text);
                FiltredFiles = currentDirectory.SearchFiles(searchTextBox.Text);
                long fullsize = 0;

                foreach (VirtualDirectory folder in FiltredDirectoris)
                {
                    ListViewItem listViewItem = new ListViewItem(folder.Name, 0);
                    listViewItem.ForeColor = folder.Color;
                    listViewItem.SubItems.Add(FormatSize(folder.GetSize()));
                    listViewItem.Tag = ("Folder", "/" + folder.GetFullPath(ArchiveOpened.Directory));
                    directoryListView.Items.Add(listViewItem);

                    fullsize += folder.GetSize();
                }

                foreach (KeyValuePair<string, SubMemoryStream> file in FiltredFiles)
                {
                    if (file.Key != null)
                    {
                        ListViewItem listViewItem = new ListViewItem(Path.GetFileName(file.Key), 1);
                        listViewItem.ForeColor = file.Value.Color;
                        listViewItem.SubItems.Add(FormatSize(file.Value.Size));
                        listViewItem.Tag = ("File", file.Key);
                        directoryListView.Items.Add(listViewItem);

                        fullsize += file.Value.Size;
                    }
                }

                UpdateDirectoryInfo(FiltredDirectoris.Count, FiltredFiles.Count, fullsize);
            }
        }

        private void SearchTextBox_MouseEnter(object sender, EventArgs e)
        {
            if (searchTextBox.Text != $"Search on : {directoryTextBox.Text}") return;

            this.Focus();
            searchTextBox.Enabled = false;
            searchTextBox.Text = "";
            searchTextBox.Enabled = true;
            searchTextBox.Focus();
        }

        private void SearchTextBox_MouseLeave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(searchTextBox.Text))
            {
                searchTextBox.Enabled = false;
                FiltredDirectoris = null;
                FiltredFiles = null;
                searchTextBox.ForeColor = Color.Gray;
                //directoryListView.Items.Clear();
                //DirectoryTextBox_TextChanged(sender, e);
                resultTextBox.Visible = false;
                searchTextBox.Text = $"Search on : {directoryTextBox.Text}";
                searchTextBox.Enabled = true;
                this.Focus();
            }
        }

        private void FolderNameTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            directoryTextBox.Text = GetNodeFullPath(e.Node);
        }

        private void DirectoryListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            List<string> filePaths = new List<string>() { };
            string tempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");

            foreach (ListViewItem item in directoryListView.SelectedItems)
            {
                if (item.Tag.ToString() == "Folder")
                {
                    ExportFolder(ArchiveOpened.Directory.GetFolderFromFullPath($"{directoryTextBox.Text}{item.Text}"), "./temp");
                    filePaths.Add($"{tempFolderPath}\\{directoryTextBox.Text}{item.Text}");
                } else
                {
                    string filePath = Path.Combine("./temp", item.Text);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllBytes(filePath, ArchiveOpened.Directory.GetFileFromFullPath($"{directoryTextBox.Text}{item.Text}"));
                    filePaths.Add($"{tempFolderPath}\\{item.Text}");
                }
            }

            directoryListView.DoDragDrop(new DataObject(DataFormats.FileDrop, filePaths.ToArray()), DragDropEffects.Move);
        }

        private void DirectoryListView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void FolderNameTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNodeRightClick = e.Node;
                treeViewContextMenuStrip.Show(folderNameTreeView, e.Location);
            }
        }

        private void NewToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AddNewFolder(GetNodeFullPath(TreeNodeRightClick), false);
            TreeNodeRightClick = null;
        }

        private void ImportToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ImportFolder(GetNodeFullPath(TreeNodeRightClick), false);
            TreeNodeRightClick = null;
        }

        private void OpenInANewViewToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenInNewView(GetNodeFullPath(TreeNodeRightClick));
            TreeNodeRightClick = null;
        }

        private void DeleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteFolder(GetNodeFullPath(TreeNodeRightClick), true);
            TreeNodeRightClick = null;
        }

        private void RenameToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RenameItem(GetNodeFullPath(TreeNodeRightClick), true);
            TreeNodeRightClick = null;
        }

        private void ReplaceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceItem(GetNodeFullPath(TreeNodeRightClick), true);
            TreeNodeRightClick = null;
        }

        private void ExportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ExportItem(GetNodeFullPath(TreeNodeRightClick), true);
            TreeNodeRightClick = null;
        }

        private void DirectoryListView_DragDrop(object sender, DragEventArgs e)
        {
            ArchiveL5Window_DragDrop(sender, e);
        }

        private void ChangePrefixForFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directoryPath = directoryTextBox.Text;

            if (SelectedItemContextMenuStrip != null)
            {
                if (SelectedItemContextMenuStrip.Tag.ToString() != "Folder")
                {
                    MessageBox.Show("You cannot select a file for this option.");
                    return;
                }

                directoryPath += SelectedItemContextMenuStrip.Text;
            }

            string newPrefix = Interaction.InputBox("Enter new prefix name:", "Change Prefix for files", Path.GetFileName(directoryPath));

            if (!string.IsNullOrWhiteSpace(newPrefix))
            {
                VirtualDirectory currentDirectory = ArchiveOpened.Directory.GetFolderFromFullPath(directoryPath);
                string[] filenames = currentDirectory.Files.Keys.ToArray();

                for (int i = 0; i < filenames.Count(); i++)
                {
                    string filename = filenames[i];

                    // Check if separator exists
                    if (filename.Contains("_"))
                    {
                        var oldItem = currentDirectory.Files[filename];
                        currentDirectory.Files.Remove(filename);

                        // Get everything after the first separator "_"
                        int separatorIndex = filename.IndexOf("_");
                        string suffixAfterSeparator = filename.Substring(separatorIndex);
                        string newFilename = newPrefix + suffixAfterSeparator;

                        currentDirectory.Files.Add(newFilename, oldItem);
                    }
                }
            }

            DrawFolder(directoryTextBox.Text);
        }
    }
}
