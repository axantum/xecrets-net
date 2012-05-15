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
            this.helpToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openEncryptedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusTabs = new System.Windows.Forms.TabControl();
            this.OpenFilesTab = new System.Windows.Forms.TabPage();
            this.OpenFilesListView = new System.Windows.Forms.ListView();
            this.OpenFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.EncryptedSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ImageList48 = new System.Windows.Forms.ImageList(this.components);
            this.ImageList16 = new System.Windows.Forms.ImageList(this.components);
            this.RecentFilesTab = new System.Windows.Forms.TabPage();
            this.RecentFilesListView = new System.Windows.Forms.ListView();
            this.WatchedFoldersTab = new System.Windows.Forms.TabPage();
            this.WatchedFoldersListView = new System.Windows.Forms.ListView();
            this.ActiveFilePolling = new System.Windows.Forms.Timer(this.components);
            this.EncryptedPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LastAccessTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DecryptedFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.StatusTabs.SuspendLayout();
            this.OpenFilesTab.SuspendLayout();
            this.RecentFilesTab.SuspendLayout();
            this.WatchedFoldersTab.SuspendLayout();
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
            this.helpToolStripButton});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripButtonOpenEncrypted
            // 
            this.toolStripButtonOpenEncrypted.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonOpenEncrypted, "toolStripButtonOpenEncrypted");
            this.toolStripButtonOpenEncrypted.Name = "toolStripButtonOpenEncrypted";
            this.toolStripButtonOpenEncrypted.Click += new System.EventHandler(this.toolStripButtonOpenEncrypted_Click);
            // 
            // toolStripButtonEncrypt
            // 
            this.toolStripButtonEncrypt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonEncrypt, "toolStripButtonEncrypt");
            this.toolStripButtonEncrypt.Name = "toolStripButtonEncrypt";
            this.toolStripButtonEncrypt.Click += new System.EventHandler(this.toolStripButtonEncrypt_Click);
            // 
            // toolStripButtonDecrypt
            // 
            this.toolStripButtonDecrypt.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButtonDecrypt, "toolStripButtonDecrypt");
            this.toolStripButtonDecrypt.Name = "toolStripButtonDecrypt";
            this.toolStripButtonDecrypt.Click += new System.EventHandler(this.toolStripButtonDecrypt_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // helpToolStripButton
            // 
            this.helpToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.helpToolStripButton, "helpToolStripButton");
            this.helpToolStripButton.Name = "helpToolStripButton";
            // 
            // menuStripMain
            // 
            this.menuStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStripMain, "menuStripMain");
            this.menuStripMain.Name = "menuStripMain";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openEncryptedToolStripMenuItem,
            this.encryptToolStripMenuItem,
            this.decryptToolStripMenuItem,
            this.toolStripSeparator2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // openEncryptedToolStripMenuItem
            // 
            this.openEncryptedToolStripMenuItem.Name = "openEncryptedToolStripMenuItem";
            resources.ApplyResources(this.openEncryptedToolStripMenuItem, "openEncryptedToolStripMenuItem");
            this.openEncryptedToolStripMenuItem.Click += new System.EventHandler(this.openEncryptedToolStripMenuItem_Click);
            // 
            // encryptToolStripMenuItem
            // 
            this.encryptToolStripMenuItem.Name = "encryptToolStripMenuItem";
            resources.ApplyResources(this.encryptToolStripMenuItem, "encryptToolStripMenuItem");
            // 
            // decryptToolStripMenuItem
            // 
            this.decryptToolStripMenuItem.Name = "decryptToolStripMenuItem";
            resources.ApplyResources(this.decryptToolStripMenuItem, "decryptToolStripMenuItem");
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // StatusTabs
            // 
            this.StatusTabs.Controls.Add(this.OpenFilesTab);
            this.StatusTabs.Controls.Add(this.RecentFilesTab);
            this.StatusTabs.Controls.Add(this.WatchedFoldersTab);
            resources.ApplyResources(this.StatusTabs, "StatusTabs");
            this.StatusTabs.Name = "StatusTabs";
            this.StatusTabs.SelectedIndex = 0;
            // 
            // OpenFilesTab
            // 
            this.OpenFilesTab.Controls.Add(this.OpenFilesListView);
            resources.ApplyResources(this.OpenFilesTab, "OpenFilesTab");
            this.OpenFilesTab.Name = "OpenFilesTab";
            this.OpenFilesTab.UseVisualStyleBackColor = true;
            // 
            // OpenFilesListView
            // 
            this.OpenFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OpenFile,
            this.EncryptedSource});
            resources.ApplyResources(this.OpenFilesListView, "OpenFilesListView");
            this.OpenFilesListView.LargeImageList = this.ImageList48;
            this.OpenFilesListView.Name = "OpenFilesListView";
            this.OpenFilesListView.SmallImageList = this.ImageList16;
            this.OpenFilesListView.UseCompatibleStateImageBehavior = false;
            this.OpenFilesListView.View = System.Windows.Forms.View.Details;
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
            // 
            // ImageList16
            // 
            this.ImageList16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList16.ImageStream")));
            this.ImageList16.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList16.Images.SetKeyName(0, "ActiveFile");
            this.ImageList16.Images.SetKeyName(1, "InactiveFile");
            // 
            // RecentFilesTab
            // 
            this.RecentFilesTab.Controls.Add(this.RecentFilesListView);
            resources.ApplyResources(this.RecentFilesTab, "RecentFilesTab");
            this.RecentFilesTab.Name = "RecentFilesTab";
            this.RecentFilesTab.UseVisualStyleBackColor = true;
            // 
            // RecentFilesListView
            // 
            this.RecentFilesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.DecryptedFile,
            this.EncryptedPath,
            this.LastAccessTime});
            resources.ApplyResources(this.RecentFilesListView, "RecentFilesListView");
            this.RecentFilesListView.LargeImageList = this.ImageList48;
            this.RecentFilesListView.Name = "RecentFilesListView";
            this.RecentFilesListView.SmallImageList = this.ImageList16;
            this.RecentFilesListView.UseCompatibleStateImageBehavior = false;
            this.RecentFilesListView.View = System.Windows.Forms.View.Details;
            this.RecentFilesListView.SelectedIndexChanged += new System.EventHandler(this.RecentFilesListView_SelectedIndexChanged);
            // 
            // WatchedFoldersTab
            // 
            this.WatchedFoldersTab.Controls.Add(this.WatchedFoldersListView);
            resources.ApplyResources(this.WatchedFoldersTab, "WatchedFoldersTab");
            this.WatchedFoldersTab.Name = "WatchedFoldersTab";
            this.WatchedFoldersTab.UseVisualStyleBackColor = true;
            // 
            // WatchedFoldersListView
            // 
            resources.ApplyResources(this.WatchedFoldersListView, "WatchedFoldersListView");
            this.WatchedFoldersListView.Name = "WatchedFoldersListView";
            this.WatchedFoldersListView.UseCompatibleStateImageBehavior = false;
            // 
            // ActiveFilePolling
            // 
            this.ActiveFilePolling.Enabled = true;
            this.ActiveFilePolling.Interval = 1000;
            this.ActiveFilePolling.Tick += new System.EventHandler(this.ActiveFilePolling_Tick);
            // 
            // EncryptedPath
            // 
            resources.ApplyResources(this.EncryptedPath, "EncryptedPath");
            // 
            // LastAccessTime
            // 
            resources.ApplyResources(this.LastAccessTime, "LastAccessTime");
            // 
            // DecryptedFile
            // 
            resources.ApplyResources(this.DecryptedFile, "DecryptedFile");
            // 
            // AxCryptMainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.StatusTabs);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.Name = "AxCryptMainForm";
            this.Load += new System.EventHandler(this.FormAxCryptMain_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.StatusTabs.ResumeLayout(false);
            this.OpenFilesTab.ResumeLayout(false);
            this.RecentFilesTab.ResumeLayout(false);
            this.WatchedFoldersTab.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton helpToolStripButton;
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
    }
}

