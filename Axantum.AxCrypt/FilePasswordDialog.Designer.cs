namespace Axantum.AxCrypt
{
    partial class FilePasswordDialog
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
            this._okButton = new System.Windows.Forms.Button();
            this.DialogFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.FileNamePanel = new System.Windows.Forms.Panel();
            this._fileNameGroupBox = new System.Windows.Forms.GroupBox();
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this.PasswordPanel = new System.Windows.Forms.Panel();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._passphraseTextBox = new System.Windows.Forms.TextBox();
            this._showPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.KeyFilePanel = new System.Windows.Forms.Panel();
            this._keyFileGroupBox = new System.Windows.Forms.GroupBox();
            this._keyFileTextBox = new System.Windows.Forms.TextBox();
            this._keyFileBrowseForButton = new System.Windows.Forms.Button();
            this.ButtonPanel = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.DialogFlowLayoutPanel.SuspendLayout();
            this.FileNamePanel.SuspendLayout();
            this._fileNameGroupBox.SuspendLayout();
            this.PasswordPanel.SuspendLayout();
            this._passphraseGroupBox.SuspendLayout();
            this.KeyFilePanel.SuspendLayout();
            this._keyFileGroupBox.SuspendLayout();
            this.ButtonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._okButton.Location = new System.Drawing.Point(74, 9);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "[OK]";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // DialogFlowLayoutPanel
            // 
            this.DialogFlowLayoutPanel.AutoSize = true;
            this.DialogFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DialogFlowLayoutPanel.Controls.Add(this.FileNamePanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.PasswordPanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.KeyFilePanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.ButtonPanel);
            this.DialogFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.DialogFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.DialogFlowLayoutPanel.Name = "DialogFlowLayoutPanel";
            this.DialogFlowLayoutPanel.Size = new System.Drawing.Size(319, 267);
            this.DialogFlowLayoutPanel.TabIndex = 0;
            // 
            // FileNamePanel
            // 
            this.FileNamePanel.Controls.Add(this._fileNameGroupBox);
            this.FileNamePanel.Location = new System.Drawing.Point(3, 3);
            this.FileNamePanel.Name = "FileNamePanel";
            this.FileNamePanel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            this.FileNamePanel.Size = new System.Drawing.Size(313, 59);
            this.FileNamePanel.TabIndex = 0;
            // 
            // _fileNameGroupBox
            // 
            this._fileNameGroupBox.AutoSize = true;
            this._fileNameGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._fileNameGroupBox.Controls.Add(this._fileNameTextBox);
            this._fileNameGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fileNameGroupBox.Location = new System.Drawing.Point(12, 12);
            this._fileNameGroupBox.Name = "_fileNameGroupBox";
            this._fileNameGroupBox.Size = new System.Drawing.Size(289, 47);
            this._fileNameGroupBox.TabIndex = 0;
            this._fileNameGroupBox.TabStop = false;
            this._fileNameGroupBox.Text = "[File]";
            // 
            // _fileNameTextBox
            // 
            this._fileNameTextBox.Enabled = false;
            this._fileNameTextBox.Location = new System.Drawing.Point(7, 19);
            this._fileNameTextBox.Name = "_fileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(270, 20);
            this._fileNameTextBox.TabIndex = 0;
            // 
            // PasswordPanel
            // 
            this.PasswordPanel.Controls.Add(this._passphraseGroupBox);
            this.PasswordPanel.Location = new System.Drawing.Point(3, 68);
            this.PasswordPanel.Name = "PasswordPanel";
            this.PasswordPanel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            this.PasswordPanel.Size = new System.Drawing.Size(313, 82);
            this.PasswordPanel.TabIndex = 1;
            // 
            // _passphraseGroupBox
            // 
            this._passphraseGroupBox.AutoSize = true;
            this._passphraseGroupBox.Controls.Add(this._passphraseTextBox);
            this._passphraseGroupBox.Controls.Add(this._showPassphraseCheckBox);
            this._passphraseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._passphraseGroupBox.Location = new System.Drawing.Point(12, 12);
            this._passphraseGroupBox.Name = "_passphraseGroupBox";
            this._passphraseGroupBox.Size = new System.Drawing.Size(289, 70);
            this._passphraseGroupBox.TabIndex = 0;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Password]";
            // 
            // _passphraseTextBox
            // 
            this._passphraseTextBox.Location = new System.Drawing.Point(7, 19);
            this._passphraseTextBox.Name = "_passphraseTextBox";
            this._passphraseTextBox.Size = new System.Drawing.Size(270, 20);
            this._passphraseTextBox.TabIndex = 0;
            this._passphraseTextBox.Enter += new System.EventHandler(this.PassphraseTextBox_Enter);
            // 
            // _showPassphraseCheckBox
            // 
            this._showPassphraseCheckBox.AutoSize = true;
            this._showPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphraseCheckBox.Location = new System.Drawing.Point(7, 46);
            this._showPassphraseCheckBox.Name = "_showPassphraseCheckBox";
            this._showPassphraseCheckBox.Size = new System.Drawing.Size(108, 17);
            this._showPassphraseCheckBox.TabIndex = 1;
            this._showPassphraseCheckBox.Text = "[Show Password]";
            this._showPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // KeyFilePanel
            // 
            this.KeyFilePanel.Controls.Add(this._keyFileGroupBox);
            this.KeyFilePanel.Location = new System.Drawing.Point(3, 156);
            this.KeyFilePanel.Name = "KeyFilePanel";
            this.KeyFilePanel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            this.KeyFilePanel.Size = new System.Drawing.Size(313, 62);
            this.KeyFilePanel.TabIndex = 2;
            // 
            // _keyFileGroupBox
            // 
            this._keyFileGroupBox.AutoSize = true;
            this._keyFileGroupBox.Controls.Add(this._keyFileTextBox);
            this._keyFileGroupBox.Controls.Add(this._keyFileBrowseForButton);
            this._keyFileGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._keyFileGroupBox.Location = new System.Drawing.Point(12, 12);
            this._keyFileGroupBox.Name = "_keyFileGroupBox";
            this._keyFileGroupBox.Size = new System.Drawing.Size(289, 50);
            this._keyFileGroupBox.TabIndex = 0;
            this._keyFileGroupBox.TabStop = false;
            this._keyFileGroupBox.Text = "[Key File]";
            // 
            // _keyFileTextBox
            // 
            this._keyFileTextBox.Location = new System.Drawing.Point(7, 20);
            this._keyFileTextBox.Name = "_keyFileTextBox";
            this._keyFileTextBox.Size = new System.Drawing.Size(234, 20);
            this._keyFileTextBox.TabIndex = 0;
            this._keyFileTextBox.Enter += new System.EventHandler(this.KeyFileTextBox_Enter);
            // 
            // _keyFileBrowseForButton
            // 
            this._keyFileBrowseForButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._keyFileBrowseForButton.Location = new System.Drawing.Point(247, 19);
            this._keyFileBrowseForButton.Name = "_keyFileBrowseForButton";
            this._keyFileBrowseForButton.Size = new System.Drawing.Size(30, 22);
            this._keyFileBrowseForButton.TabIndex = 1;
            this._keyFileBrowseForButton.Text = "...";
            this._keyFileBrowseForButton.UseVisualStyleBackColor = true;
            this._keyFileBrowseForButton.Click += new System.EventHandler(this.KeyFileBrowseForButton_Click);
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this._okButton);
            this.ButtonPanel.Controls.Add(this._cancelButton);
            this.ButtonPanel.Location = new System.Drawing.Point(3, 224);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Padding = new System.Windows.Forms.Padding(12, 12, 12, 0);
            this.ButtonPanel.Size = new System.Drawing.Size(313, 40);
            this.ButtonPanel.TabIndex = 3;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._cancelButton.Location = new System.Drawing.Point(164, 9);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "[Cancel]";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // FilePasswordDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(321, 273);
            this.Controls.Add(this.DialogFlowLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilePasswordDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[File Password]";
            this.Activated += new System.EventHandler(this.LogOnDialog_Activated);
            this.DialogFlowLayoutPanel.ResumeLayout(false);
            this.FileNamePanel.ResumeLayout(false);
            this.FileNamePanel.PerformLayout();
            this._fileNameGroupBox.ResumeLayout(false);
            this._fileNameGroupBox.PerformLayout();
            this.PasswordPanel.ResumeLayout(false);
            this.PasswordPanel.PerformLayout();
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
            this.KeyFilePanel.ResumeLayout(false);
            this.KeyFilePanel.PerformLayout();
            this._keyFileGroupBox.ResumeLayout(false);
            this._keyFileGroupBox.PerformLayout();
            this.ButtonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel PasswordPanel;
        private System.Windows.Forms.Panel FileNamePanel;
        private System.Windows.Forms.GroupBox _fileNameGroupBox;
        private System.Windows.Forms.FlowLayoutPanel DialogFlowLayoutPanel;
        private System.Windows.Forms.Panel ButtonPanel;
        private System.Windows.Forms.Panel KeyFilePanel;
        private System.Windows.Forms.GroupBox _keyFileGroupBox;
        private System.Windows.Forms.Button _keyFileBrowseForButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.GroupBox _passphraseGroupBox;
        private System.Windows.Forms.CheckBox _showPassphraseCheckBox;
        private System.Windows.Forms.TextBox _passphraseTextBox;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private System.Windows.Forms.TextBox _keyFileTextBox;
    }
}