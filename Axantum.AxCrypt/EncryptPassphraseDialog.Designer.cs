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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
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
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonOk);
            this.panel1.Location = new System.Drawing.Point(52, 116);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 37);
            this.panel1.TabIndex = 5;
            // 
            // buttonCancel
            // 
            this.buttonCancel.CausesValidation = false;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(103, 11);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.CausesValidation = false;
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOk.Location = new System.Drawing.Point(22, 11);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 8;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.AutoSize = true;
            this.PassphraseGroupBox.Controls.Add(this.VerifyPassphraseTextbox);
            this.PassphraseGroupBox.Controls.Add(this.label1);
            this.PassphraseGroupBox.Controls.Add(this.PassphraseTextBox);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 12);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(280, 98);
            this.PassphraseGroupBox.TabIndex = 4;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "Enter Passphrase";
            // 
            // VerifyPassphraseTextbox
            // 
            this.VerifyPassphraseTextbox.Location = new System.Drawing.Point(6, 59);
            this.VerifyPassphraseTextbox.Name = "VerifyPassphraseTextbox";
            this.VerifyPassphraseTextbox.Size = new System.Drawing.Size(243, 20);
            this.VerifyPassphraseTextbox.TabIndex = 7;
            this.VerifyPassphraseTextbox.Validating += new System.ComponentModel.CancelEventHandler(this.VerifyPassphraseTextbox_Validating);
            this.VerifyPassphraseTextbox.Validated += new System.EventHandler(this.VerifyPassphraseTextbox_Validated);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(6, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Verify Passphrase";
            // 
            // PassphraseTextBox
            // 
            this.PassphraseTextBox.CausesValidation = false;
            this.PassphraseTextBox.Location = new System.Drawing.Point(7, 20);
            this.PassphraseTextBox.Name = "PassphraseTextBox";
            this.PassphraseTextBox.Size = new System.Drawing.Size(242, 20);
            this.PassphraseTextBox.TabIndex = 0;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // EncryptPassphraseDialog
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.CancelButton = this.buttonCancel;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(305, 165);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.PassphraseGroupBox);
            this.Name = "EncryptPassphraseDialog";
            this.Text = "EncryptPassphraseDialog";
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
    }
}