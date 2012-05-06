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
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusTabs = new System.Windows.Forms.TabControl();
            this.OpenFilesTab = new System.Windows.Forms.TabPage();
            this.OpenFilesListView = new System.Windows.Forms.ListView();
            this.WatchedFoldersTab = new System.Windows.Forms.TabPage();
            this.WatchedFoldersListView = new System.Windows.Forms.ListView();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.menuStripMain.SuspendLayout();
            this.StatusTabs.SuspendLayout();
            this.OpenFilesTab.SuspendLayout();
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
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // StatusTabs
            // 
            this.StatusTabs.Controls.Add(this.OpenFilesTab);
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
            resources.ApplyResources(this.OpenFilesListView, "OpenFilesListView");
            this.OpenFilesListView.Name = "OpenFilesListView";
            this.OpenFilesListView.UseCompatibleStateImageBehavior = false;
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
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
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
    }
}

