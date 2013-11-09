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
            // panel1
            // 
            resources.ApplyResources(this._panel1, "panel1");
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Name = "panel1";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this._buttonCancel, "buttonCancel");
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.Name = "buttonCancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            resources.ApplyResources(this._buttonOk, "buttonOk");
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.Name = "buttonOk";
            this._buttonOk.UseVisualStyleBackColor = true;
            // 
            // DecryptPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
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
    }
}