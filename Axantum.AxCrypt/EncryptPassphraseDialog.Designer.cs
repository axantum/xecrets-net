namespace Axantum.AxCrypt
{
    partial class EncryptPassphraseDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EncryptPassphraseDialog));
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this.ShowPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this.VerifyPassphraseTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.PassphraseTextBox = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1.SuspendLayout();
            this.PassphraseGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOk);
            this.errorProvider1.SetError(this.panel1, resources.GetString("panel1.Error"));
            this.errorProvider1.SetIconAlignment(this.panel1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("panel1.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.panel1, ((int)(resources.GetObject("panel1.IconPadding"))));
            this.panel1.Name = "panel1";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.errorProvider1.SetError(this.buttonCancel, resources.GetString("buttonCancel.Error"));
            this.errorProvider1.SetIconAlignment(this.buttonCancel, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("buttonCancel.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.buttonCancel, ((int)(resources.GetObject("buttonCancel.IconPadding"))));
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            resources.ApplyResources(this.buttonOk, "buttonOk");
            this.buttonOk.CausesValidation = false;
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.errorProvider1.SetError(this.buttonOk, resources.GetString("buttonOk.Error"));
            this.errorProvider1.SetIconAlignment(this.buttonOk, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("buttonOk.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.buttonOk, ((int)(resources.GetObject("buttonOk.IconPadding"))));
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseGroupBox
            // 
            resources.ApplyResources(this.PassphraseGroupBox, "PassphraseGroupBox");
            this.PassphraseGroupBox.Controls.Add(this.ShowPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this.VerifyPassphraseTextbox);
            this.PassphraseGroupBox.Controls.Add(this.label1);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this.errorProvider1.SetError(this.PassphraseGroupBox, resources.GetString("PassphraseGroupBox.Error"));
            this.errorProvider1.SetIconAlignment(this.PassphraseGroupBox, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("PassphraseGroupBox.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.PassphraseGroupBox, ((int)(resources.GetObject("PassphraseGroupBox.IconPadding"))));
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.TabStop = false;
            // 
            // ShowPassphraseCheckBox
            // 
            resources.ApplyResources(this.ShowPassphraseCheckBox, "ShowPassphraseCheckBox");
            this.errorProvider1.SetError(this.ShowPassphraseCheckBox, resources.GetString("ShowPassphraseCheckBox.Error"));
            this.errorProvider1.SetIconAlignment(this.ShowPassphraseCheckBox, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("ShowPassphraseCheckBox.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.ShowPassphraseCheckBox, ((int)(resources.GetObject("ShowPassphraseCheckBox.IconPadding"))));
            this.ShowPassphraseCheckBox.Name = "ShowPassphraseCheckBox";
            this.ShowPassphraseCheckBox.UseVisualStyleBackColor = true;
            this.ShowPassphraseCheckBox.CheckedChanged += new System.EventHandler(this.ShowPassphraseCheckBox_CheckedChanged);
            // 
            // VerifyPassphraseTextbox
            // 
            resources.ApplyResources(this.VerifyPassphraseTextbox, "VerifyPassphraseTextbox");
            this.errorProvider1.SetError(this.VerifyPassphraseTextbox, resources.GetString("VerifyPassphraseTextbox.Error"));
            this.errorProvider1.SetIconAlignment(this.VerifyPassphraseTextbox, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("VerifyPassphraseTextbox.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.VerifyPassphraseTextbox, ((int)(resources.GetObject("VerifyPassphraseTextbox.IconPadding"))));
            this.VerifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this.VerifyPassphraseTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.VerifyPassphraseTextbox_Validating);
            this.VerifyPassphraseTextbox.Validated += new System.EventHandler(this.VerifyPassphraseTextbox_Validated);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.errorProvider1.SetError(this.label1, resources.GetString("label1.Error"));
            this.errorProvider1.SetIconAlignment(this.label1, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("label1.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.label1, ((int)(resources.GetObject("label1.IconPadding"))));
            this.label1.Name = "label1";
            // 
            // PassphraseTextBox
            // 
            resources.ApplyResources(this.PassphraseTextBox, "PassphraseTextBox");
            this.PassphraseTextBox.CausesValidation = false;
            this.errorProvider1.SetError(this.PassphraseTextBox, resources.GetString("PassphraseTextBox.Error"));
            this.errorProvider1.SetIconAlignment(this.PassphraseTextBox, ((System.Windows.Forms.ErrorIconAlignment)(resources.GetObject("PassphraseTextBox.IconAlignment"))));
            this.errorProvider1.SetIconPadding(this.PassphraseTextBox, ((int)(resources.GetObject("PassphraseTextBox.IconPadding"))));
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            resources.ApplyResources(this.errorProvider1, "errorProvider1");
            // 
            // EncryptPassphraseDialog
            // 
            this.AcceptButton = this.buttonOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.CausesValidation = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Name = "EncryptPassphraseDialog";
            this.Load += new System.EventHandler(this.EncryptPassphraseDialog_Load);
            this.panel1.ResumeLayout(false);
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.TextBox PassphraseTextBox;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox VerifyPassphraseTextbox;
        internal System.Windows.Forms.CheckBox ShowPassphraseCheckBox;
    }
}