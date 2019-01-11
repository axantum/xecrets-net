namespace Axantum.AxCrypt
{
    partial class VerifyAccountDialog
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
            this._toolTipActivation = new System.Windows.Forms.ToolTip(this.components);
            this._activationCode = new System.Windows.Forms.TextBox();
            this._resendButtonToolTip = new System.Windows.Forms.ToolTip(this.components);
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._activationCodeGroupBox = new System.Windows.Forms.GroupBox();
            this._checkEmailLabel = new System.Windows.Forms.Label();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphrase = new System.Windows.Forms.CheckBox();
            this._passphraseVerification = new System.Windows.Forms.TextBox();
            this._verifyPasswordLabel = new System.Windows.Forms.Label();
            this._passphrase = new System.Windows.Forms.TextBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._helpButton = new System.Windows.Forms.Button();
            this._resendButton = new System.Windows.Forms.Button();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider4 = new System.Windows.Forms.ErrorProvider(this.components);
            this._promptTextLabel = new System.Windows.Forms.Label();
            this._passwordStrengthMeter = new Axantum.AxCrypt.Forms.PasswordStrengthMeter();
            this._activationCodeGroupBox.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider4)).BeginInit();
            this.SuspendLayout();
            // 
            // _toolTipActivation
            // 
            this._toolTipActivation.AutoPopDelay = 10000;
            this._toolTipActivation.InitialDelay = 100;
            this._toolTipActivation.ReshowDelay = 100;
            // 
            // _activationCode
            // 
            this._activationCode.Location = new System.Drawing.Point(9, 18);
            this._activationCode.Name = "_activationCode";
            this._activationCode.Size = new System.Drawing.Size(88, 20);
            this._activationCode.TabIndex = 0;
            this._toolTipActivation.SetToolTip(this._activationCode, "We\'ve sent a 6-digit code to your email inbox.\r\n\r\nUnfortunately it might be misin" +
        "terpreted as spam,\r\nso search there if it\'s not in the inbox.\r\n\r\nThe subject sho" +
        "uld contain the word \'AxCrypt\'.");
            // 
            // _buttonOk
            // 
            this._buttonOk.CausesValidation = false;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(5, 9);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(91, 9);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(80, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _activationCodeGroupBox
            // 
            this._activationCodeGroupBox.Controls.Add(this._checkEmailLabel);
            this._activationCodeGroupBox.Controls.Add(this._activationCode);
            this._activationCodeGroupBox.Location = new System.Drawing.Point(12, 62);
            this._activationCodeGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._activationCodeGroupBox.Name = "_activationCodeGroupBox";
            this._activationCodeGroupBox.Size = new System.Drawing.Size(334, 44);
            this._activationCodeGroupBox.TabIndex = 5;
            this._activationCodeGroupBox.TabStop = false;
            this._activationCodeGroupBox.Text = "[Activation Code]";
            // 
            // _checkEmailLabel
            // 
            this._checkEmailLabel.AutoSize = true;
            this._checkEmailLabel.Location = new System.Drawing.Point(109, 21);
            this._checkEmailLabel.Name = "_checkEmailLabel";
            this._checkEmailLabel.Size = new System.Drawing.Size(123, 13);
            this._checkEmailLabel.TabIndex = 1;
            this._checkEmailLabel.Text = "[Check email and spam!]";
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.AutoSize = true;
            this.PassphraseGroupBox.Controls.Add(this._passwordStrengthMeter);
            this.PassphraseGroupBox.Controls.Add(this._showPassphrase);
            this.PassphraseGroupBox.Controls.Add(this._passphraseVerification);
            this.PassphraseGroupBox.Controls.Add(this._verifyPasswordLabel);
            this.PassphraseGroupBox.Controls.Add(this._passphrase);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 112);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(334, 153);
            this.PassphraseGroupBox.TabIndex = 5;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "[Set Your Password]";
            // 
            // _showPassphrase
            // 
            this._showPassphrase.AutoSize = true;
            this._showPassphrase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphrase.Location = new System.Drawing.Point(7, 117);
            this._showPassphrase.Name = "_showPassphrase";
            this._showPassphrase.Size = new System.Drawing.Size(108, 17);
            this._showPassphrase.TabIndex = 3;
            this._showPassphrase.Text = "[Show Password]";
            this._showPassphrase.UseVisualStyleBackColor = true;
            // 
            // _passphraseVerification
            // 
            this._passphraseVerification.Location = new System.Drawing.Point(6, 87);
            this._passphraseVerification.Name = "_passphraseVerification";
            this._passphraseVerification.Size = new System.Drawing.Size(243, 20);
            this._passphraseVerification.TabIndex = 2;
            // 
            // _verifyPasswordLabel
            // 
            this._verifyPasswordLabel.AutoSize = true;
            this._verifyPasswordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._verifyPasswordLabel.Location = new System.Drawing.Point(6, 71);
            this._verifyPasswordLabel.Name = "_verifyPasswordLabel";
            this._verifyPasswordLabel.Size = new System.Drawing.Size(88, 13);
            this._verifyPasswordLabel.TabIndex = 1;
            this._verifyPasswordLabel.Text = "[Verify Password]";
            // 
            // _passphrase
            // 
            this._passphrase.CausesValidation = false;
            this._passphrase.Location = new System.Drawing.Point(7, 20);
            this._passphrase.Name = "_passphrase";
            this._passphrase.Size = new System.Drawing.Size(242, 20);
            this._passphrase.TabIndex = 0;
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._helpButton);
            this._panel1.Controls.Add(this._resendButton);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(2, 267);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(350, 41);
            this._panel1.TabIndex = 3;
            // 
            // _helpButton
            // 
            this._helpButton.CausesValidation = false;
            this._helpButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._helpButton.Location = new System.Drawing.Point(263, 9);
            this._helpButton.Name = "_helpButton";
            this._helpButton.Size = new System.Drawing.Size(80, 23);
            this._helpButton.TabIndex = 3;
            this._helpButton.Text = "[Help]";
            this._helpButton.UseVisualStyleBackColor = true;
            this._helpButton.Click += new System.EventHandler(this._helpButton_Click);
            // 
            // _resendButton
            // 
            this._resendButton.CausesValidation = false;
            this._resendButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._resendButton.Location = new System.Drawing.Point(177, 9);
            this._resendButton.Name = "_resendButton";
            this._resendButton.Size = new System.Drawing.Size(80, 23);
            this._resendButton.TabIndex = 2;
            this._resendButton.Text = "[Resend]";
            this._resendButton.UseVisualStyleBackColor = true;
            this._resendButton.Click += new System.EventHandler(this.ResendButton_Click);
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
            // _errorProvider4
            // 
            this._errorProvider4.ContainerControl = this;
            // 
            // _promptTextLabel
            // 
            this._promptTextLabel.Location = new System.Drawing.Point(12, 9);
            this._promptTextLabel.Name = "_promptTextLabel";
            this._promptTextLabel.Size = new System.Drawing.Size(334, 46);
            this._promptTextLabel.TabIndex = 6;
            this._promptTextLabel.Text = "[You have now signed up...]";
            // 
            // _passwordStrengthMeter
            // 
            this._passwordStrengthMeter.Location = new System.Drawing.Point(6, 46);
            this._passwordStrengthMeter.Name = "_passwordStrengthMeter";
            this._passwordStrengthMeter.Size = new System.Drawing.Size(243, 18);
            this._passwordStrengthMeter.TabIndex = 6;
            // 
            // VerifyAccountDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(354, 315);
            this.Controls.Add(this._promptTextLabel);
            this.Controls.Add(this._activationCodeGroupBox);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Controls.Add(this._panel1);
            this.Name = "VerifyAccountDialog";
            this.Text = "[Activate Account]";
            this.Load += new System.EventHandler(this.VerifyAccountDialog_Load);
            this._activationCodeGroupBox.ResumeLayout(false);
            this._activationCodeGroupBox.PerformLayout();
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        private System.Windows.Forms.TextBox _passphraseVerification;
        private System.Windows.Forms.Label _verifyPasswordLabel;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.GroupBox _activationCodeGroupBox;
        private System.Windows.Forms.Label _checkEmailLabel;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.ErrorProvider _errorProvider3;
        private System.Windows.Forms.ErrorProvider _errorProvider4;
        private System.Windows.Forms.TextBox _passphrase;
        private System.Windows.Forms.CheckBox _showPassphrase;
        private System.Windows.Forms.TextBox _activationCode;
        private System.Windows.Forms.ToolTip _toolTipActivation;
        private Axantum.AxCrypt.Forms.PasswordStrengthMeter _passwordStrengthMeter;
        private System.Windows.Forms.Button _resendButton;
        private System.Windows.Forms.ToolTip _resendButtonToolTip;
        private System.Windows.Forms.Button _helpButton;
        private System.Windows.Forms.Label _promptTextLabel;
    }
}