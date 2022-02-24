namespace AxCrypt.Desktop.Window
{
    partial class LegacyKeyShareDialog
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
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this._knownContactsGroupBox = new System.Windows.Forms.GroupBox();
            this._notSharedWith = new System.Windows.Forms.ListBox();
            this._addContactGroupBox = new System.Windows.Forms.GroupBox();
            this._newContact = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this._removeKnownContactButton = new System.Windows.Forms.Button();
            this._unshareButton = new System.Windows.Forms.Button();
            this._shareButton = new System.Windows.Forms.Button();
            this._sharedWithGroupBox = new System.Windows.Forms.GroupBox();
            this._sharedWith = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this._knownContactsGroupBox.SuspendLayout();
            this._addContactGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this._sharedWithGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(534, 211);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this._sharedWithGroupBox, 2, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.MinimumSize = new System.Drawing.Size(475, 100);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(528, 170);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this._knownContactsGroupBox, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this._addContactGroupBox, 0, 1);
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(214, 170);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // groupBox1
            // 
            this._knownContactsGroupBox.Controls.Add(this._notSharedWith);
            this._knownContactsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._knownContactsGroupBox.Location = new System.Drawing.Point(3, 3);
            this._knownContactsGroupBox.Name = "groupBox1";
            this._knownContactsGroupBox.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._knownContactsGroupBox.Size = new System.Drawing.Size(208, 119);
            this._knownContactsGroupBox.TabIndex = 2;
            this._knownContactsGroupBox.TabStop = false;
            this._knownContactsGroupBox.Text = "[Known Contacts]";
            // 
            // _notSharedWith
            // 
            this._notSharedWith.Dock = System.Windows.Forms.DockStyle.Fill;
            this._notSharedWith.FormattingEnabled = true;
            this._notSharedWith.Location = new System.Drawing.Point(3, 16);
            this._notSharedWith.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._notSharedWith.Name = "_notSharedWith";
            this._notSharedWith.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._notSharedWith.Size = new System.Drawing.Size(202, 103);
            this._notSharedWith.TabIndex = 0;
            // 
            // groupBox3
            // 
            this._addContactGroupBox.Controls.Add(this._newContact);
            this._addContactGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._addContactGroupBox.Location = new System.Drawing.Point(3, 128);
            this._addContactGroupBox.Name = "groupBox3";
            this._addContactGroupBox.Padding = new System.Windows.Forms.Padding(3, 3, 25, 3);
            this._addContactGroupBox.Size = new System.Drawing.Size(208, 39);
            this._addContactGroupBox.TabIndex = 3;
            this._addContactGroupBox.TabStop = false;
            this._addContactGroupBox.Text = "[Add Contact]";
            // 
            // _newContact
            // 
            this._newContact.Dock = System.Windows.Forms.DockStyle.Fill;
            this._newContact.Location = new System.Drawing.Point(3, 16);
            this._newContact.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this._newContact.Name = "_newContact";
            this._newContact.Size = new System.Drawing.Size(180, 20);
            this._newContact.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this._removeKnownContactButton, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._unshareButton, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._shareButton, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(217, 16);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 16, 3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(94, 83);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // _removeKnownContactButton
            // 
            this._removeKnownContactButton.AutoSize = true;
            this._removeKnownContactButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._removeKnownContactButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._removeKnownContactButton.Location = new System.Drawing.Point(3, 61);
            this._removeKnownContactButton.MinimumSize = new System.Drawing.Size(58, 23);
            this._removeKnownContactButton.Name = "_removeKnownContactButton";
            this._removeKnownContactButton.Size = new System.Drawing.Size(88, 23);
            this._removeKnownContactButton.TabIndex = 2;
            this._removeKnownContactButton.Text = "[Remove]";
            this._removeKnownContactButton.UseVisualStyleBackColor = true;
            this._removeKnownContactButton.Visible = false;
            // 
            // _unshareButton
            // 
            this._unshareButton.AutoSize = true;
            this._unshareButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._unshareButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._unshareButton.Location = new System.Drawing.Point(3, 32);
            this._unshareButton.MinimumSize = new System.Drawing.Size(58, 23);
            this._unshareButton.Name = "_unshareButton";
            this._unshareButton.Size = new System.Drawing.Size(88, 23);
            this._unshareButton.TabIndex = 1;
            this._unshareButton.Text = "[<< Unshare]";
            this._unshareButton.UseVisualStyleBackColor = true;
            this._unshareButton.Visible = false;
            // 
            // _shareButton
            // 
            this._shareButton.AutoSize = true;
            this._shareButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._shareButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this._shareButton.Location = new System.Drawing.Point(3, 3);
            this._shareButton.MinimumSize = new System.Drawing.Size(58, 23);
            this._shareButton.Name = "_shareButton";
            this._shareButton.Size = new System.Drawing.Size(88, 23);
            this._shareButton.TabIndex = 0;
            this._shareButton.Text = "[Share >>]";
            this._shareButton.UseVisualStyleBackColor = true;
            this._shareButton.Visible = false;
            // 
            // groupBox2
            // 
            this._sharedWithGroupBox.Controls.Add(this._sharedWith);
            this._sharedWithGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sharedWithGroupBox.Location = new System.Drawing.Point(317, 3);
            this._sharedWithGroupBox.Name = "groupBox2";
            this._sharedWithGroupBox.Padding = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._sharedWithGroupBox.Size = new System.Drawing.Size(208, 164);
            this._sharedWithGroupBox.TabIndex = 0;
            this._sharedWithGroupBox.TabStop = false;
            this._sharedWithGroupBox.Text = "[Shared With]";
            // 
            // _sharedWith
            // 
            this._sharedWith.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sharedWith.FormattingEnabled = true;
            this._sharedWith.Location = new System.Drawing.Point(3, 16);
            this._sharedWith.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this._sharedWith.Name = "_sharedWith";
            this._sharedWith.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this._sharedWith.Size = new System.Drawing.Size(202, 148);
            this._sharedWith.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this._okButton, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this._cancelButton, 1, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(366, 179);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(162, 29);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(3, 3);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(80, 23);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "[OK]";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // _cancelButton
            // 
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(89, 3);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(80, 23);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "[Cancel]";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // KeyShareDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(534, 211);
            this.Controls.Add(this.tableLayoutPanel3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 200);
            this.Name = "KeyShareDialog";
            this.Text = "[AxCrypt Key Sharing]";
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this._knownContactsGroupBox.ResumeLayout(false);
            this._addContactGroupBox.ResumeLayout(false);
            this._addContactGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this._sharedWithGroupBox.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox _sharedWithGroupBox;
        private System.Windows.Forms.ListBox _sharedWith;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button _removeKnownContactButton;
        private System.Windows.Forms.Button _unshareButton;
        private System.Windows.Forms.Button _shareButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.GroupBox _knownContactsGroupBox;
        private System.Windows.Forms.ListBox _notSharedWith;
        private System.Windows.Forms.GroupBox _addContactGroupBox;
        private System.Windows.Forms.TextBox _newContact;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
    }
}