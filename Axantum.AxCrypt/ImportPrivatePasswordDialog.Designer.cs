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
            this.components = new System.ComponentModel.Container();
            this.PassphraseGroupBox = new System.Windows.Forms.GroupBox();
            this._showPassphraseCheckBox = new System.Windows.Forms.CheckBox();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonCancel = new System.Windows.Forms.Button();
            this._buttonOk = new System.Windows.Forms.Button();
            this._passphraseTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this._accessIdGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this._privateKeyFileTextBox = new System.Windows.Forms.TextBox();
            this._browsePrivateKeyFileButton = new System.Windows.Forms.Button();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this.PassphraseGroupBox.SuspendLayout();
            this._panel1.SuspendLayout();
            this._accessIdGroupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            this.SuspendLayout();
            // 
            // PassphraseGroupBox
            // 
            this.PassphraseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PassphraseGroupBox.Controls.Add(this._showPassphraseCheckBox);
            this.PassphraseGroupBox.Controls.Add(this._panel1);
            this.PassphraseGroupBox.Controls.Add(this._passphraseTextBox);
            this.PassphraseGroupBox.Location = new System.Drawing.Point(12, 75);
            this.PassphraseGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 13);
            this.PassphraseGroupBox.Name = "PassphraseGroupBox";
            this.PassphraseGroupBox.Size = new System.Drawing.Size(286, 112);
            this.PassphraseGroupBox.TabIndex = 1;
            this.PassphraseGroupBox.TabStop = false;
            this.PassphraseGroupBox.Text = "[Enter Passphrase]";
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
            this._panel1.Location = new System.Drawing.Point(57, 68);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(173, 36);
            this._panel1.TabIndex = 2;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(89, 7);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(80, 23);
            this._buttonCancel.TabIndex = 1;
            this._buttonCancel.Text = "[Cancel]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // _buttonOk
            // 
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(3, 7);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // _passphraseTextBox
            // 
            this._passphraseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._errorProvider2.SetIconPadding(this._passphraseTextBox, 5);
            this._errorProvider1.SetIconPadding(this._passphraseTextBox, 5);
            this._passphraseTextBox.Location = new System.Drawing.Point(11, 19);
            this._passphraseTextBox.Name = "_passphraseTextBox";
            this._passphraseTextBox.Size = new System.Drawing.Size(216, 20);
            this._passphraseTextBox.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // groupBox1
            // 
            this._accessIdGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._accessIdGroupBox.Controls.Add(this.tableLayoutPanel1);
            this._accessIdGroupBox.Location = new System.Drawing.Point(12, 12);
            this._accessIdGroupBox.Name = "groupBox1";
            this._accessIdGroupBox.Size = new System.Drawing.Size(286, 57);
            this._accessIdGroupBox.TabIndex = 0;
            this._accessIdGroupBox.TabStop = false;
            this._accessIdGroupBox.Text = "[Secret and Sharing Key Pair]";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.Controls.Add(this._privateKeyFileTextBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this._browsePrivateKeyFileButton, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(8, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(272, 29);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // _privateKeyFileTextBox
            // 
            this._privateKeyFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this._privateKeyFileTextBox.CausesValidation = false;
            this._privateKeyFileTextBox.Location = new System.Drawing.Point(3, 5);
            this._privateKeyFileTextBox.Name = "_privateKeyFileTextBox";
            this._privateKeyFileTextBox.Size = new System.Drawing.Size(216, 20);
            this._privateKeyFileTextBox.TabIndex = 0;
            // 
            // _browsePrivateKeyFileButton
            // 
            this._browsePrivateKeyFileButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._browsePrivateKeyFileButton.CausesValidation = false;
            this._errorProvider2.SetIconPadding(this._browsePrivateKeyFileButton, 5);
            this._errorProvider1.SetIconPadding(this._browsePrivateKeyFileButton, 5);
            this._browsePrivateKeyFileButton.Location = new System.Drawing.Point(225, 3);
            this._browsePrivateKeyFileButton.Name = "_browsePrivateKeyFileButton";
            this._browsePrivateKeyFileButton.Size = new System.Drawing.Size(27, 24);
            this._browsePrivateKeyFileButton.TabIndex = 1;
            this._browsePrivateKeyFileButton.Text = "...";
            this._browsePrivateKeyFileButton.UseVisualStyleBackColor = true;
            this._browsePrivateKeyFileButton.Click += new System.EventHandler(this._browsePrivateKeyFileButton_Click);
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // ImportPrivatePasswordDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._buttonCancel;
            this.ClientSize = new System.Drawing.Size(305, 196);
            this.Controls.Add(this._accessIdGroupBox);
            this.Controls.Add(this.PassphraseGroupBox);
            this.MaximumSize = new System.Drawing.Size(500, 235);
            this.MinimumSize = new System.Drawing.Size(300, 235);
            this.Name = "ImportPrivatePasswordDialog";
            this.Text = "[Import Account Secret and Sharing Key Pair]";
            this.PassphraseGroupBox.ResumeLayout(false);
            this.PassphraseGroupBox.PerformLayout();
            this._panel1.ResumeLayout(false);
            this._accessIdGroupBox.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.GroupBox PassphraseGroupBox;
        internal System.Windows.Forms.CheckBox _showPassphraseCheckBox;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Button _buttonOk;
        internal System.Windows.Forms.TextBox _passphraseTextBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox _accessIdGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox _privateKeyFileTextBox;
        private System.Windows.Forms.Button _browsePrivateKeyFileButton;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
    }
}