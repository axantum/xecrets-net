namespace Axantum.AxCrypt
{
    partial class NewPassphraseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewPassphraseDialog));
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.VerifyPassphraseTextbox = new System.Windows.Forms.TextBox();
            this._label1 = new System.Windows.Forms.Label();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._groupBox1 = new System.Windows.Forms.GroupBox();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this._groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this.SuspendLayout();
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
            this.PassphraseGroupBox.Controls.Add(this.VerifyPassphraseTextbox);
            this.PassphraseGroupBox.Controls.Add(this._label1);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.TabStop = false;
            // 
            // ShowPassphraseCheckBox
            // 
            resources.ApplyResources(this.ShowPassphraseCheckBox, "ShowPassphraseCheckBox");
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // VerifyPassphraseTextbox
            // 
            resources.ApplyResources(this.VerifyPassphraseTextbox, "VerifyPassphraseTextbox");
            this.VerifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this.VerifyPassphraseTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.VerifyPassphraseTextbox_Validating);
            this.VerifyPassphraseTextbox.Validated += new System.EventHandler(this.VerifyPassphraseTextbox_Validated);
            // 
            // label1
            // 
            resources.ApplyResources(this._label1, "label1");
            this._label1.Name = "label1";
            // 
            // PassphraseTextBox
            // 
            this.PassphraseTextBox.CausesValidation = false;
            resources.ApplyResources(this.PassphraseTextBox, "PassphraseTextBox");
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            // 
            // errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // groupBox1
            // 
            this._groupBox1.Controls.Add(this.NameTextBox);
            resources.ApplyResources(this._groupBox1, "groupBox1");
            this._groupBox1.Name = "groupBox1";
            this._groupBox1.TabStop = false;
            // 
            // NameTextBox
            // 
            resources.ApplyResources(this.NameTextBox, "NameTextBox");
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.NameTextBox_Validating);
            this.NameTextBox.Validated += new System.EventHandler(this.NameTextBox_Validated);
            // 
            // errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // EncryptPassphraseDialog
            // 
            this.AcceptButton = this._buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.CausesValidation = false;
            this.Controls.Add(this._groupBox1);
            this.Controls.Add(this._panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Name = "EncryptPassphraseDialog";
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            this._panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this._groupBox1.ResumeLayout(false);
            this._groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
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
        private System.Windows.Forms.Label _label1;
        private System.Windows.Forms.TextBox VerifyPassphraseTextbox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
        private System.Windows.Forms.GroupBox _groupBox1;
        internal System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
    }
}