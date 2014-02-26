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
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._newButton = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.FileNamePanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this.FileNamePanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.PassphraseGroupBox);
            this.panel1.Name = "panel1";
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this._panel1);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            resources.ApplyResources(this.PassphraseGroupBox, "PassphraseGroupBox");
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.TabStop = false;
            // 
            // ShowPassphraseCheckBox
            // 
            resources.ApplyResources(this.ShowPassphraseCheckBox, "ShowPassphraseCheckBox");
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // PassphraseTextBox
            // 
            resources.ApplyResources(this.PassphraseTextBox, "PassphraseTextBox");
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.PassphraseTextBox_Validating);
            this.PassphraseTextBox.Validated += new System.EventHandler(this.PassphraseTextBox_Validated);
            // 
            // _panel1
            // 
            resources.ApplyResources(this._panel1, "_panel1");
            this._panel1.Controls.Add(this._newButton);
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Name = "_panel1";
            // 
            // _newButton
            // 
            this._newButton.CausesValidation = false;
            this._newButton.DialogResult = System.Windows.Forms.DialogResult.Retry;
            resources.ApplyResources(this._newButton, "_newButton");
            this._newButton.Name = "_newButton";
            this._newButton.UseVisualStyleBackColor = true;
            this._newButton.Click += new System.EventHandler(this.newButton_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.CausesValidation = false;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._buttonCancel, "_buttonCancel");
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.CausesValidation = false;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._buttonOk, "_buttonOk");
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // FileNamePanel
            // 
            resources.ApplyResources(this.FileNamePanel, "FileNamePanel");
            this.FileNamePanel.Controls.Add(this.groupBox1);
            this.FileNamePanel.Name = "FileNamePanel";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.FileNameTextBox);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // FileNameTextBox
            // 
            resources.ApplyResources(this.FileNameTextBox, "FileNameTextBox");
            this.FileNameTextBox.Name = "FileNameTextBox";
            // 
            // LogOnDialog
            // 
            this.AcceptButton = this._buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.FileNamePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "LogOnDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Activated += new System.EventHandler(this.LogOnDialog_Activated);
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.FileNamePanel.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _newButton;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Panel FileNamePanel;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.TextBox FileNameTextBox;
    }
}