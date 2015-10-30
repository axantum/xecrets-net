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
            this._passphrase = new System.Windows.Forms.TextBox();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphrase = new System.Windows.Forms.CheckBox();
            this._passphraseVerification = new System.Windows.Forms.TextBox();
            this._label1 = new System.Windows.Forms.Label();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._email = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this._activationCode = new System.Windows.Forms.TextBox();
            this._errorProvider4 = new System.Windows.Forms.ErrorProvider(this.components);
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider4)).BeginInit();
            this.SuspendLayout();
            // 
            // _passphrase
            // 
            this._passphrase.CausesValidation = false;
            this._passphrase.Location = new System.Drawing.Point(7, 20);
            this._passphrase.Name = "_passphrase";
            this._passphrase.Size = new System.Drawing.Size(242, 20);
            this._passphrase.TabIndex = 0;
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.AutoSize = true;
            this.PassphraseGroupBox.Controls.Add(this._showPassphrase);
            this.PassphraseGroupBox.Controls.Add(this._passphraseVerification);
            this.PassphraseGroupBox.Controls.Add(this._label1);
            this.PassphraseGroupBox.Controls.Add(this._passphrase);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 108);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(280, 125);
            this.PassphraseGroupBox.TabIndex = 5;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "Set Your Password";
            // 
            // _showPassphrase
            // 
            this._showPassphrase.AutoSize = true;
            this._showPassphrase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphrase.Location = new System.Drawing.Point(7, 89);
            this._showPassphrase.Name = "_showPassphrase";
            this._showPassphrase.Size = new System.Drawing.Size(102, 17);
            this._showPassphrase.TabIndex = 3;
            this._showPassphrase.Text = "Show Password";
            this._showPassphrase.UseVisualStyleBackColor = true;
            // 
            // _passphraseVerification
            // 
            this._passphraseVerification.Location = new System.Drawing.Point(6, 59);
            this._passphraseVerification.Name = "_passphraseVerification";
            this._passphraseVerification.Size = new System.Drawing.Size(243, 20);
            this._passphraseVerification.TabIndex = 2;
            // 
            // _label1
            // 
            this._label1.AutoSize = true;
            this._label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._label1.Location = new System.Drawing.Point(6, 43);
            this._label1.Name = "_label1";
            this._label1.Size = new System.Drawing.Size(82, 13);
            this._label1.TabIndex = 1;
            this._label1.Text = "Verify Password";
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(52, 242);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(200, 37);
            this._panel1.TabIndex = 3;
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
            this._buttonCancel.Text = "Cancel";
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
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // _email
            // 
            this._email.Enabled = false;
            this._email.Location = new System.Drawing.Point(9, 18);
            this._email.Name = "_email";
            this._email.Size = new System.Drawing.Size(242, 20);
            this._email.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this._email);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(280, 44);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Email";
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this._activationCode);
            this.groupBox2.Location = new System.Drawing.Point(12, 62);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(280, 44);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Activation Code";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(130, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Check your email inbox!";
            // 
            // _activationCode
            // 
            this._activationCode.Location = new System.Drawing.Point(9, 18);
            this._activationCode.Name = "_activationCode";
            this._activationCode.Size = new System.Drawing.Size(88, 20);
            this._activationCode.TabIndex = 0;
            // 
            // _errorProvider4
            // 
            this._errorProvider4.ContainerControl = this;
            // 
            // VerifyAccountDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(304, 290);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Controls.Add(this._panel1);
            this.Controls.Add(this.groupBox1);
            this.Name = "VerifyAccountDialog";
            this.Text = "Activate Account";
            this.Load += new System.EventHandler(this.VerifyAccountDialog_Load);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider4)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        private System.Windows.Forms.TextBox _passphraseVerification;
        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        private System.Windows.Forms.ErrorProvider _errorProvider3;
        private System.Windows.Forms.ErrorProvider _errorProvider4;
        private System.Windows.Forms.TextBox _passphrase;
        private System.Windows.Forms.CheckBox _showPassphrase;
        private System.Windows.Forms.TextBox _email;
        private System.Windows.Forms.TextBox _activationCode;
    }
}