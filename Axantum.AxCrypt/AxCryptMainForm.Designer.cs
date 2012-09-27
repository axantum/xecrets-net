namespace Axantum.AxCrypt
{
    partial class AxCryptMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxCryptMainForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonOpenEncrypted = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonEncrypt = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDecrypt = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.CloseAndRemoveOpenFilesButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.UpdateToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.helpButton = new System.Windows.Forms.ToolStripButton();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEncryptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.CloseOpenFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.OptionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LanguageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EnglishMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SwedishMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkVersionNow = new System.Windows.Forms.ToolStripMenuItem();
            this.setUpdateCheckUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.about = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusTabs = new System.Windows.Forms.TabControl();
            this.OpenFilesTab = new System.Windows.Forms.TabPage();
            this.OpenFilesListView = new System.Windows.Forms.ListView();
            this.OpenFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EncryptedSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ImageList48 = new System.Windows.Forms.ImageList(this.components);
            this.ImageList16 = new System.Windows.Forms.ImageList(this.components);
            this.RecentFilesTab = new System.Windows.Forms.TabPage();
            this.RecentFilesListView = new System.Windows.Forms.ListView();
            this.DecryptedFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastAccessTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EncryptedPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.WatchedFoldersTab = new System.Windows.Forms.TabPage();
            this.WatchedFoldersListView = new System.Windows.Forms.ListView();
            this.LogTab = new System.Windows.Forms.TabPage();
            this.LogOutput = new System.Windows.Forms.TextBox();
            this.ActiveFilePolling = new System.Windows.Forms.Timer(this.components);
            this.ProgressPanel = new System.Windows.Forms.TableLayoutPanel();
            this.RecentFilesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RemoveRecentFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenFilesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.EnterPassphraseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ProgressContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ProgressContextCancelMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.TrayNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.EncryptedFileManager = new Axantum.AxCrypt.Background();
            this.toolStrip1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.StatusTabs.SuspendLayout();
            this.OpenFilesTab.SuspendLayout();
            this.RecentFilesTab.SuspendLayout();
            this.WatchedFoldersTab.SuspendLayout();
            this.LogTab.SuspendLayout();
            this.RecentFilesContextMenu.SuspendLayout();
            this.OpenFilesContextMenu.SuspendLayout();
            this.ProgressContextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EncryptedFileManager)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonOpenEncrypted,
            this.toolStripButtonEncrypt,
            this.toolStripButtonDecrypt,
            this.toolStripSeparator1,
            this.CloseAndRemoveOpenFilesButton,
            this.toolStripSeparator4,
            this.UpdateToolStripButton,
            this.toolStripSeparator5,
            this.helpButton});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripButtonOpenEncrypted
            // 
            resources.ApplyResources(this.toolStripButtonOpenEncrypted, "toolStripButtonOpenEncrypted");
            this.toolStripButtonOpenEncrypted.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpenEncrypted.Name = "toolStripButtonOpenEncrypted";
            this.toolStripButtonOpenEncrypted.Click += new System.EventHandler(this.toolStripButtonOpenEncrypted_Click);
            // 
            // toolStripButtonEncrypt
            // 
            resources.ApplyResources(this.toolStripButtonEncrypt, "toolStripButtonEncrypt");
            this.toolStripButtonEncrypt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonEncrypt.Name = "toolStripButtonEncrypt";
            this.toolStripButtonEncrypt.Click += new System.EventHandler(this.toolStripButtonEncrypt_Click);
            // 
            // toolStripButtonDecrypt
            // 
            resources.ApplyResources(this.toolStripButtonDecrypt, "toolStripButtonDecrypt");
            this.toolStripButtonDecrypt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDecrypt.Name = "toolStripButtonDecrypt";
            this.toolStripButtonDecrypt.Click += new System.EventHandler(this.toolStripButtonDecrypt_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // CloseAndRemoveOpenFilesButton
            // 
            resources.ApplyResources(this.CloseAndRemoveOpenFilesButton, "CloseAndRemoveOpenFilesButton");
            this.CloseAndRemoveOpenFilesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.CloseAndRemoveOpenFilesButton.Name = "CloseAndRemoveOpenFilesButton";
            this.CloseAndRemoveOpenFilesButton.Click += new System.EventHandler(this.CloseOpenFilesButton_Click);
            // 
            // toolStripSeparator4
            // 
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // UpdateToolStripButton
            // 
            resources.ApplyResources(this.UpdateToolStripButton, "UpdateToolStripButton");
            this.UpdateToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.UpdateToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.refreshgreen;
            this.UpdateToolStripButton.Name = "UpdateToolStripButton";
            this.UpdateToolStripButton.Click += new System.EventHandler(this.UpdateToolStripButton_Click);
            // 
            // toolStripSeparator5
            // 
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // helpButton
            // 
            resources.ApplyResources(this.helpButton, "helpButton");
            this.helpButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpButton.Name = "helpButton";
            this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
            // 
            // menuStripMain
            // 
            resources.ApplyResources(this.menuStripMain, "menuStripMain");
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.DebugToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStripMain.Name = "menuStripMain";
            // 
            // fileToolStripMenuItem
            // 
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEncryptedToolStripMenuItem,
            this.encryptToolStripMenuItem,
            this.decryptToolStripMenuItem,
            this.toolStripSeparator2,
            this.CloseOpenFilesMenuItem,
            this.toolStripSeparator3,
            this.OptionsMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            // 
            // openEncryptedToolStripMenuItem
            // 
            resources.ApplyResources(this.openEncryptedToolStripMenuItem, "openEncryptedToolStripMenuItem");
            this.openEncryptedToolStripMenuItem.Name = "openEncryptedToolStripMenuItem";
            this.openEncryptedToolStripMenuItem.Click += new System.EventHandler(this.openEncryptedToolStripMenuItem_Click);
            // 
            // encryptToolStripMenuItem
            // 
            resources.ApplyResources(this.encryptToolStripMenuItem, "encryptToolStripMenuItem");
            this.encryptToolStripMenuItem.Name = "encryptToolStripMenuItem";
            this.encryptToolStripMenuItem.Click += new System.EventHandler(this.encryptToolStripMenuItem_Click);
            // 
            // decryptToolStripMenuItem
            // 
            resources.ApplyResources(this.decryptToolStripMenuItem, "decryptToolStripMenuItem");
            this.decryptToolStripMenuItem.Name = "decryptToolStripMenuItem";
            this.decryptToolStripMenuItem.Click += new System.EventHandler(this.decryptToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // CloseOpenFilesMenuItem
            // 
            resources.ApplyResources(this.CloseOpenFilesMenuItem, "CloseOpenFilesMenuItem");
            this.CloseOpenFilesMenuItem.Name = "CloseOpenFilesMenuItem";
            this.CloseOpenFilesMenuItem.Click += new System.EventHandler(this.CloseOpenFilesMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // OptionsMenuItem
            // 
            resources.ApplyResources(this.OptionsMenuItem, "OptionsMenuItem");
            this.OptionsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LanguageMenuItem,
            this.debugToolStripMenuItem1});
            this.OptionsMenuItem.Name = "OptionsMenuItem";
            // 
            // LanguageMenuItem
            // 
            resources.ApplyResources(this.LanguageMenuItem, "LanguageMenuItem");
            this.LanguageMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EnglishMenuItem,
            this.SwedishMenuItem});
            this.LanguageMenuItem.Name = "LanguageMenuItem";
            this.LanguageMenuItem.DropDownOpening += new System.EventHandler(this.LanguageMenuItem_DropDownOpening);
            // 
            // EnglishMenuItem
            // 
            resources.ApplyResources(this.EnglishMenuItem, "EnglishMenuItem");
            this.EnglishMenuItem.Name = "EnglishMenuItem";
            this.EnglishMenuItem.Tag = "en";
            this.EnglishMenuItem.Click += new System.EventHandler(this.EnglishMenuItem_Click);
            // 
            // SwedishMenuItem
            // 
            resources.ApplyResources(this.SwedishMenuItem, "SwedishMenuItem");
            this.SwedishMenuItem.Name = "SwedishMenuItem";
            this.SwedishMenuItem.Tag = "sv";
            this.SwedishMenuItem.Click += new System.EventHandler(this.SwedishMenuItem_Click);
            // 
            // debugToolStripMenuItem1
            // 
            resources.ApplyResources(this.debugToolStripMenuItem1, "debugToolStripMenuItem1");
            this.debugToolStripMenuItem1.Name = "debugToolStripMenuItem1";
            this.debugToolStripMenuItem1.Click += new System.EventHandler(this.debugToolStripMenuItem1_Click);
            // 
            // exitToolStripMenuItem
            // 
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // DebugToolStripMenuItem
            // 
            resources.ApplyResources(this.DebugToolStripMenuItem, "DebugToolStripMenuItem");
            this.DebugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkVersionNow,
            this.setUpdateCheckUrlToolStripMenuItem});
            this.DebugToolStripMenuItem.Name = "DebugToolStripMenuItem";
            // 
            // checkVersionNow
            // 
            resources.ApplyResources(this.checkVersionNow, "checkVersionNow");
            this.checkVersionNow.Name = "checkVersionNow";
            this.checkVersionNow.Click += new System.EventHandler(this.checkVersionNow_Click);
            // 
            // setUpdateCheckUrlToolStripMenuItem
            // 
            resources.ApplyResources(this.setUpdateCheckUrlToolStripMenuItem, "setUpdateCheckUrlToolStripMenuItem");
            this.setUpdateCheckUrlToolStripMenuItem.Name = "setUpdateCheckUrlToolStripMenuItem";
            this.setUpdateCheckUrlToolStripMenuItem.Click += new System.EventHandler(this.setUpdateCheckUrlToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewHelpMenuItem,
            this.about});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            // 
            // viewHelpMenuItem
            // 
            resources.ApplyResources(this.viewHelpMenuItem, "viewHelpMenuItem");
            this.viewHelpMenuItem.Name = "viewHelpMenuItem";
            this.viewHelpMenuItem.Click += new System.EventHandler(this.viewHelpMenuItem_Click);
            // 
            // about
            // 
            resources.ApplyResources(this.about, "about");
            this.about.Name = "about";
            this.about.Click += new System.EventHandler(this.about_Click);
            // 
            // StatusTabs
            // 
            resources.ApplyResources(this.StatusTabs, "StatusTabs");
            this.StatusTabs.Controls.Add(this.OpenFilesTab);
            this.StatusTabs.Controls.Add(this.RecentFilesTab);
            this.StatusTabs.Controls.Add(this.WatchedFoldersTab);
            this.StatusTabs.Controls.Add(this.LogTab);
            this.StatusTabs.Name = "StatusTabs";
            this.StatusTabs.SelectedIndex = 0;
            // 
            // OpenFilesTab
            // 
            resources.ApplyResources(this.OpenFilesTab, "OpenFilesTab");
            this.OpenFilesTab.Controls.Add(this.OpenFilesListView);
            this.OpenFilesTab.Name = "OpenFilesTab";
            this.OpenFilesTab.UseVisualStyleBackColor = true;
            // 
            // OpenFilesListView
            // 
            resources.ApplyResources(this.OpenFilesListView, "OpenFilesListView");
            this.OpenFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OpenFile,
            this.EncryptedSource});
            this.OpenFilesListView.FullRowSelect = true;
            this.OpenFilesListView.LargeImageList = this.ImageList48;
            this.OpenFilesListView.Name = "OpenFilesListView";
            this.OpenFilesListView.SmallImageList = this.ImageList16;
            this.OpenFilesListView.UseCompatibleStateImageBehavior = false;
            this.OpenFilesListView.View = System.Windows.Forms.View.Details;
            this.OpenFilesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OpenFilesListView_MouseClick);
            this.OpenFilesListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OpenFilesListView_MouseDoubleClick);
            // 
            // OpenFile
            // 
            resources.ApplyResources(this.OpenFile, "OpenFile");
            // 
            // EncryptedSource
            // 
            resources.ApplyResources(this.EncryptedSource, "EncryptedSource");
            // 
            // ImageList48
            // 
            this.ImageList48.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList48.ImageStream")));
            this.ImageList48.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList48.Images.SetKeyName(0, "ActiveFile");
            this.ImageList48.Images.SetKeyName(1, "InactiveFile");
            this.ImageList48.Images.SetKeyName(2, "Exclamation");
            // 
            // ImageList16
            // 
            this.ImageList16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList16.ImageStream")));
            this.ImageList16.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList16.Images.SetKeyName(0, "ActiveFile");
            this.ImageList16.Images.SetKeyName(1, "InactiveFile");
            this.ImageList16.Images.SetKeyName(2, "Exclamation");
            // 
            // RecentFilesTab
            // 
            resources.ApplyResources(this.RecentFilesTab, "RecentFilesTab");
            this.RecentFilesTab.Controls.Add(this.RecentFilesListView);
            this.RecentFilesTab.Name = "RecentFilesTab";
            this.RecentFilesTab.UseVisualStyleBackColor = true;
            // 
            // RecentFilesListView
            // 
            resources.ApplyResources(this.RecentFilesListView, "RecentFilesListView");
            this.RecentFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DecryptedFile,
            this.LastAccessTime,
            this.EncryptedPath});
            this.RecentFilesListView.FullRowSelect = true;
            this.RecentFilesListView.LargeImageList = this.ImageList48;
            this.RecentFilesListView.Name = "RecentFilesListView";
            this.RecentFilesListView.SmallImageList = this.ImageList16;
            this.RecentFilesListView.UseCompatibleStateImageBehavior = false;
            this.RecentFilesListView.View = System.Windows.Forms.View.Details;
            this.RecentFilesListView.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.RecentFilesListView_ColumnWidthChanged);
            this.RecentFilesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.RecentFilesListView_MouseClick);
            this.RecentFilesListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RecentFilesListView_MouseDoubleClick);
            // 
            // DecryptedFile
            // 
            resources.ApplyResources(this.DecryptedFile, "DecryptedFile");
            // 
            // LastAccessTime
            // 
            resources.ApplyResources(this.LastAccessTime, "LastAccessTime");
            // 
            // EncryptedPath
            // 
            resources.ApplyResources(this.EncryptedPath, "EncryptedPath");
            // 
            // WatchedFoldersTab
            // 
            resources.ApplyResources(this.WatchedFoldersTab, "WatchedFoldersTab");
            this.WatchedFoldersTab.Controls.Add(this.WatchedFoldersListView);
            this.WatchedFoldersTab.Name = "WatchedFoldersTab";
            this.WatchedFoldersTab.UseVisualStyleBackColor = true;
            // 
            // WatchedFoldersListView
            // 
            resources.ApplyResources(this.WatchedFoldersListView, "WatchedFoldersListView");
            this.WatchedFoldersListView.FullRowSelect = true;
            this.WatchedFoldersListView.Name = "WatchedFoldersListView";
            this.WatchedFoldersListView.UseCompatibleStateImageBehavior = false;
            // 
            // LogTab
            // 
            resources.ApplyResources(this.LogTab, "LogTab");
            this.LogTab.Controls.Add(this.LogOutput);
            this.LogTab.Name = "LogTab";
            this.LogTab.UseVisualStyleBackColor = true;
            // 
            // LogOutput
            // 
            resources.ApplyResources(this.LogOutput, "LogOutput");
            this.LogOutput.Name = "LogOutput";
            this.LogOutput.ReadOnly = true;
            // 
            // ActiveFilePolling
            // 
            this.ActiveFilePolling.Interval = 1000;
            this.ActiveFilePolling.Tick += new System.EventHandler(this.ActiveFilePolling_Tick);
            // 
            // ProgressPanel
            // 
            resources.ApplyResources(this.ProgressPanel, "ProgressPanel");
            this.ProgressPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.ProgressPanel.Name = "ProgressPanel";
            // 
            // RecentFilesContextMenu
            // 
            resources.ApplyResources(this.RecentFilesContextMenu, "RecentFilesContextMenu");
            this.RecentFilesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RemoveRecentFileMenuItem});
            this.RecentFilesContextMenu.Name = "RecentFilesContextMenu";
            // 
            // RemoveRecentFileMenuItem
            // 
            resources.ApplyResources(this.RemoveRecentFileMenuItem, "RemoveRecentFileMenuItem");
            this.RemoveRecentFileMenuItem.Name = "RemoveRecentFileMenuItem";
            this.RemoveRecentFileMenuItem.Click += new System.EventHandler(this.RemoveRecentFileMenuItem_Click);
            // 
            // OpenFilesContextMenu
            // 
            resources.ApplyResources(this.OpenFilesContextMenu, "OpenFilesContextMenu");
            this.OpenFilesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EnterPassphraseMenuItem});
            this.OpenFilesContextMenu.Name = "OpenFilesContextMenu";
            // 
            // EnterPassphraseMenuItem
            // 
            resources.ApplyResources(this.EnterPassphraseMenuItem, "EnterPassphraseMenuItem");
            this.EnterPassphraseMenuItem.Name = "EnterPassphraseMenuItem";
            this.EnterPassphraseMenuItem.Click += new System.EventHandler(this.EnterPassphraseMenuItem_Click);
            // 
            // ProgressContextMenu
            // 
            resources.ApplyResources(this.ProgressContextMenu, "ProgressContextMenu");
            this.ProgressContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ProgressContextCancelMenu});
            this.ProgressContextMenu.Name = "ProgressContextMenu";
            // 
            // ProgressContextCancelMenu
            // 
            resources.ApplyResources(this.ProgressContextCancelMenu, "ProgressContextCancelMenu");
            this.ProgressContextCancelMenu.Name = "ProgressContextCancelMenu";
            this.ProgressContextCancelMenu.Click += new System.EventHandler(this.ProgressContextCancelMenu_Click);
            // 
            // TrayNotifyIcon
            // 
            resources.ApplyResources(this.TrayNotifyIcon, "TrayNotifyIcon");
            this.TrayNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.TrayNotifyIcon_MouseDoubleClick);
            // 
            // AxCryptMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ProgressPanel);
            this.Controls.Add(this.StatusTabs);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "AxCryptMainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxCryptMainForm_FormClosing);
            this.Load += new System.EventHandler(this.AxCryptMainForm_Load);
            this.Resize += new System.EventHandler(this.AxCryptMainForm_Resize);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.StatusTabs.ResumeLayout(false);
            this.OpenFilesTab.ResumeLayout(false);
            this.RecentFilesTab.ResumeLayout(false);
            this.WatchedFoldersTab.ResumeLayout(false);
            this.LogTab.ResumeLayout(false);
            this.LogTab.PerformLayout();
            this.RecentFilesContextMenu.ResumeLayout(false);
            this.OpenFilesContextMenu.ResumeLayout(false);
            this.ProgressContextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.EncryptedFileManager)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpButton;
        private System.Windows.Forms.ToolStripButton toolStripButtonEncrypt;
        private System.Windows.Forms.ToolStripButton toolStripButtonDecrypt;
        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenEncrypted;
        private System.Windows.Forms.ToolStripMenuItem openEncryptedToolStripMenuItem;
        private System.Windows.Forms.TabControl StatusTabs;
        private System.Windows.Forms.TabPage OpenFilesTab;
        private System.Windows.Forms.ListView OpenFilesListView;
        private System.Windows.Forms.TabPage WatchedFoldersTab;
        private System.Windows.Forms.ListView WatchedFoldersListView;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Timer ActiveFilePolling;
        private System.Windows.Forms.ImageList ImageList48;
        private System.Windows.Forms.ImageList ImageList16;
        private System.Windows.Forms.TabPage RecentFilesTab;
        private System.Windows.Forms.ListView RecentFilesListView;
        private System.Windows.Forms.ColumnHeader OpenFile;
        private System.Windows.Forms.ColumnHeader EncryptedSource;
        private System.Windows.Forms.ColumnHeader EncryptedPath;
        private System.Windows.Forms.ColumnHeader LastAccessTime;
        private System.Windows.Forms.ColumnHeader DecryptedFile;
        private System.Windows.Forms.TabPage LogTab;
        private System.Windows.Forms.TextBox LogOutput;
        private System.Windows.Forms.ContextMenuStrip RecentFilesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem RemoveRecentFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CloseOpenFilesMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton CloseAndRemoveOpenFilesButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ContextMenuStrip OpenFilesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem EnterPassphraseMenuItem;
        internal System.Windows.Forms.TableLayoutPanel ProgressPanel;
        private System.Windows.Forms.ContextMenuStrip ProgressContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ProgressContextCancelMenu;
        internal Background EncryptedFileManager;
        private System.Windows.Forms.NotifyIcon TrayNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem OptionsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem LanguageMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EnglishMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SwedishMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton UpdateToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem DebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setUpdateCheckUrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem checkVersionNow;
        private System.Windows.Forms.ToolStripMenuItem about;
        private System.Windows.Forms.ToolStripMenuItem viewHelpMenuItem;
    }
}

