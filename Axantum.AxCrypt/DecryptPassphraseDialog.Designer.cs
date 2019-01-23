namespace Axantum.AxCrypt
{
    partial class DecryptPassphraseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DecryptPassphraseDialog));
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._fileNameLabel = new System.Windows.Forms.Label();
            this._panel1 = new System.Windows.Forms.Panel();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.Passphrase = new System.Windows.Forms.TextBox();
            this._panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(22, 11);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(108, 11);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(80, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _fileNameLabel
            // 
            this._fileNameLabel.AutoSize = true;
            this._fileNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this._fileNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._fileNameLabel.Location = new System.Drawing.Point(9, 9);
            this._fileNameLabel.Name = "_fileNameLabel";
            this._fileNameLabel.Size = new System.Drawing.Size(85, 13);
            this._fileNameLabel.TabIndex = 0;
            this._fileNameLabel.Text = "[fileName.ext]";
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(40, 121);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(200, 46);
            this._panel1.TabIndex = 2;
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.AutoSize = true;
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this.Passphrase);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 33);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(259, 87);
            this.PassphraseGroupBox.TabIndex = 1;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "[Enter Password]";
            // 
            // ShowPassphraseCheckBox
            // 
            this.ShowPassphraseCheckBox.AutoSize = true;
            this.ShowPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ShowPassphraseCheckBox.Location = new System.Drawing.Point(7, 51);
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.Size = new System.Drawing.Size(117, 17);
            this.ShowPassphraseCheckBox.TabIndex = 1;
            this.ShowPassphraseCheckBox.Text = "[Show Passphrase]";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            this.ShowPassphraseCheckBox.CheckedChanged += new System.EventHandler(this.ShowPassphraseCheckBox_CheckedChanged);
            // 
            // Passphrase
            // 
            this.Passphrase.Location = new System.Drawing.Point(7, 20);
            this.Passphrase.Name = "Passphrase";
            this.Passphrase.Size = new System.Drawing.Size(242, 20);
            this.Passphrase.TabIndex = 0;
            // 
            // DecryptPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(281, 175);
            this.Controls.Add(this._fileNameLabel);
            this.Controls.Add(this._panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Name = "DecryptPassphraseDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AxCrypt";
            this.Load += new System.EventHandler(this.DecryptPassphraseDialog_Load);
            this._panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.TextBox Passphrase;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        private System.Windows.Forms.Label _fileNameLabel;
    }
}