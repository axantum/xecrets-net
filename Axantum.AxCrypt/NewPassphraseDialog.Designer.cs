namespace Axantum.AxCrypt
{
    partial class NewPassphraseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewPassphraseDialog));
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._fileGroupBox = new System.Windows.Forms.GroupBox();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.FileNamePanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.VerifyPassphraseTextbox = new System.Windows.Forms.TextBox();
            this._verifyPasswordLabel = new System.Windows.Forms.Label();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this._passwordStrengthMeter = new Axantum.AxCrypt.PasswordStrengthMeter();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this._fileGroupBox.SuspendLayout();
            this.FileNamePanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this._panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // _fileGroupBox
            // 
            this._fileGroupBox.Controls.Add(this.FileNameTextBox);
            this._fileGroupBox.Location = new System.Drawing.Point(12, 12);
            this._fileGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._fileGroupBox.Name = "_fileGroupBox";
            this._fileGroupBox.Size = new System.Drawing.Size(280, 44);
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
            // FileNamePanel
            // 
            this.FileNamePanel.AutoSize = true;
            this.FileNamePanel.Controls.Add(this._fileGroupBox);
            this.FileNamePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.FileNamePanel.Location = new System.Drawing.Point(0, 0);
            this.FileNamePanel.Name = "FileNamePanel";
            this.FileNamePanel.Size = new System.Drawing.Size(315, 59);
            this.FileNamePanel.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this._panel1);
            this.panel1.Controls.Add(this.PassphraseGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 59);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(315, 208);
            this.panel1.TabIndex = 1;
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(52, 153);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(200, 37);
            this._panel1.TabIndex = 1;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(103, 11);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.CausesValidation = false;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(22, 11);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.AutoSize = true;
            this.PassphraseGroupBox.Controls.Add(this._passwordStrengthMeter);
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this.VerifyPassphraseTextbox);
            this.PassphraseGroupBox.Controls.Add(this._verifyPasswordLabel);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 11);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(280, 139);
            this.PassphraseGroupBox.TabIndex = 0;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "[Enter Password]";
            // 
            // ShowPassphraseCheckBox
            // 
            this.ShowPassphraseCheckBox.AutoSize = true;
            this.ShowPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ShowPassphraseCheckBox.Location = new System.Drawing.Point(7, 103);
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.Size = new System.Drawing.Size(108, 17);
            this.ShowPassphraseCheckBox.TabIndex = 3;
            this.ShowPassphraseCheckBox.Text = "[Show Password]";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // VerifyPassphraseTextbox
            // 
            this.VerifyPassphraseTextbox.Location = new System.Drawing.Point(6, 73);
            this.VerifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this.VerifyPassphraseTextbox.Size = new System.Drawing.Size(243, 20);
            this.VerifyPassphraseTextbox.TabIndex = 2;
            // 
            // _verifyPasswordLabel
            // 
            this._verifyPasswordLabel.AutoSize = true;
            this._verifyPasswordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._verifyPasswordLabel.Location = new System.Drawing.Point(6, 57);
            this._verifyPasswordLabel.Name = "_verifyPasswordLabel";
            this._verifyPasswordLabel.Size = new System.Drawing.Size(88, 13);
            this._verifyPasswordLabel.TabIndex = 1;
            this._verifyPasswordLabel.Text = "[Verify Password]";
            // 
            // PassphraseTextBox
            // 
            this.PassphraseTextBox.CausesValidation = false;
            this.PassphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this.PassphraseTextBox.TabIndex = 0;
            // 
            // _passwordStrengthMeter
            // 
            this._passwordStrengthMeter.Location = new System.Drawing.Point(6, 44);
            this._passwordStrengthMeter.Name = "_passwordStrengthMeter";
            this._passwordStrengthMeter.Size = new System.Drawing.Size(243, 10);
            this._passwordStrengthMeter.TabIndex = 4;
            // 
            // NewPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(315, 267);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.FileNamePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewPassphraseDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[Create New Passphrase]";
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            this._fileGroupBox.ResumeLayout(false);
            this._fileGroupBox.PerformLayout();
            this.FileNamePanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        private System.Windows.Forms.TextBox VerifyPassphraseTextbox;
        private System.Windows.Forms.Label _verifyPasswordLabel;
        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.Panel FileNamePanel;
        private System.Windows.Forms.GroupBox _fileGroupBox;
        internal System.Windows.Forms.TextBox FileNameTextBox;
        private PasswordStrengthMeter _passwordStrengthMeter;
    }
}