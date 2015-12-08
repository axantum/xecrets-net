namespace Axantum.AxCrypt
{
    partial class LogOnAccountDialog
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
            this.panel1 = new System.Windows.Forms.Panel();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphrase = new System.Windows.Forms.CheckBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._newButton = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._passphrase = new System.Windows.Forms.TextBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.EmailPanel = new System.Windows.Forms.Panel();
            this._emailGroupBox = new System.Windows.Forms.GroupBox();
            this._email = new System.Windows.Forms.TextBox();
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1.SuspendLayout();
            this._passphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.EmailPanel.SuspendLayout();
            this._emailGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this._passphraseGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 59);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(337, 154);
            this.panel1.TabIndex = 1;
            // 
            // PassphraseGroupBox
            // 
            this._passphraseGroupBox.Controls.Add(this._showPassphrase);
            this._passphraseGroupBox.Controls.Add(this._panel1);
            this._passphraseGroupBox.Controls.Add(this._passphrase);
            this._passphraseGroupBox.Location = new System.Drawing.Point(12, 6);
            this._passphraseGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 13);
            this._passphraseGroupBox.Name = "PassphraseGroupBox";
            this._passphraseGroupBox.Size = new System.Drawing.Size(298, 131);
            this._passphraseGroupBox.TabIndex = 0;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Password]";
            // 
            // _showPassphrase
            // 
            this._showPassphrase.AutoSize = true;
            this._showPassphrase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphrase.Location = new System.Drawing.Point(7, 53);
            this._showPassphrase.Name = "_showPassphrase";
            this._showPassphrase.Size = new System.Drawing.Size(108, 17);
            this._showPassphrase.TabIndex = 1;
            this._showPassphrase.Text = "[Show Password]";
            this._showPassphrase.UseVisualStyleBackColor = true;
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
            this._newButton.Click += new System.EventHandler(this.NewButton_Click);
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
            this._buttonCancel.Click += new System.EventHandler(this._buttonCancel_Click);
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
            this._buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // _passphrase
            // 
            this._passphrase.AcceptsReturn = true;
            this._passphrase.Location = new System.Drawing.Point(7, 20);
            this._passphrase.Name = "_passphrase";
            this._passphrase.Size = new System.Drawing.Size(242, 20);
            this._passphrase.TabIndex = 0;
            this._passphrase.Enter += new System.EventHandler(this.PassphraseTextBox_Enter);
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // EmailPanel
            // 
            this.EmailPanel.AutoSize = true;
            this.EmailPanel.Controls.Add(this._emailGroupBox);
            this.EmailPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EmailPanel.Location = new System.Drawing.Point(0, 0);
            this.EmailPanel.Name = "EmailPanel";
            this.EmailPanel.Size = new System.Drawing.Size(337, 59);
            this.EmailPanel.TabIndex = 0;
            // 
            // groupBox1
            // 
            this._emailGroupBox.Controls.Add(this._email);
            this._emailGroupBox.Location = new System.Drawing.Point(12, 12);
            this._emailGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._emailGroupBox.Name = "groupBox1";
            this._emailGroupBox.Size = new System.Drawing.Size(298, 44);
            this._emailGroupBox.TabIndex = 0;
            this._emailGroupBox.TabStop = false;
            this._emailGroupBox.Text = "[Email address]";
            // 
            // _email
            // 
            this._email.Enabled = false;
            this._email.Location = new System.Drawing.Point(9, 18);
            this._email.Name = "_email";
            this._email.Size = new System.Drawing.Size(242, 20);
            this._email.TabIndex = 0;
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // LogOnAccountDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(337, 213);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.EmailPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "LogOnAccountDialog";
            this.Text = "[Account Sign In]";
            this.Activated += new System.EventHandler(this.LogOnAccountDialog_Activated);
            this.Load += new System.EventHandler(this.LogOnAccountDialog_Load);
            this.ResizeEnd += new System.EventHandler(this.LogOnAccountDialog_ResizeAndMoveEnd);
            this.panel1.ResumeLayout(false);
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.EmailPanel.ResumeLayout(false);
            this._emailGroupBox.ResumeLayout(false);
            this._emailGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox _passphraseGroupBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _newButton;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel EmailPanel;
        private System.Windows.Forms.GroupBox _emailGroupBox;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.TextBox _email;
        private System.Windows.Forms.CheckBox _showPassphrase;
        private System.Windows.Forms.TextBox _passphrase;
    }
}