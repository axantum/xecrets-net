namespace Axantum.AxCrypt
{
    partial class LogOnDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogOnDialog));
            this._panel1 = new System.Windows.Forms.Panel();
            this._newButton = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this._panel1, "panel1");
            this._panel1.Controls.Add(this._newButton);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Name = "panel1";
            // 
            // newButton
            // 
            this._newButton.CausesValidation = false;
            this._newButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            resources.ApplyResources(this._newButton, "newButton");
            this._newButton.Name = "newButton";
            this._newButton.UseVisualStyleBackColor = true;
            this._newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._buttonCancel, "buttonCancel");
            this._buttonCancel.Name = "buttonCancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this._buttonOk.CausesValidation = false;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._buttonOk, "buttonOk");
            this._buttonOk.Name = "buttonOk";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseGroupBox
            // 
            resources.ApplyResources(this.PassphraseGroupBox, "PassphraseGroupBox");
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
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
            // PassphraseTextBox
            // 
            resources.ApplyResources(this.PassphraseTextBox, "PassphraseTextBox");
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.PassphraseTextBox_Validating);
            this.PassphraseTextBox.Validated += new System.EventHandler(this.PassphraseTextBox_Validated);
            // 
            // errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // LogOnDialog
            // 
            this.AcceptButton = this._buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.CausesValidation = false;
            this.Controls.Add(this._panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LogOnDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            this._panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        private System.Windows.Forms.Button _newButton;
    }
}