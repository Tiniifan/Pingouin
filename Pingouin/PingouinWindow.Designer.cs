namespace Pingouin
{
    partial class PingouinWindow
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PingouinWindow));
            this.directoryListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.directoryContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.folderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInANewViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.xFSAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xPCKToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.backButton = new System.Windows.Forms.Button();
            this.directoryInfoTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog2 = new System.Windows.Forms.OpenFileDialog();
            this.directoryTextBox = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog3 = new System.Windows.Forms.OpenFileDialog();
            this.archiveOpenedTabControl = new System.Windows.Forms.TabControl();
            this.homeTabPage = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.folderNameTreeView = new System.Windows.Forms.TreeView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.resultTextBox = new System.Windows.Forms.TextBox();
            this.deleteTabPage = new System.Windows.Forms.TabPage();
            this.directoryContextMenuStrip.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.archiveOpenedTabControl.SuspendLayout();
            this.homeTabPage.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // directoryListView
            // 
            this.directoryListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directoryListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.directoryListView.ContextMenuStrip = this.directoryContextMenuStrip;
            this.directoryListView.Enabled = false;
            this.directoryListView.HideSelection = false;
            this.directoryListView.Location = new System.Drawing.Point(387, 3);
            this.directoryListView.MultiSelect = false;
            this.directoryListView.Name = "directoryListView";
            this.directoryListView.Size = new System.Drawing.Size(379, 351);
            this.directoryListView.SmallImageList = this.imageList1;
            this.directoryListView.TabIndex = 0;
            this.directoryListView.UseCompatibleStateImageBehavior = false;
            this.directoryListView.View = System.Windows.Forms.View.Details;
            this.directoryListView.SelectedIndexChanged += new System.EventHandler(this.DirectoryListView_SelectedIndexChanged);
            this.directoryListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DirectoryListView_MouseDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Tag = "";
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Size";
            this.columnHeader2.Width = 100;
            // 
            // directoryContextMenuStrip
            // 
            this.directoryContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.folderToolStripMenuItem,
            this.filesToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.exportToolStripMenuItem});
            this.directoryContextMenuStrip.Name = "directoryContextMenuStrip";
            this.directoryContextMenuStrip.Size = new System.Drawing.Size(118, 136);
            this.directoryContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.DirectoryContextMenuStrip_Opening);
            // 
            // folderToolStripMenuItem
            // 
            this.folderToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.importToolStripMenuItem,
            this.openInANewViewToolStripMenuItem});
            this.folderToolStripMenuItem.Name = "folderToolStripMenuItem";
            this.folderToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.folderToolStripMenuItem.Text = "Folder";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.NewToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.importToolStripMenuItem.Text = "Import";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // openInANewViewToolStripMenuItem
            // 
            this.openInANewViewToolStripMenuItem.Name = "openInANewViewToolStripMenuItem";
            this.openInANewViewToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.openInANewViewToolStripMenuItem.Text = "Open in a new view";
            this.openInANewViewToolStripMenuItem.Click += new System.EventHandler(this.OpenInANewViewToolStripMenuItem_Click);
            // 
            // filesToolStripMenuItem
            // 
            this.filesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem1});
            this.filesToolStripMenuItem.Name = "filesToolStripMenuItem";
            this.filesToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.filesToolStripMenuItem.Text = "Files";
            // 
            // importToolStripMenuItem1
            // 
            this.importToolStripMenuItem1.Name = "importToolStripMenuItem1";
            this.importToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.importToolStripMenuItem1.Text = "Import";
            this.importToolStripMenuItem1.Click += new System.EventHandler(this.ImportFilesToolStripMenuItem1_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.renameToolStripMenuItem.Text = "Rename";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.RenameToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.ReplaceToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.exportToolStripMenuItem.Text = "Export";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "IconFolder.png");
            this.imageList1.Images.SetKeyName(1, "IconFile.png");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(808, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.newToolStripMenuItem1,
            this.importToolStripMenuItem2,
            this.saveToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // newToolStripMenuItem1
            // 
            this.newToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.xFSAToolStripMenuItem,
            this.xPCKToolStripMenuItem});
            this.newToolStripMenuItem1.Name = "newToolStripMenuItem1";
            this.newToolStripMenuItem1.Size = new System.Drawing.Size(110, 22);
            this.newToolStripMenuItem1.Text = "New";
            // 
            // xFSAToolStripMenuItem
            // 
            this.xFSAToolStripMenuItem.Name = "xFSAToolStripMenuItem";
            this.xFSAToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.xFSAToolStripMenuItem.Text = "XFSA";
            this.xFSAToolStripMenuItem.Click += new System.EventHandler(this.XFSAToolStripMenuItem_Click);
            // 
            // xPCKToolStripMenuItem
            // 
            this.xPCKToolStripMenuItem.Name = "xPCKToolStripMenuItem";
            this.xPCKToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.xPCKToolStripMenuItem.Text = "XPCK";
            this.xPCKToolStripMenuItem.Click += new System.EventHandler(this.XPCKToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem2
            // 
            this.importToolStripMenuItem2.Name = "importToolStripMenuItem2";
            this.importToolStripMenuItem2.Size = new System.Drawing.Size(110, 22);
            this.importToolStripMenuItem2.Text = "Import";
            this.importToolStripMenuItem2.Click += new System.EventHandler(this.ImportToolStripMenuItem2_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.SaveToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // backButton
            // 
            this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.backButton.BackColor = System.Drawing.Color.White;
            this.backButton.Enabled = false;
            this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.backButton.Location = new System.Drawing.Point(3, 3);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(160, 20);
            this.backButton.TabIndex = 2;
            this.backButton.Text = "<-";
            this.backButton.UseVisualStyleBackColor = false;
            this.backButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // directoryInfoTextBox
            // 
            this.directoryInfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directoryInfoTextBox.BackColor = System.Drawing.Color.White;
            this.directoryInfoTextBox.Enabled = false;
            this.directoryInfoTextBox.Location = new System.Drawing.Point(8, 410);
            this.directoryInfoTextBox.Name = "directoryInfoTextBox";
            this.directoryInfoTextBox.ReadOnly = true;
            this.directoryInfoTextBox.Size = new System.Drawing.Size(764, 20);
            this.directoryInfoTextBox.TabIndex = 4;
            // 
            // openFileDialog2
            // 
            this.openFileDialog2.FileName = "openFileDialog2";
            // 
            // directoryTextBox
            // 
            this.directoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directoryTextBox.BackColor = System.Drawing.Color.White;
            this.directoryTextBox.Enabled = false;
            this.directoryTextBox.Location = new System.Drawing.Point(3, 3);
            this.directoryTextBox.Name = "directoryTextBox";
            this.directoryTextBox.ReadOnly = true;
            this.directoryTextBox.Size = new System.Drawing.Size(205, 20);
            this.directoryTextBox.TabIndex = 3;
            this.directoryTextBox.TextChanged += new System.EventHandler(this.DirectoryTextBox_TextChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.BackColor = System.Drawing.Color.White;
            this.progressBar1.Location = new System.Drawing.Point(11, 12);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(761, 20);
            this.progressBar1.TabIndex = 5;
            this.progressBar1.Visible = false;
            // 
            // openFileDialog3
            // 
            this.openFileDialog3.FileName = "openFileDialog3";
            // 
            // archiveOpenedTabControl
            // 
            this.archiveOpenedTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.archiveOpenedTabControl.Controls.Add(this.homeTabPage);
            this.archiveOpenedTabControl.Controls.Add(this.deleteTabPage);
            this.archiveOpenedTabControl.Location = new System.Drawing.Point(12, 27);
            this.archiveOpenedTabControl.Name = "archiveOpenedTabControl";
            this.archiveOpenedTabControl.SelectedIndex = 0;
            this.archiveOpenedTabControl.Size = new System.Drawing.Size(793, 462);
            this.archiveOpenedTabControl.TabIndex = 6;
            this.archiveOpenedTabControl.SelectedIndexChanged += new System.EventHandler(this.ArchiveOpenedTabControl_SelectedIndexChanged);
            this.archiveOpenedTabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.ArchiveOpenedTabControl_Selecting);
            // 
            // homeTabPage
            // 
            this.homeTabPage.Controls.Add(this.tableLayoutPanel1);
            this.homeTabPage.Controls.Add(this.directoryInfoTextBox);
            this.homeTabPage.Controls.Add(this.tableLayoutPanel2);
            this.homeTabPage.Controls.Add(this.progressBar1);
            this.homeTabPage.Location = new System.Drawing.Point(4, 22);
            this.homeTabPage.Name = "homeTabPage";
            this.homeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.homeTabPage.Size = new System.Drawing.Size(785, 436);
            this.homeTabPage.TabIndex = 0;
            this.homeTabPage.Text = "Home";
            this.homeTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.folderNameTreeView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.directoryListView, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 47);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 357F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(769, 357);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // folderNameTreeView
            // 
            this.folderNameTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folderNameTreeView.Location = new System.Drawing.Point(3, 3);
            this.folderNameTreeView.Name = "folderNameTreeView";
            this.folderNameTreeView.Size = new System.Drawing.Size(378, 351);
            this.folderNameTreeView.TabIndex = 0;
            this.folderNameTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.FolderNameTreeView_AfterSelect);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.08803F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.74194F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.19355F));
            this.tableLayoutPanel2.Controls.Add(this.panel3, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 6);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(775, 35);
            this.tableLayoutPanel2.TabIndex = 7;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.searchTextBox);
            this.panel3.Location = new System.Drawing.Point(388, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(384, 27);
            this.panel3.TabIndex = 8;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.BackColor = System.Drawing.Color.White;
            this.searchTextBox.Enabled = false;
            this.searchTextBox.Location = new System.Drawing.Point(0, 3);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(379, 20);
            this.searchTextBox.TabIndex = 5;
            this.searchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            this.searchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchTextBox_KeyPress);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.backButton);
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(165, 27);
            this.panel2.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.directoryTextBox);
            this.panel1.Controls.Add(this.resultTextBox);
            this.panel1.Location = new System.Drawing.Point(174, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(208, 27);
            this.panel1.TabIndex = 8;
            // 
            // resultTextBox
            // 
            this.resultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultTextBox.BackColor = System.Drawing.Color.White;
            this.resultTextBox.Location = new System.Drawing.Point(3, 3);
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.ReadOnly = true;
            this.resultTextBox.Size = new System.Drawing.Size(205, 20);
            this.resultTextBox.TabIndex = 6;
            this.resultTextBox.Visible = false;
            // 
            // deleteTabPage
            // 
            this.deleteTabPage.Location = new System.Drawing.Point(4, 22);
            this.deleteTabPage.Name = "deleteTabPage";
            this.deleteTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.deleteTabPage.Size = new System.Drawing.Size(785, 436);
            this.deleteTabPage.TabIndex = 1;
            this.deleteTabPage.Text = "-";
            this.deleteTabPage.UseVisualStyleBackColor = true;
            // 
            // PingouinWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 495);
            this.Controls.Add(this.archiveOpenedTabControl);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "PingouinWindow";
            this.Text = "Pingouin";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.ArchiveL5Window_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.ArchiveL5Window_DragEnter);
            this.directoryContextMenuStrip.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.archiveOpenedTabControl.ResumeLayout(false);
            this.homeTabPage.ResumeLayout(false);
            this.homeTabPage.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView directoryListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.ContextMenuStrip directoryContextMenuStrip;
        private System.Windows.Forms.TextBox directoryInfoTextBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog2;
        private System.Windows.Forms.ToolStripMenuItem folderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.TextBox directoryTextBox;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem xFSAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xPCKToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem2;
        private System.Windows.Forms.OpenFileDialog openFileDialog3;
        private System.Windows.Forms.TabControl archiveOpenedTabControl;
        private System.Windows.Forms.TabPage homeTabPage;
        private System.Windows.Forms.TreeView folderNameTreeView;
        private System.Windows.Forms.ToolStripMenuItem openInANewViewToolStripMenuItem;
        private System.Windows.Forms.TabPage deleteTabPage;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.TextBox resultTextBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

