namespace Axantum.AxCrypt
{
    partial class ImportPrivatePasswordDialog
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
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._passphraseTextBox = new System.Windows.Forms.TextBox();
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.Controls.Add(this._showPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this._panel1);
            this.PassphraseGroupBox.Controls.Add(this._passphraseTextBox);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 12);
            this.PassphraseGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 13);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(298, 131);
            this.PassphraseGroupBox.TabIndex = 7;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "Enter Passphrase";
            // 
            // _showPassphraseCheckBox
            // 
            this._showPassphraseCheckBox.AutoSize = true;
            this._showPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphraseCheckBox.Location = new System.Drawing.Point(7, 53);
            this._showPassphraseCheckBox.Name = "_showPassphraseCheckBox";
            this._showPassphraseCheckBox.Size = new System.Drawing.Size(111, 17);
            this._showPassphraseCheckBox.TabIndex = 8;
            this._showPassphraseCheckBox.Text = "Show Passphrase";
            this._showPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _panel1
            // 
            this._panel1.AutoSize = true;
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(7, 85);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(285, 37);
            this._panel1.TabIndex = 7;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(98, 7);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 9;
            this._buttonCancel.Text = "Cancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(17, 7);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 8;
            this._buttonOk.Text = "OK";
            this._buttonOk.UseVisualStyleBackColor = true;
            // 
            // _passphraseTextBox
            // 
            this._passphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this._passphraseTextBox.Name = "_passphraseTextBox";
            this._passphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this._passphraseTextBox.TabIndex = 0;
            // 
            // ImportPrivatePasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(321, 156);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Name = "ImportPrivatePasswordDialog";
            this.Text = "Enter Passphrase";
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox _showPassphraseCheckBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.TextBox _passphraseTextBox;
    }
}