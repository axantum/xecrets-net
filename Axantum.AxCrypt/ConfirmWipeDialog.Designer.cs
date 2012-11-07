namespace Axantum.AxCrypt
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
            this.promptLabel = new System.Windows.Forms.Label();
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.iconPictureBox = new System.Windows.Forms.PictureBox();
            this.noButton = new System.Windows.Forms.Button();
            this.yesButton = new System.Windows.Forms.Button();
            this.ConfirmAllCheckBox = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.CausesValidation = false;
            this.promptLabel.Location = new System.Drawing.Point(75, 9);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(387, 45);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Are you sure want to permanently delete this file? This action cannot be undone.";
            this.promptLabel.Click += new System.EventHandler(this.promptLabel_Click);
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoEllipsis = true;
            this.FileNameLabel.Location = new System.Drawing.Point(75, 54);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(387, 24);
            this.FileNameLabel.TabIndex = 1;
            this.FileNameLabel.Text = "A file.ext";
            // 
            // iconPictureBox
            // 
            this.iconPictureBox.Location = new System.Drawing.Point(13, 9);
            this.iconPictureBox.Name = "iconPictureBox";
            this.iconPictureBox.Size = new System.Drawing.Size(48, 48);
            this.iconPictureBox.TabIndex = 2;
            this.iconPictureBox.TabStop = false;
            // 
            // noButton
            // 
            this.noButton.AutoSize = true;
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Location = new System.Drawing.Point(306, 116);
            this.noButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.noButton.Name = "noButton";
            this.noButton.Size = new System.Drawing.Size(75, 23);
            this.noButton.TabIndex = 3;
            this.noButton.Text = "&No";
            this.noButton.UseVisualStyleBackColor = true;
            // 
            // yesButton
            // 
            this.yesButton.AutoSize = true;
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Location = new System.Drawing.Point(225, 116);
            this.yesButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.yesButton.Name = "yesButton";
            this.yesButton.Size = new System.Drawing.Size(75, 23);
            this.yesButton.TabIndex = 4;
            this.yesButton.Text = "&Yes";
            this.yesButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmAllCheckBox
            // 
            this.ConfirmAllCheckBox.AutoSize = true;
            this.ConfirmAllCheckBox.Location = new System.Drawing.Point(78, 81);
            this.ConfirmAllCheckBox.Name = "ConfirmAllCheckBox";
            this.ConfirmAllCheckBox.Size = new System.Drawing.Size(162, 17);
            this.ConfirmAllCheckBox.TabIndex = 5;
            this.ConfirmAllCheckBox.Text = "Do this for &all remaining files?";
            this.ConfirmAllCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.AutoSize = true;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(387, 116);
            this.cancelButton.MinimumSize = new System.Drawing.Size(75, 23);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "&Cancel All";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmWipeDialog
            // 
            this.AcceptButton = this.cancelButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(474, 151);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.ConfirmAllCheckBox);
            this.Controls.Add(this.yesButton);
            this.Controls.Add(this.noButton);
            this.Controls.Add(this.iconPictureBox);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this.promptLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfirmWipeDialog";
            this.Text = "AxCrypt Secure Delete";
            this.Load += new System.EventHandler(this.ConfirmWipeDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.PictureBox iconPictureBox;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Button cancelButton;
        internal System.Windows.Forms.CheckBox ConfirmAllCheckBox;
        internal System.Windows.Forms.Label FileNameLabel;

    }
}