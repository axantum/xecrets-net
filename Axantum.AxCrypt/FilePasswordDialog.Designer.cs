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
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._newButton = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this.FileNamePanel = new System.Windows.Forms.Panel();
            this._fileGroupBox = new System.Windows.Forms.GroupBox();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this.FileNamePanel.SuspendLayout();
            this._fileGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.PassphraseGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 59);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(376, 161);
            this.panel1.TabIndex = 1;
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this._panel1);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 6);
            this.PassphraseGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 13);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(298, 131);
            this.PassphraseGroupBox.TabIndex = 0;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "[Enter Password]";
            // 
            // ShowPassphraseCheckBox
            // 
            this.ShowPassphraseCheckBox.AutoSize = true;
            this.ShowPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ShowPassphraseCheckBox.Location = new System.Drawing.Point(7, 53);
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.Size = new System.Drawing.Size(108, 17);
            this.ShowPassphraseCheckBox.TabIndex = 1;
            this.ShowPassphraseCheckBox.Text = "[Show Password]";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _panel1
            // 
            this._panel1.AutoSize = true;
            this._panel1.Controls.Add(this._newButton);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(7, 85);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(273, 37);
            this._panel1.TabIndex = 2;
            // 
            // _newButton
            // 
            this._newButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this._newButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._newButton.Location = new System.Drawing.Point(180, 7);
            this._newButton.Name = "_newButton";
            this._newButton.Size = new System.Drawing.Size(75, 23);
            this._newButton.TabIndex = 2;
            this._newButton.Text = "[&New]";
            this._newButton.UseVisualStyleBackColor = true;
            this._newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(98, 7);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(17, 7);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseTextBox
            // 
            this.PassphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this.PassphraseTextBox.TabIndex = 0;
            this.PassphraseTextBox.Enter += new System.EventHandler(this.PassphraseTextBox_Enter);
            // 
            // FileNamePanel
            // 
            this.FileNamePanel.AutoSize = true;
            this.FileNamePanel.Controls.Add(this._fileGroupBox);
            this.FileNamePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FileNamePanel.Location = new System.Drawing.Point(0, 0);
            this.FileNamePanel.Name = "FileNamePanel";
            this.FileNamePanel.Size = new System.Drawing.Size(376, 59);
            this.FileNamePanel.TabIndex = 0;
            // 
            // groupBox1
            // 
            this._fileGroupBox.Controls.Add(this.FileNameTextBox);
            this._fileGroupBox.Location = new System.Drawing.Point(12, 12);
            this._fileGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._fileGroupBox.Name = "groupBox1";
            this._fileGroupBox.Size = new System.Drawing.Size(298, 44);
            this._fileGroupBox.TabIndex = 0;
            this._fileGroupBox.TabStop = false;
            this._fileGroupBox.Text = "[File]";
            // 
            // FileNameTextBox
            // 
            this.FileNameTextBox.Enabled = false;
            this.FileNameTextBox.Location = new System.Drawing.Point(9, 18);
            this.FileNameTextBox.Name = "FileNameTextBox";
            this.FileNameTextBox.Size = new System.Drawing.Size(242, 20);
            this.FileNameTextBox.TabIndex = 0;
            // 
            // FilePasswordDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(376, 220);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.FileNamePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilePasswordDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[File Password]";
            this.Activated += new System.EventHandler(this.LogOnDialog_Activated);
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.FileNamePanel.ResumeLayout(false);
            this._fileGroupBox.ResumeLayout(false);
            this._fileGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _newButton;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Panel FileNamePanel;
        private System.Windows.Forms.GroupBox _fileGroupBox;
        internal System.Windows.Forms.TextBox FileNameTextBox;
    }
}