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
            resources.ApplyResources(this.promptLabel, "promptLabel");
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Click += new System.EventHandler(this.promptLabel_Click);
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoEllipsis = true;
            resources.ApplyResources(this.FileNameLabel, "FileNameLabel");
            this.FileNameLabel.Name = "FileNameLabel";
            // 
            // iconPictureBox
            // 
            resources.ApplyResources(this.iconPictureBox, "iconPictureBox");
            this.iconPictureBox.Name = "iconPictureBox";
            this.iconPictureBox.TabStop = false;
            // 
            // noButton
            // 
            resources.ApplyResources(this.noButton, "noButton");
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Name = "noButton";
            this.noButton.UseVisualStyleBackColor = true;
            // 
            // yesButton
            // 
            resources.ApplyResources(this.yesButton, "yesButton");
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Name = "yesButton";
            this.yesButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmAllCheckBox
            // 
            resources.ApplyResources(this.ConfirmAllCheckBox, "ConfirmAllCheckBox");
            this.ConfirmAllCheckBox.Name = "ConfirmAllCheckBox";
            this.ConfirmAllCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmWipeDialog
            // 
            this.AcceptButton = this.cancelButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.ConfirmAllCheckBox);
            this.Controls.Add(this.yesButton);
            this.Controls.Add(this.noButton);
            this.Controls.Add(this.iconPictureBox);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this.promptLabel);
            this.Name = "ConfirmWipeDialog";
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