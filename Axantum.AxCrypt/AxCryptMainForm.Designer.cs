namespace Axantum.AxCrypt
{
    partial class AxCryptMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AxCryptMainForm));
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.openEncryptedToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.encryptToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.decryptToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.encryptionKeyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.closeAndRemoveOpenFilesToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.updateToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEncryptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.closeOpenFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swedishLanguageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkVersionNowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setUpdateCheckUrlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeImageList = new System.Windows.Forms.ImageList(this.components);
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            this.activeFilePollingTimer = new System.Windows.Forms.Timer(this.components);
            this.progressTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.recentFilesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeRecentFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptAndRemoveFromListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFilesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.enterPassphraseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.progressContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.progressContextCancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTabPage = new System.Windows.Forms.TabPage();
            this.logOutputTextBox = new System.Windows.Forms.TextBox();
            this.watchedFoldersTabPage = new System.Windows.Forms.TabPage();
            this.watchedFoldersListView = new System.Windows.Forms.ListView();
            this.recentFilesTabPage = new System.Windows.Forms.TabPage();
            this.recentFilesListView = new System.Windows.Forms.ListView();
            this.decryptedFileColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lastAccessTimeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.encryptedPathColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusTabControl = new System.Windows.Forms.TabControl();
            this.persistentState = new Axantum.AxCrypt.PersistentState(this.components);
            this.progressBackgroundWorker = new Axantum.AxCrypt.ProgressBackgroundWorker(this.components);
            this.backgroundMonitor = new Axantum.AxCrypt.BackgroundMonitor(this.components);
            this.mainToolStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.recentFilesContextMenuStrip.SuspendLayout();
            this.openFilesContextMenuStrip.SuspendLayout();
            this.progressContextMenuStrip.SuspendLayout();
            this.logTabPage.SuspendLayout();
            this.watchedFoldersTabPage.SuspendLayout();
            this.recentFilesTabPage.SuspendLayout();
            this.statusTabControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.persistentState)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.AllowMerge = false;
            resources.ApplyResources(this.mainToolStrip, "mainToolStrip");
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEncryptedToolStripButton,
            this.encryptToolStripButton,
            this.decryptToolStripButton,
            this.toolStripSeparator1,
            this.encryptionKeyToolStripButton,
            this.closeAndRemoveOpenFilesToolStripButton,
            this.toolStripSeparator4,
            this.updateToolStripButton,
            this.toolStripSeparator5,
            this.helpToolStripButton});
            this.mainToolStrip.Name = "mainToolStrip";
            // 
            // openEncryptedToolStripButton
            // 
            resources.ApplyResources(this.openEncryptedToolStripButton, "openEncryptedToolStripButton");
            this.openEncryptedToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openEncryptedToolStripButton.Name = "openEncryptedToolStripButton";
            this.openEncryptedToolStripButton.Click += new System.EventHandler(this.openEncryptedToolStripButton_Click);
            // 
            // encryptToolStripButton
            // 
            resources.ApplyResources(this.encryptToolStripButton, "encryptToolStripButton");
            this.encryptToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.encryptToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.encryptlock32;
            this.encryptToolStripButton.Name = "encryptToolStripButton";
            this.encryptToolStripButton.Click += new System.EventHandler(this.toolStripButtonEncrypt_Click);
            // 
            // decryptToolStripButton
            // 
            resources.ApplyResources(this.decryptToolStripButton, "decryptToolStripButton");
            this.decryptToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.decryptToolStripButton.Name = "decryptToolStripButton";
            this.decryptToolStripButton.Click += new System.EventHandler(this.decryptToolStripButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // encryptionKeyToolStripButton
            // 
            this.encryptionKeyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.encryptionKeyToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.encryptionkeygreen32;
            resources.ApplyResources(this.encryptionKeyToolStripButton, "encryptionKeyToolStripButton");
            this.encryptionKeyToolStripButton.Name = "encryptionKeyToolStripButton";
            this.encryptionKeyToolStripButton.Click += new System.EventHandler(this.encryptionKeyToolStripButton_Click);
            // 
            // closeAndRemoveOpenFilesToolStripButton
            // 
            resources.ApplyResources(this.closeAndRemoveOpenFilesToolStripButton, "closeAndRemoveOpenFilesToolStripButton");
            this.closeAndRemoveOpenFilesToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.closeAndRemoveOpenFilesToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.closeremovestandbyred32;
            this.closeAndRemoveOpenFilesToolStripButton.Name = "closeAndRemoveOpenFilesToolStripButton";
            this.closeAndRemoveOpenFilesToolStripButton.Click += new System.EventHandler(this.closeAndRemoveOpenFilesToolStripButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // updateToolStripButton
            // 
            resources.ApplyResources(this.updateToolStripButton, "updateToolStripButton");
            this.updateToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.updateToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.refreshgreen;
            this.updateToolStripButton.Name = "updateToolStripButton";
            this.updateToolStripButton.Click += new System.EventHandler(this.updateToolStripButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.helpToolStripButton.Image = global::Axantum.AxCrypt.Properties.Resources.helpquestiongreen32;
            resources.ApplyResources(this.helpToolStripButton, "helpToolStripButton");
            this.helpToolStripButton.Name = "helpToolStripButton";
            this.helpToolStripButton.Click += new System.EventHandler(this.helpToolStripButton_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.mainMenuStrip, "mainMenuStrip");
            this.mainMenuStrip.Name = "mainMenuStrip";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEncryptedToolStripMenuItem,
            this.encryptToolStripMenuItem,
            this.decryptToolStripMenuItem,
            this.toolStripSeparator2,
            this.closeOpenFilesToolStripMenuItem,
            this.toolStripSeparator3,
            this.optionsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
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
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // closeOpenFilesToolStripMenuItem
            // 
            resources.ApplyResources(this.closeOpenFilesToolStripMenuItem, "closeOpenFilesToolStripMenuItem");
            this.closeOpenFilesToolStripMenuItem.Name = "closeOpenFilesToolStripMenuItem";
            this.closeOpenFilesToolStripMenuItem.Click += new System.EventHandler(this.closeOpenFilesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.languageToolStripMenuItem,
            this.debugOptionsToolStripMenuItem});
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishLanguageToolStripMenuItem,
            this.swedishLanguageToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
            this.languageToolStripMenuItem.DropDownOpening += new System.EventHandler(this.languageToolStripMenuItem_DropDownOpening);
            // 
            // englishLanguageToolStripMenuItem
            // 
            this.englishLanguageToolStripMenuItem.Name = "englishLanguageToolStripMenuItem";
            resources.ApplyResources(this.englishLanguageToolStripMenuItem, "englishLanguageToolStripMenuItem");
            this.englishLanguageToolStripMenuItem.Tag = "en";
            this.englishLanguageToolStripMenuItem.Click += new System.EventHandler(this.englishLanguageToolStripMenuItem_Click);
            // 
            // swedishLanguageToolStripMenuItem
            // 
            this.swedishLanguageToolStripMenuItem.Name = "swedishLanguageToolStripMenuItem";
            resources.ApplyResources(this.swedishLanguageToolStripMenuItem, "swedishLanguageToolStripMenuItem");
            this.swedishLanguageToolStripMenuItem.Tag = "sv";
            this.swedishLanguageToolStripMenuItem.Click += new System.EventHandler(this.swedishLanguageToolStripMenuItem_Click);
            // 
            // debugOptionsToolStripMenuItem
            // 
            this.debugOptionsToolStripMenuItem.Name = "debugOptionsToolStripMenuItem";
            resources.ApplyResources(this.debugOptionsToolStripMenuItem, "debugOptionsToolStripMenuItem");
            this.debugOptionsToolStripMenuItem.Click += new System.EventHandler(this.debugOptionsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkVersionNowToolStripMenuItem,
            this.setUpdateCheckUrlToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            resources.ApplyResources(this.debugToolStripMenuItem, "debugToolStripMenuItem");
            // 
            // checkVersionNowToolStripMenuItem
            // 
            this.checkVersionNowToolStripMenuItem.Name = "checkVersionNowToolStripMenuItem";
            resources.ApplyResources(this.checkVersionNowToolStripMenuItem, "checkVersionNowToolStripMenuItem");
            this.checkVersionNowToolStripMenuItem.Click += new System.EventHandler(this.checkVersionNowToolStripMenuItem_Click);
            // 
            // setUpdateCheckUrlToolStripMenuItem
            // 
            this.setUpdateCheckUrlToolStripMenuItem.Name = "setUpdateCheckUrlToolStripMenuItem";
            resources.ApplyResources(this.setUpdateCheckUrlToolStripMenuItem, "setUpdateCheckUrlToolStripMenuItem");
            this.setUpdateCheckUrlToolStripMenuItem.Click += new System.EventHandler(this.setUpdateCheckUrlToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewHelpMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // viewHelpMenuItem
            // 
            this.viewHelpMenuItem.Name = "viewHelpMenuItem";
            resources.ApplyResources(this.viewHelpMenuItem, "viewHelpMenuItem");
            this.viewHelpMenuItem.Click += new System.EventHandler(this.viewHelpMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // largeImageList
            // 
            this.largeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("largeImageList.ImageStream")));
            this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.largeImageList.Images.SetKeyName(0, "ActiveFile");
            this.largeImageList.Images.SetKeyName(1, "InactiveFile");
            this.largeImageList.Images.SetKeyName(2, "Exclamation");
            // 
            // smallImageList
            // 
            this.smallImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("smallImageList.ImageStream")));
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.smallImageList.Images.SetKeyName(0, "ActiveFile");
            this.smallImageList.Images.SetKeyName(1, "InactiveFile");
            this.smallImageList.Images.SetKeyName(2, "Exclamation");
            this.smallImageList.Images.SetKeyName(3, "DecryptedFile");
            this.smallImageList.Images.SetKeyName(4, "DecryptedUnknownKeyFile");
            this.smallImageList.Images.SetKeyName(5, "ActiveFileKnownKey");
            // 
            // activeFilePollingTimer
            // 
            this.activeFilePollingTimer.Interval = 1000;
            this.activeFilePollingTimer.Tick += new System.EventHandler(this.activeFilePollingTimer_Tick);
            // 
            // progressTableLayoutPanel
            // 
            resources.ApplyResources(this.progressTableLayoutPanel, "progressTableLayoutPanel");
            this.progressTableLayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.progressTableLayoutPanel.Name = "progressTableLayoutPanel";
            // 
            // recentFilesContextMenuStrip
            // 
            this.recentFilesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeRecentFileToolStripMenuItem,
            this.decryptAndRemoveFromListToolStripMenuItem});
            this.recentFilesContextMenuStrip.Name = "RecentFilesContextMenu";
            resources.ApplyResources(this.recentFilesContextMenuStrip, "recentFilesContextMenuStrip");
            // 
            // removeRecentFileToolStripMenuItem
            // 
            this.removeRecentFileToolStripMenuItem.Name = "removeRecentFileToolStripMenuItem";
            resources.ApplyResources(this.removeRecentFileToolStripMenuItem, "removeRecentFileToolStripMenuItem");
            this.removeRecentFileToolStripMenuItem.Click += new System.EventHandler(this.removeRecentFileToolStripMenuItem_Click);
            // 
            // decryptAndRemoveFromListToolStripMenuItem
            // 
            this.decryptAndRemoveFromListToolStripMenuItem.Name = "decryptAndRemoveFromListToolStripMenuItem";
            resources.ApplyResources(this.decryptAndRemoveFromListToolStripMenuItem, "decryptAndRemoveFromListToolStripMenuItem");
            this.decryptAndRemoveFromListToolStripMenuItem.Click += new System.EventHandler(this.decryptAndRemoveFromListToolStripMenuItem_Click);
            // 
            // openFilesContextMenuStrip
            // 
            this.openFilesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enterPassphraseToolStripMenuItem});
            this.openFilesContextMenuStrip.Name = "OpenFilesContextMenu";
            resources.ApplyResources(this.openFilesContextMenuStrip, "openFilesContextMenuStrip");
            // 
            // enterPassphraseToolStripMenuItem
            // 
            this.enterPassphraseToolStripMenuItem.Name = "enterPassphraseToolStripMenuItem";
            resources.ApplyResources(this.enterPassphraseToolStripMenuItem, "enterPassphraseToolStripMenuItem");
            this.enterPassphraseToolStripMenuItem.Click += new System.EventHandler(this.enterPassphraseToolStripMenuItem_Click);
            // 
            // progressContextMenuStrip
            // 
            this.progressContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressContextCancelToolStripMenuItem});
            this.progressContextMenuStrip.Name = "ProgressContextMenu";
            resources.ApplyResources(this.progressContextMenuStrip, "progressContextMenuStrip");
            // 
            // progressContextCancelToolStripMenuItem
            // 
            this.progressContextCancelToolStripMenuItem.Name = "progressContextCancelToolStripMenuItem";
            resources.ApplyResources(this.progressContextCancelToolStripMenuItem, "progressContextCancelToolStripMenuItem");
            this.progressContextCancelToolStripMenuItem.Click += new System.EventHandler(this.progressContextCancelToolStripMenuItem_Click);
            // 
            // logTabPage
            // 
            this.logTabPage.Controls.Add(this.logOutputTextBox);
            resources.ApplyResources(this.logTabPage, "logTabPage");
            this.logTabPage.Name = "logTabPage";
            this.logTabPage.UseVisualStyleBackColor = true;
            // 
            // logOutputTextBox
            // 
            resources.ApplyResources(this.logOutputTextBox, "logOutputTextBox");
            this.logOutputTextBox.Name = "logOutputTextBox";
            this.logOutputTextBox.ReadOnly = true;
            // 
            // watchedFoldersTabPage
            // 
            this.watchedFoldersTabPage.Controls.Add(this.watchedFoldersListView);
            resources.ApplyResources(this.watchedFoldersTabPage, "watchedFoldersTabPage");
            this.watchedFoldersTabPage.Name = "watchedFoldersTabPage";
            this.watchedFoldersTabPage.UseVisualStyleBackColor = true;
            // 
            // watchedFoldersListView
            // 
            resources.ApplyResources(this.watchedFoldersListView, "watchedFoldersListView");
            this.watchedFoldersListView.FullRowSelect = true;
            this.watchedFoldersListView.Name = "watchedFoldersListView";
            this.watchedFoldersListView.ShowItemToolTips = true;
            this.watchedFoldersListView.UseCompatibleStateImageBehavior = false;
            // 
            // recentFilesTabPage
            // 
            this.recentFilesTabPage.Controls.Add(this.recentFilesListView);
            resources.ApplyResources(this.recentFilesTabPage, "recentFilesTabPage");
            this.recentFilesTabPage.Name = "recentFilesTabPage";
            this.recentFilesTabPage.UseVisualStyleBackColor = true;
            // 
            // recentFilesListView
            // 
            this.recentFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.decryptedFileColumnHeader,
            this.lastAccessTimeColumnHeader,
            this.encryptedPathColumnHeader});
            resources.ApplyResources(this.recentFilesListView, "recentFilesListView");
            this.recentFilesListView.FullRowSelect = true;
            this.recentFilesListView.LargeImageList = this.largeImageList;
            this.recentFilesListView.Name = "recentFilesListView";
            this.recentFilesListView.ShowItemToolTips = true;
            this.recentFilesListView.SmallImageList = this.smallImageList;
            this.recentFilesListView.UseCompatibleStateImageBehavior = false;
            this.recentFilesListView.View = System.Windows.Forms.View.Details;
            this.recentFilesListView.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.recentFilesListView_ColumnWidthChanged);
            this.recentFilesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.recentFilesListView_MouseClick);
            this.recentFilesListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.recentFilesListView_MouseDoubleClick);
            // 
            // decryptedFileColumnHeader
            // 
            resources.ApplyResources(this.decryptedFileColumnHeader, "decryptedFileColumnHeader");
            // 
            // lastAccessTimeColumnHeader
            // 
            resources.ApplyResources(this.lastAccessTimeColumnHeader, "lastAccessTimeColumnHeader");
            // 
            // encryptedPathColumnHeader
            // 
            resources.ApplyResources(this.encryptedPathColumnHeader, "encryptedPathColumnHeader");
            // 
            // statusTabControl
            // 
            resources.ApplyResources(this.statusTabControl, "statusTabControl");
            this.statusTabControl.Controls.Add(this.recentFilesTabPage);
            this.statusTabControl.Controls.Add(this.watchedFoldersTabPage);
            this.statusTabControl.Controls.Add(this.logTabPage);
            this.statusTabControl.Name = "statusTabControl";
            this.statusTabControl.SelectedIndex = 0;
            // 
            // persistentState
            // 
            this.persistentState.Current = null;
            // 
            // progressBackgroundWorker
            // 
            this.progressBackgroundWorker.ProgressBarCreated += new System.EventHandler<System.Windows.Forms.ControlEventArgs>(this.progressBackgroundWorker_ProgressBarCreated);
            this.progressBackgroundWorker.ProgressBarClicked += new System.EventHandler<System.Windows.Forms.MouseEventArgs>(this.progressBackgroundWorker_ProgressBarClicked);
            // 
            // AxCryptMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressTableLayoutPanel);
            this.Controls.Add(this.statusTabControl);
            this.Controls.Add(this.mainToolStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "AxCryptMainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AxCryptMainForm_FormClosing);
            this.Load += new System.EventHandler(this.AxCryptMainForm_Load);
            this.Resize += new System.EventHandler(this.AxCryptMainForm_Resize);
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.recentFilesContextMenuStrip.ResumeLayout(false);
            this.openFilesContextMenuStrip.ResumeLayout(false);
            this.progressContextMenuStrip.ResumeLayout(false);
            this.logTabPage.ResumeLayout(false);
            this.logTabPage.PerformLayout();
            this.watchedFoldersTabPage.ResumeLayout(false);
            this.recentFilesTabPage.ResumeLayout(false);
            this.statusTabControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.persistentState)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backgroundMonitor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
        private System.Windows.Forms.ToolStripButton encryptToolStripButton;
        private System.Windows.Forms.ToolStripButton decryptToolStripButton;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem encryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton openEncryptedToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem openEncryptedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Timer activeFilePollingTimer;
        private System.Windows.Forms.ImageList largeImageList;
        private System.Windows.Forms.ImageList smallImageList;
        private System.Windows.Forms.ContextMenuStrip recentFilesContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeRecentFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeOpenFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton closeAndRemoveOpenFilesToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ContextMenuStrip openFilesContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem enterPassphraseToolStripMenuItem;
        internal System.Windows.Forms.TableLayoutPanel progressTableLayoutPanel;
        private System.Windows.Forms.ContextMenuStrip progressContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem progressContextCancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem swedishLanguageToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton updateToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setUpdateCheckUrlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem checkVersionNowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewHelpMenuItem;
        private PersistentState persistentState;
        private ProgressBackgroundWorker progressBackgroundWorker;
        private BackgroundMonitor backgroundMonitor;
        private System.Windows.Forms.TabPage logTabPage;
        private System.Windows.Forms.TextBox logOutputTextBox;
        private System.Windows.Forms.TabPage watchedFoldersTabPage;
        private System.Windows.Forms.ListView watchedFoldersListView;
        private System.Windows.Forms.TabPage recentFilesTabPage;
        private System.Windows.Forms.ListView recentFilesListView;
        private System.Windows.Forms.ColumnHeader decryptedFileColumnHeader;
        private System.Windows.Forms.ColumnHeader lastAccessTimeColumnHeader;
        private System.Windows.Forms.ColumnHeader encryptedPathColumnHeader;
        private System.Windows.Forms.TabControl statusTabControl;
        private System.Windows.Forms.ToolStripButton encryptionKeyToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem decryptAndRemoveFromListToolStripMenuItem;
    }
}

