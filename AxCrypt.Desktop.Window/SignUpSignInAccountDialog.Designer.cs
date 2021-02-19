using System;
using System.ComponentModel;

namespace AxCrypt.Desktop.Window
{
    partial class SignUpSignInAccountDialog
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
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonSwitchUser = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._userEmailGroupBox = new System.Windows.Forms.GroupBox();
            this.UserEmailTextBox = new System.Windows.Forms.TextBox();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphrase = new System.Windows.Forms.CheckBox();
            this._passphrase = new System.Windows.Forms.TextBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._troubleRememberingPanel = new System.Windows.Forms.Panel();
            this._troubleRememberingLabel = new System.Windows.Forms.LinkLabel();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._createAccountLinkLabel = new System.Windows.Forms.LinkLabel();
            this._languageSelectionTextLabel = new System.Windows.Forms.Label();
            this._languageCultureDropDown = new System.Windows.Forms.ComboBox();
            this._languagePanel = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this._passphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this._languagePanel.SuspendLayout();
            this._troubleRememberingPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this.SuspendLayout();
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(130, 11);
            this._buttonOk.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(117, 35);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(268, 11);
            this._buttonCancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(117, 35);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            //
            // _buttonSwitchUser
            // 
            this._buttonSwitchUser.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this._buttonSwitchUser.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonSwitchUser.Location = new System.Drawing.Point(375, 24);
            this._buttonSwitchUser.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._buttonSwitchUser.Name = "_buttonSwitchUser";
            this._buttonSwitchUser.Size = new System.Drawing.Size(125, 35);
            this._buttonSwitchUser.TabIndex = 2;
            this._buttonSwitchUser.Text = "[Switch user]";
            this._buttonSwitchUser.UseVisualStyleBackColor = true;
            this._buttonSwitchUser.Click += new System.EventHandler(this.ButtonSwitchUser_Click);
            this._buttonSwitchUser.Visible = false;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(529, 302);
            this.panel1.TabIndex = 1;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._userEmailGroupBox);
            this.flowLayoutPanel1.Controls.Add(this._passphraseGroupBox);
            this.flowLayoutPanel1.Controls.Add(this._languagePanel);
            this.flowLayoutPanel1.Controls.Add(this._panel1);
            this.flowLayoutPanel1.Controls.Add(this._createAccountLinkLabel);
            this.flowLayoutPanel1.Controls.Add(this._troubleRememberingPanel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(529, 400);
            this.flowLayoutPanel1.TabIndex = 5;
            // 
            // _userEmailGroupBox
            // 
            this._userEmailGroupBox.Controls.Add(this.UserEmailTextBox);
            this._userEmailGroupBox.Controls.Add(this._buttonSwitchUser);
            this._userEmailGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._userEmailGroupBox.Location = new System.Drawing.Point(4, 5);
            this._userEmailGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 5, 5);
            this._userEmailGroupBox.MinimumSize = new System.Drawing.Size(514, 0);
            this._userEmailGroupBox.Name = "_userEmailGroupBox";
            this._userEmailGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 30, 5);
            this._userEmailGroupBox.Size = new System.Drawing.Size(514, 80);
            this._userEmailGroupBox.TabIndex = 0;
            this._userEmailGroupBox.TabStop = false;
            this._userEmailGroupBox.Text = "[Email address]";
            // 
            // _userEmail
            // 
            this.UserEmailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
| System.Windows.Forms.AnchorStyles.Right)));
            this.UserEmailTextBox.AcceptsReturn = true;
            this.UserEmailTextBox.Location = new System.Drawing.Point(4, 28);
            this.UserEmailTextBox.Margin = new System.Windows.Forms.Padding(100, 5, 100, 5);
            this.UserEmailTextBox.MinimumSize = new System.Drawing.Size(480, 26);
            this.UserEmailTextBox.Name = "EmailTextBox";
            this.UserEmailTextBox.Size = new System.Drawing.Size(480, 26);
            this.UserEmailTextBox.TabIndex = 0;
            // 
            // _passphraseGroupBox
            // 
            this._passphraseGroupBox.Controls.Add(this._passphrase);
            this._passphraseGroupBox.Controls.Add(this._showPassphrase);
            this._passphraseGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._passphraseGroupBox.Location = new System.Drawing.Point(4, 29);
            this._passphraseGroupBox.Margin = new System.Windows.Forms.Padding(4, 5, 5, 5);
            this._passphraseGroupBox.MinimumSize = new System.Drawing.Size(514, 0);
            this._passphraseGroupBox.Name = "_passphraseGroupBox";
            this._passphraseGroupBox.Padding = new System.Windows.Forms.Padding(4, 5, 30, 5);
            this._passphraseGroupBox.Size = new System.Drawing.Size(514, 108);
            this._passphraseGroupBox.TabIndex = 1;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Password]";
            // 
            // _passphrase
            // 
            this._passphrase.AcceptsReturn = true;
            this._passphrase.Dock = System.Windows.Forms.DockStyle.Fill;
            this._passphrase.Location = new System.Drawing.Point(4, 53);
            this._passphrase.Margin = new System.Windows.Forms.Padding(100, 5, 100, 5);
            this._passphrase.MinimumSize = new System.Drawing.Size(480, 26);
            this._passphrase.Name = "_passphrase";
            this._passphrase.Size = new System.Drawing.Size(480, 26);
            this._passphrase.TabIndex = 1;
            this._passphrase.Enter += new System.EventHandler(this.PassphraseTextBox_Enter);
            // 
            // _showPassphrase
            // 
            this._showPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._showPassphrase.AutoSize = true;
            this._showPassphrase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphrase.Location = new System.Drawing.Point(9, 73);
            this._showPassphrase.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._showPassphrase.Name = "_showPassphrase";
            this._showPassphrase.Size = new System.Drawing.Size(156, 24);
            this._showPassphrase.TabIndex = 2;
            this._showPassphrase.Text = "[Show Password]";
            this._showPassphrase.UseVisualStyleBackColor = true;
            // 
            // _panel1
            // 
            this._panel1.AutoSize = true;
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panel1.Location = new System.Drawing.Point(4, 110);
            this._panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(515, 51);
            this._panel1.TabIndex = 3;
            // 
            // _troubleRememberingPanel
            // 
            this._troubleRememberingPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._troubleRememberingPanel.BackColor = System.Drawing.SystemColors.Info;
            this._troubleRememberingPanel.Controls.Add(this._troubleRememberingLabel);
            this._troubleRememberingPanel.Location = new System.Drawing.Point(3, 227);
            this._troubleRememberingPanel.Name = "_troubleRememberingPanel";
            this._troubleRememberingPanel.Size = new System.Drawing.Size(517, 40);
            this._troubleRememberingPanel.TabIndex = 6;
            // 
            // _troubleRememberingLabel
            // 
            this._troubleRememberingLabel.AutoSize = true;
            this._troubleRememberingLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._troubleRememberingLabel.LinkColor = System.Drawing.Color.Black;
            this._troubleRememberingLabel.Location = new System.Drawing.Point(113, 11);
            this._troubleRememberingLabel.Name = "_troubleRememberingLabel";
            this._troubleRememberingLabel.Size = new System.Drawing.Size(318, 20);
            this._troubleRememberingLabel.TabIndex = 0;
            this._troubleRememberingLabel.TabStop = true;
            this._troubleRememberingLabel.Text = "[Trouble remembering your password?]";
            this._troubleRememberingLabel.VisitedLinkColor = System.Drawing.Color.Black;
            this._troubleRememberingLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.troubleRememberingLabel_LinkClicked);
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // _createAccountLinkLabel
            // 
            this._createAccountLinkLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._createAccountLinkLabel.Location = new System.Drawing.Point(4, 166);
            this._createAccountLinkLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._createAccountLinkLabel.Name = "_createAccountLinkLabel";
            this._createAccountLinkLabel.Size = new System.Drawing.Size(515, 38);
            this._createAccountLinkLabel.TabIndex = 4;
            this._createAccountLinkLabel.TabStop = true;
            this._createAccountLinkLabel.Text = "[New user? Sign up]";
            this._createAccountLinkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this._createAccountLinkLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._createAccountLinkLabel.LinkColor = AxCrypt.Forms.Style.Styling.WarningColor;
            this._createAccountLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.createNewAccountLabel_LinkClicked);
            // 
            // _languageSelectionTextLabel
            // 
            this._languageSelectionTextLabel.Location = new System.Drawing.Point(4, 9);
            this._languageSelectionTextLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._languageSelectionTextLabel.Name = "_languageSelectionTextLabel";
            this._languageSelectionTextLabel.Size = new System.Drawing.Size(240, 24);
            this._languageSelectionTextLabel.TabIndex = 0;
            this._languageSelectionTextLabel.Text = "[Choose your Language]";
            // 
            // _languageCultureDropDown
            // 
            this._languageCultureDropDown.DisplayMember = "Key";
            this._languageCultureDropDown.Dock = System.Windows.Forms.DockStyle.Right;
            this._languageCultureDropDown.FormattingEnabled = true;
            this._languageCultureDropDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._languageCultureDropDown.Location = new System.Drawing.Point(356, 9);
            this._languageCultureDropDown.Margin = new System.Windows.Forms.Padding(0, 6, 6, 6);
            this._languageCultureDropDown.Name = "_languageCultureDropDown";
            this._languageCultureDropDown.Size = new System.Drawing.Size(250, 24);
            this._languageCultureDropDown.TabIndex = 1;
            this._languageCultureDropDown.ValueMember = "Value";
            this._languageCultureDropDown.SelectedIndexChanged += new System.EventHandler(this.LanguageCultureDropDown_SelectedIndexChanged);
            // 
            // _languagePanel
            // 
            this._languagePanel.AutoSize = true;
            this._languagePanel.Controls.Add(this._languageSelectionTextLabel);
            this._languagePanel.Controls.Add(this._languageCultureDropDown);
            this._languagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._languagePanel.Location = new System.Drawing.Point(4, 249);
            this._languagePanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._languagePanel.Padding = new System.Windows.Forms.Padding(0, 8, 10, 0);
            this._languagePanel.Name = "_languagePanel";
            this._languagePanel.Size = new System.Drawing.Size(457, 32);
            this._languagePanel.TabIndex = 3;
            this._languagePanel.Visible = false;
            // 
            // LogOnAccountDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.HelpButton = true;
            this.ClientSize = new System.Drawing.Size(541, 400);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(541, 375);
            this.Name = "LogOnAccountDialog";
            this.Text = "[Account Sign In]";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.SignUpSignInDialog_HelpButtonClicked);
            this.Activated += new System.EventHandler(this.SignUpSignInAccountDialog_Activated);
            this.Load += new System.EventHandler(this.SignUpSignInAccountDialog_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this._languagePanel.ResumeLayout(false);
            this._languagePanel.PerformLayout();
            this._troubleRememberingPanel.ResumeLayout(false);
            this._troubleRememberingPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox _passphraseGroupBox;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.GroupBox _userEmailGroupBox;
        private System.Windows.Forms.CheckBox _showPassphrase;
        private System.Windows.Forms.TextBox _passphrase;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonSwitchUser;
        private System.Windows.Forms.LinkLabel _createAccountLinkLabel;
        private System.Windows.Forms.Label _languageSelectionTextLabel;
        private System.Windows.Forms.ComboBox _languageCultureDropDown;
        private System.Windows.Forms.Panel _languagePanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel _troubleRememberingPanel;
        private System.Windows.Forms.LinkLabel _troubleRememberingLabel;
        internal System.Windows.Forms.TextBox UserEmailTextBox;
    }
}