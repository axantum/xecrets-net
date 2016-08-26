namespace Axantum.AxCrypt
{
    partial class CreateNewAccountDialog
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
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._passwordStrengthMeter = new Axantum.AxCrypt.PasswordStrengthMeter();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.VerifyPassphraseTextbox = new System.Windows.Forms.TextBox();
            this._verifyPasswordLabel = new System.Windows.Forms.Label();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.EmailTextBox = new System.Windows.Forms.TextBox();
            this._emailGroupBox = new System.Windows.Forms.GroupBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            this._helpButton = new System.Windows.Forms.Button();
            this._passphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this._emailGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).BeginInit();
            this.SuspendLayout();
            // 
            // PassphraseTextBox
            // 
            this.PassphraseTextBox.CausesValidation = false;
            this.PassphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this.PassphraseTextBox.TabIndex = 0;
            // 
            // _passphraseGroupBox
            // 
            this._passphraseGroupBox.AutoSize = true;
            this._passphraseGroupBox.Controls.Add(this._passwordStrengthMeter);
            this._passphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this._passphraseGroupBox.Controls.Add(this.VerifyPassphraseTextbox);
            this._passphraseGroupBox.Controls.Add(this._verifyPasswordLabel);
            this._passphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this._passphraseGroupBox.Location = new System.Drawing.Point(2, 47);
            this._passphraseGroupBox.Name = "_passphraseGroupBox";
            this._passphraseGroupBox.Size = new System.Drawing.Size(280, 143);
            this._passphraseGroupBox.TabIndex = 2;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Password]";
            // 
            // _passwordStrengthMeter
            // 
            this._passwordStrengthMeter.Location = new System.Drawing.Point(7, 46);
            this._passwordStrengthMeter.Name = "_passwordStrengthMeter";
            this._passwordStrengthMeter.Size = new System.Drawing.Size(243, 10);
            this._passwordStrengthMeter.TabIndex = 5;
            // 
            // ShowPassphraseCheckBox
            // 
            this.ShowPassphraseCheckBox.AutoSize = true;
            this.ShowPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ShowPassphraseCheckBox.Location = new System.Drawing.Point(7, 107);
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.Size = new System.Drawing.Size(108, 17);
            this.ShowPassphraseCheckBox.TabIndex = 3;
            this.ShowPassphraseCheckBox.Text = "[Show Password]";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // VerifyPassphraseTextbox
            // 
            this.VerifyPassphraseTextbox.Location = new System.Drawing.Point(6, 77);
            this.VerifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this.VerifyPassphraseTextbox.Size = new System.Drawing.Size(243, 20);
            this.VerifyPassphraseTextbox.TabIndex = 2;
            // 
            // _verifyPasswordLabel
            // 
            this._verifyPasswordLabel.AutoSize = true;
            this._verifyPasswordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._verifyPasswordLabel.Location = new System.Drawing.Point(6, 61);
            this._verifyPasswordLabel.Name = "_verifyPasswordLabel";
            this._verifyPasswordLabel.Size = new System.Drawing.Size(88, 13);
            this._verifyPasswordLabel.TabIndex = 1;
            this._verifyPasswordLabel.Text = "[Verify Password]";
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._helpButton);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(2, 196);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(251, 37);
            this._panel1.TabIndex = 0;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(87, 6);
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
            this._buttonOk.Location = new System.Drawing.Point(6, 6);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // EmailTextBox
            // 
            this.EmailTextBox.Location = new System.Drawing.Point(9, 18);
            this.EmailTextBox.Name = "EmailTextBox";
            this.EmailTextBox.Size = new System.Drawing.Size(242, 20);
            this.EmailTextBox.TabIndex = 0;
            // 
            // _emailGroupBox
            // 
            this._emailGroupBox.Controls.Add(this.EmailTextBox);
            this._emailGroupBox.Location = new System.Drawing.Point(2, 2);
            this._emailGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._emailGroupBox.Name = "_emailGroupBox";
            this._emailGroupBox.Size = new System.Drawing.Size(280, 44);
            this._emailGroupBox.TabIndex = 1;
            this._emailGroupBox.TabStop = false;
            this._emailGroupBox.Text = "[Email]";
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // _errorProvider3
            // 
            this._errorProvider3.ContainerControl = this;
            // 
            // _helpButton
            // 
            this._helpButton.CausesValidation = false;
            this._helpButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._helpButton.Location = new System.Drawing.Point(168, 6);
            this._helpButton.Name = "_helpButton";
            this._helpButton.Size = new System.Drawing.Size(75, 23);
            this._helpButton.TabIndex = 2;
            this._helpButton.Text = "[Help]";
            this._helpButton.UseVisualStyleBackColor = true;
            this._helpButton.Click += new System.EventHandler(this._helpButton_Click);
            // 
            // CreateNewAccountDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 237);
            this.Controls.Add(this._emailGroupBox);
            this.Controls.Add(this._passphraseGroupBox);
            this.Controls.Add(this._panel1);
            this.Name = "CreateNewAccountDialog";
            this.Text = "[Create Account]";
            this.Load += new System.EventHandler(this.CreateNewAccountDialog_Load);
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this._emailGroupBox.ResumeLayout(false);
            this._emailGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.GroupBox _passphraseGroupBox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        private System.Windows.Forms.TextBox VerifyPassphraseTextbox;
        private System.Windows.Forms.Label _verifyPasswordLabel;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.TextBox EmailTextBox;
        private System.Windows.Forms.GroupBox _emailGroupBox;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.ErrorProvider _errorProvider3;
        private PasswordStrengthMeter _passwordStrengthMeter;
        private System.Windows.Forms.Button _helpButton;
    }
}