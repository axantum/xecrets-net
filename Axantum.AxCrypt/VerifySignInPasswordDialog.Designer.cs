namespace Axantum.AxCrypt
{
    partial class VerifySignInPasswordDialog
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
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._verifyInstructionLabel = new System.Windows.Forms.Label();
            this._passphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._passphraseTextBox = new System.Windows.Forms.TextBox();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this._passphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // _buttonOk
            // 
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(8, 7);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this.ButtonOk_Click);
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(89, 7);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(75, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            this._buttonCancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this._verifyInstructionLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._passphraseGroupBox, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(310, 163);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _verifyInstructionLabel
            // 
            this._verifyInstructionLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._verifyInstructionLabel.AutoSize = true;
            this._verifyInstructionLabel.Location = new System.Drawing.Point(30, 15);
            this._verifyInstructionLabel.Margin = new System.Windows.Forms.Padding(5);
            this._verifyInstructionLabel.MaximumSize = new System.Drawing.Size(250, 0);
            this._verifyInstructionLabel.MinimumSize = new System.Drawing.Size(250, 0);
            this._verifyInstructionLabel.Name = "_verifyInstructionLabel";
            this._verifyInstructionLabel.Size = new System.Drawing.Size(250, 13);
            this._verifyInstructionLabel.TabIndex = 0;
            this._verifyInstructionLabel.Text = "[Text]";
            this._verifyInstructionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _passphraseGroupBox
            // 
            this._passphraseGroupBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this._passphraseGroupBox.Controls.Add(this._showPassphraseCheckBox);
            this._passphraseGroupBox.Controls.Add(this._panel1);
            this._passphraseGroupBox.Controls.Add(this._passphraseTextBox);
            this._passphraseGroupBox.Location = new System.Drawing.Point(3, 46);
            this._passphraseGroupBox.MinimumSize = new System.Drawing.Size(0, 120);
            this._passphraseGroupBox.Name = "_passphraseGroupBox";
            this._passphraseGroupBox.Size = new System.Drawing.Size(304, 120);
            this._passphraseGroupBox.TabIndex = 2;
            this._passphraseGroupBox.TabStop = false;
            this._passphraseGroupBox.Text = "[Enter Passphrase]";
            // 
            // _showPassphraseCheckBox
            // 
            this._showPassphraseCheckBox.AutoSize = true;
            this._showPassphraseCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._showPassphraseCheckBox.Location = new System.Drawing.Point(11, 45);
            this._showPassphraseCheckBox.Name = "_showPassphraseCheckBox";
            this._showPassphraseCheckBox.Size = new System.Drawing.Size(117, 17);
            this._showPassphraseCheckBox.TabIndex = 1;
            this._showPassphraseCheckBox.Text = "[Show Passphrase]";
            this._showPassphraseCheckBox.UseVisualStyleBackColor = true;
            // 
            // _panel1
            // 
            this._panel1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this._panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Location = new System.Drawing.Point(66, 68);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(173, 36);
            this._panel1.TabIndex = 2;
            // 
            // _passphraseTextBox
            // 
            this._passphraseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._passphraseTextBox.Location = new System.Drawing.Point(11, 19);
            this._passphraseTextBox.Name = "_passphraseTextBox";
            this._passphraseTextBox.Size = new System.Drawing.Size(266, 20);
            this._passphraseTextBox.TabIndex = 0;
            this._passphraseTextBox.Enter += new System.EventHandler(this.PassphraseTextBox_Enter);
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // VerifySignInPasswordDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(310, 163);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "VerifySignInPasswordDialog";
            this.Text = "VerifySignInPasswordDialog";
            this.Activated += new System.EventHandler(this.VerifySignInPasswordDialog_Activated);
            this.Load += new System.EventHandler(this.VerifySignInPasswordDialog_Load);
            this.ResizeEnd += new System.EventHandler(this.VerifySignInPasswordDialog_ResizeAndMoveEnd);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this._passphraseGroupBox.ResumeLayout(false);
            this._passphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label _verifyInstructionLabel;
        internal System.Windows.Forms.GroupBox _passphraseGroupBox;
        internal System.Windows.Forms.CheckBox _showPassphraseCheckBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.TextBox _passphraseTextBox;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
    }
}