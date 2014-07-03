namespace Axantum.AxCrypt
{
    partial class WatchedFoldersDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WatchedFoldersDialog));
            this._watchedFoldersListView = new System.Windows.Forms.ListView();
            this._watchedFolderColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._watchedFoldersContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._watchedFoldersRemoveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersdecryptTemporarilyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersOpenExplorerHereMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._watchedFoldersContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // _watchedFoldersListView
            // 
            this._watchedFoldersListView.AllowDrop = true;
            this._watchedFoldersListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._watchedFolderColumnHeader});
            this._watchedFoldersListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._watchedFoldersListView.Location = new System.Drawing.Point(0, 0);
            this._watchedFoldersListView.Name = "_watchedFoldersListView";
            this._watchedFoldersListView.Size = new System.Drawing.Size(658, 160);
            this._watchedFoldersListView.TabIndex = 0;
            this._watchedFoldersListView.UseCompatibleStateImageBehavior = false;
            this._watchedFoldersListView.View = System.Windows.Forms.View.Details;
            // 
            // _watchedFolderColumnHeader
            // 
            this._watchedFolderColumnHeader.Text = "Folder";
            this._watchedFolderColumnHeader.Width = 325;
            // 
            // _watchedFoldersContextMenuStrip
            // 
            this._watchedFoldersContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._watchedFoldersRemoveMenuItem,
            this._watchedFoldersdecryptTemporarilyMenuItem,
            this._watchedFoldersOpenExplorerHereMenuItem});
            this._watchedFoldersContextMenuStrip.Name = "watchedFoldersContextMenuStrip";
            this._watchedFoldersContextMenuStrip.Size = new System.Drawing.Size(186, 92);
            // 
            // _watchedFoldersRemoveMenuItem
            // 
            this._watchedFoldersRemoveMenuItem.Name = "_watchedFoldersRemoveMenuItem";
            this._watchedFoldersRemoveMenuItem.Size = new System.Drawing.Size(185, 22);
            this._watchedFoldersRemoveMenuItem.Text = "&Decrypt Permanently";
            // 
            // _watchedFoldersdecryptTemporarilyMenuItem
            // 
            this._watchedFoldersdecryptTemporarilyMenuItem.Name = "_watchedFoldersdecryptTemporarilyMenuItem";
            this._watchedFoldersdecryptTemporarilyMenuItem.Size = new System.Drawing.Size(185, 22);
            this._watchedFoldersdecryptTemporarilyMenuItem.Text = "Decrypt &Temporarily";
            // 
            // _watchedFoldersOpenExplorerHereMenuItem
            // 
            this._watchedFoldersOpenExplorerHereMenuItem.Name = "_watchedFoldersOpenExplorerHereMenuItem";
            this._watchedFoldersOpenExplorerHereMenuItem.Size = new System.Drawing.Size(185, 22);
            this._watchedFoldersOpenExplorerHereMenuItem.Text = "Open &Explorer Here";
            // 
            // WatchedFoldersDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(658, 160);
            this.Controls.Add(this._watchedFoldersListView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WatchedFoldersDialog";
            this.Text = "Encrypted Folders";
            this.Load += new System.EventHandler(this.WatchedFoldersDialog_Load);
            this._watchedFoldersContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView _watchedFoldersListView;
        private System.Windows.Forms.ColumnHeader _watchedFolderColumnHeader;
        private System.Windows.Forms.ContextMenuStrip _watchedFoldersContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersRemoveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersdecryptTemporarilyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _watchedFoldersOpenExplorerHereMenuItem;
    }
}