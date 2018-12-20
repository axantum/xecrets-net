namespace Axantum.AxCrypt.Forms
{
    partial class ConfirmWipeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmWipeDialog));
            this._promptLabel = new System.Windows.Forms.Label();
            this.FileNameLabel = new System.Windows.Forms.Label();
            this._iconPictureBox = new System.Windows.Forms.PictureBox();
            this._noButton = new System.Windows.Forms.Button();
            this._yesButton = new System.Windows.Forms.Button();
            this._confirmAllCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // _promptLabel
            // 
            this._promptLabel.CausesValidation = false;
            this._promptLabel.Location = new System.Drawing.Point(75, 9);
            this._promptLabel.Name = "_promptLabel";
            this._promptLabel.Size = new System.Drawing.Size(387, 45);
            this._promptLabel.TabIndex = 0;
            this._promptLabel.Text = "[Are you sure want to permanently delete this document? This action cannot be und" +
    "one.]";
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoEllipsis = true;
            this.FileNameLabel.Location = new System.Drawing.Point(75, 54);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(387, 24);
            this.FileNameLabel.TabIndex = 1;
            this.FileNameLabel.Text = "[My Document.ext]";
            // 
            // _iconPictureBox
            // 
            this._iconPictureBox.Location = new System.Drawing.Point(13, 9);
            this._iconPictureBox.Name = "_iconPictureBox";
            this._iconPictureBox.Size = new System.Drawing.Size(48, 48);
            this._iconPictureBox.TabIndex = 2;
            this._iconPictureBox.TabStop = false;
            // 
            // _noButton
            // 
            this._noButton.AutoSize = true;
            this._noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this._noButton.Location = new System.Drawing.Point(306, 116);
            this._noButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._noButton.Name = "_noButton";
            this._noButton.Size = new System.Drawing.Size(75, 23);
            this._noButton.TabIndex = 4;
            this._noButton.Text = "[&No]";
            this._noButton.UseVisualStyleBackColor = true;
            // 
            // _yesButton
            // 
            this._yesButton.AutoSize = true;
            this._yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this._yesButton.Location = new System.Drawing.Point(225, 116);
            this._yesButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._yesButton.Name = "_yesButton";
            this._yesButton.Size = new System.Drawing.Size(75, 23);
            this._yesButton.TabIndex = 3;
            this._yesButton.Text = "[&Yes]";
            this._yesButton.UseVisualStyleBackColor = true;
            // 
            // _confirmAllCheckBox
            // 
            this._confirmAllCheckBox.AutoSize = true;
            this._confirmAllCheckBox.Location = new System.Drawing.Point(78, 81);
            this._confirmAllCheckBox.Name = "_confirmAllCheckBox";
            this._confirmAllCheckBox.Size = new System.Drawing.Size(202, 17);
            this._confirmAllCheckBox.TabIndex = 2;
            this._confirmAllCheckBox.Text = "[Do this for &all remaining documents?]";
            this._confirmAllCheckBox.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.AutoSize = true;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(387, 116);
            this._cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(80, 23);
            this._cancelButton.TabIndex = 5;
            this._cancelButton.Text = "[&Cancel All]";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmWipeDialog
            // 
            this.AcceptButton = this._cancelButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(474, 151);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._confirmAllCheckBox);
            this.Controls.Add(this._yesButton);
            this.Controls.Add(this._noButton);
            this.Controls.Add(this._iconPictureBox);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this._promptLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfirmWipeDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[AxCrypt Secure Delete]";
            this.Load += new System.EventHandler(this.ConfirmWipeDialog_Load);
            this.Shown += new System.EventHandler(this.ConfirmWipeDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _promptLabel;
        private System.Windows.Forms.PictureBox _iconPictureBox;
        private System.Windows.Forms.Button _noButton;
        private System.Windows.Forms.Button _yesButton;
        private System.Windows.Forms.Button _cancelButton;
        internal System.Windows.Forms.CheckBox _confirmAllCheckBox;
        internal System.Windows.Forms.Label FileNameLabel;

    }
}