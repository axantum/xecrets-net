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
            this._promptLabel = new System.Windows.Forms.Label();
            this.FileNameLabel = new System.Windows.Forms.Label();
            this._iconPictureBox = new System.Windows.Forms.PictureBox();
            this._noButton = new System.Windows.Forms.Button();
            this._yesButton = new System.Windows.Forms.Button();
            this.ConfirmAllCheckBox = new System.Windows.Forms.CheckBox();
            this._cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this._promptLabel.CausesValidation = false;
            resources.ApplyResources(this._promptLabel, "promptLabel");
            this._promptLabel.Name = "promptLabel";
            this._promptLabel.Click += new System.EventHandler(this.promptLabel_Click);
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoEllipsis = true;
            resources.ApplyResources(this.FileNameLabel, "FileNameLabel");
            this.FileNameLabel.Name = "FileNameLabel";
            // 
            // iconPictureBox
            // 
            resources.ApplyResources(this._iconPictureBox, "iconPictureBox");
            this._iconPictureBox.Name = "iconPictureBox";
            this._iconPictureBox.TabStop = false;
            // 
            // noButton
            // 
            resources.ApplyResources(this._noButton, "noButton");
            this._noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this._noButton.Name = "noButton";
            this._noButton.UseVisualStyleBackColor = true;
            // 
            // yesButton
            // 
            resources.ApplyResources(this._yesButton, "yesButton");
            this._yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this._yesButton.Name = "yesButton";
            this._yesButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmAllCheckBox
            // 
            resources.ApplyResources(this.ConfirmAllCheckBox, "ConfirmAllCheckBox");
            this.ConfirmAllCheckBox.Name = "ConfirmAllCheckBox";
            this.ConfirmAllCheckBox.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            resources.ApplyResources(this._cancelButton, "cancelButton");
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Name = "cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConfirmWipeDialog
            // 
            this.AcceptButton = this._cancelButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this.ConfirmAllCheckBox);
            this.Controls.Add(this._yesButton);
            this.Controls.Add(this._noButton);
            this.Controls.Add(this._iconPictureBox);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this._promptLabel);
            this.Name = "ConfirmWipeDialog";
            this.Load += new System.EventHandler(this.ConfirmWipeDialog_Load);
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
        internal System.Windows.Forms.CheckBox ConfirmAllCheckBox;
        internal System.Windows.Forms.Label FileNameLabel;

    }
}