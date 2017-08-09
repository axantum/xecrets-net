namespace Axantum.AxCrypt.Forms
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
            this._fileNameTextBox = new System.Windows.Forms.TextBox();
            this._fileNamePanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonHelp = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._passwordStrengthMeter = new Axantum.AxCrypt.Forms.PasswordStrengthMeter();
            this._showPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this._verifyPassphraseTextbox = new System.Windows.Forms.TextBox();
            this._verifyPasswordLabel = new System.Windows.Forms.Label();
            this._passphraseTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this._fileGroupBox.SuspendLayout();
            this._fileNamePanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this._panel1.SuspendLayout();
            this._passphraseGroupBox.SuspendLayout();
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
            this._fileGroupBox.Controls.Add(this._fileNameTextBox);
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
            this._fileNameTextBox.Enabled = false;
            this._fileNameTextBox.Location = new System.Drawing.Point(9, 18);
            this._fileNameTextBox.Name = "FileNameTextBox";
            this._fileNameTextBox.Size = new System.Drawing.Size(242, 20);
            this._fileNameTextBox.TabIndex = 0;
            // 
            // FileNamePanel
            // 
            this._fileNamePanel.AutoSize = true;
            this._fileNamePanel.Controls.Add(this._fileGroupBox);
            this._fileNamePanel.Dock = System.Windows.Forms.DockStyle.Top;
            this._fileNamePanel.Location = new System.Drawing.Point(0, 0);
            this._fileNamePanel.Name = "FileNamePanel";
            this._fileNamePanel.Size = new System.Drawing.Size(315, 59);
            this._fileNamePanel.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this._panel1);
            this.panel1.Controls.Add(this._passphraseGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 59);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(315, 216);
            this.panel1.TabIndex = 1;
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._buttonHelp);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(12, 161);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(280, 37);
            this._panel1.TabIndex = 1;
            // 
            // _buttonHelp
            // 
            this._buttonHelp.CausesValidation = false;
            this._buttonHelp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonHelp.Location = new System.Drawing.Point(168, 11);
            this._buttonHelp.Name = "_buttonHelp";
            this._buttonHelp.Size = new System.Drawing.Size(75, 23);
            this._buttonHelp.TabIndex = 2;
            this._buttonHelp.Text = "[Help]";
            this._buttonHelp.UseVisualStyleBackColor = true;
            this._buttonHelp.Click += new System.EventHandler(this._buttonHelp_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(87, 11);
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
            this._buttonOk.Location = new System.Drawing.Point(6, 11);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseGroupBox
            // 
            this._passphraseGroupBox.AutoSize = true;
            this._passphraseGroupBox.Controls.Add(this._passwordStrengthMeter);
            this._passphraseGroupBox.Controls.Add(this._showPassphraseCheckBox);
            this._passphraseGroupBox.Controls.Add(this._verifyPassphraseTextbox);
            this._passphraseGroupBox.Controls.Add(this._verifyPasswordLabel);
            this._passphraseGroupBox.Controls.Add(this._passphraseTextBox);
            this._passphraseGroupBox.Location = new System.Drawing.Point(12, 11);
            this._passphraseGroupBox.Name = "PassphraseGroupBox";
            this._passphraseGroupBox.Size = new System.Drawing.Size(280, 147);
            this._passphraseGroupBox.TabIndex = 0;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Password]";
            // 
            // _passwordStrengthMeter
            // 
            this._passwordStrengthMeter.Location = new System.Drawing.Point(6, 44);
            this._passwordStrengthMeter.Name = "_passwordStrengthMeter";
            this._passwordStrengthMeter.Size = new System.Drawing.Size(243, 18);
            this._passwordStrengthMeter.TabIndex = 4;
            // 
            // ShowPassphraseCheckBox
            // 
            this._showPassphraseCheckBox.AutoSize = true;
            this._showPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphraseCheckBox.Location = new System.Drawing.Point(7, 111);
            this._showPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this._showPassphraseCheckBox.Size = new System.Drawing.Size(108, 17);
            this._showPassphraseCheckBox.TabIndex = 3;
            this._showPassphraseCheckBox.Text = "[Show Password]";
            this._showPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // VerifyPassphraseTextbox
            // 
            this._verifyPassphraseTextbox.Location = new System.Drawing.Point(6, 81);
            this._verifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this._verifyPassphraseTextbox.Size = new System.Drawing.Size(243, 20);
            this._verifyPassphraseTextbox.TabIndex = 2;
            // 
            // _verifyPasswordLabel
            // 
            this._verifyPasswordLabel.AutoSize = true;
            this._verifyPasswordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._verifyPasswordLabel.Location = new System.Drawing.Point(6, 65);
            this._verifyPasswordLabel.Name = "_verifyPasswordLabel";
            this._verifyPasswordLabel.Size = new System.Drawing.Size(88, 13);
            this._verifyPasswordLabel.TabIndex = 1;
            this._verifyPasswordLabel.Text = "[Verify Password]";
            // 
            // PassphraseTextBox
            // 
            this._passphraseTextBox.CausesValidation = false;
            this._passphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this._passphraseTextBox.Name = "PassphraseTextBox";
            this._passphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this._passphraseTextBox.TabIndex = 0;
            // 
            // NewPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(315, 275);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this._fileNamePanel);
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
            this._fileNamePanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this._panel1.ResumeLayout(false);
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
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
        private System.Windows.Forms.GroupBox _passphraseGroupBox;
        private System.Windows.Forms.TextBox _verifyPassphraseTextbox;
        private System.Windows.Forms.Label _verifyPasswordLabel;
        private System.Windows.Forms.Panel _fileNamePanel;
        private System.Windows.Forms.GroupBox _fileGroupBox;
        private System.Windows.Forms.TextBox _fileNameTextBox;
        private PasswordStrengthMeter _passwordStrengthMeter;
        private System.Windows.Forms.Button _buttonHelp;
        private System.Windows.Forms.CheckBox _showPassphraseCheckBox;
        private System.Windows.Forms.TextBox _passphraseTextBox;
    }
}