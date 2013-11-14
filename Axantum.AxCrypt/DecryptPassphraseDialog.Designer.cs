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
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.Passphrase = new System.Windows.Forms.TextBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._fileNameLabel = new System.Windows.Forms.Label();
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PassphraseGroupBox
            // 
            resources.ApplyResources(this.PassphraseGroupBox, "PassphraseGroupBox");
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this.Passphrase);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.TabStop = false;
            // 
            // ShowPassphraseCheckBox
            // 
            resources.ApplyResources(this.ShowPassphraseCheckBox, "ShowPassphraseCheckBox");
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            this.ShowPassphraseCheckBox.CheckedChanged += new System.EventHandler(this.ShowPassphraseCheckBox_CheckedChanged);
            // 
            // Passphrase
            // 
            resources.ApplyResources(this.Passphrase, "Passphrase");
            this.Passphrase.Name = "Passphrase";
            // 
            // _panel1
            // 
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            resources.ApplyResources(this._panel1, "_panel1");
            this._panel1.Name = "_panel1";
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._buttonCancel, "_buttonCancel");
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._buttonOk, "_buttonOk");
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.UseVisualStyleBackColor = true;
            // 
            // _fileNameLabel
            // 
            resources.ApplyResources(this._fileNameLabel, "_fileNameLabel");
            this._fileNameLabel.Name = "_fileNameLabel";
            this._fileNameLabel.Click += new System.EventHandler(this._fileNameLabel_Click);
            // 
            // DecryptPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.Controls.Add(this._fileNameLabel);
            this.Controls.Add(this._panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Name = "DecryptPassphraseDialog";
            this.Load += new System.EventHandler(this.DecryptPassphraseDialog_Load);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
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