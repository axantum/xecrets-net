namespace Axantum.AxCrypt
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.ProductNameText = new System.Windows.Forms.Label();
            this.VersionText = new System.Windows.Forms.Label();
            this.CopyrightText = new System.Windows.Forms.Label();
            this.CompanyNameText = new System.Windows.Forms.Label();
            this.Description = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Image = global::Axantum.AxCrypt.Properties.Resources.axcrypticon128;
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.TabStop = false;
            // 
            // ProductNameText
            // 
            resources.ApplyResources(this.ProductNameText, "ProductNameText");
            this.ProductNameText.Name = "ProductNameText";
            // 
            // VersionText
            // 
            resources.ApplyResources(this.VersionText, "VersionText");
            this.VersionText.Name = "VersionText";
            // 
            // CopyrightText
            // 
            resources.ApplyResources(this.CopyrightText, "CopyrightText");
            this.CopyrightText.Name = "CopyrightText";
            // 
            // CompanyNameText
            // 
            resources.ApplyResources(this.CompanyNameText, "CompanyNameText");
            this.CompanyNameText.Name = "CompanyNameText";
            // 
            // Description
            // 
            resources.ApplyResources(this.Description, "Description");
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.TabStop = false;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Name = "okButton";
            // 
            // AboutBox
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this.ProductNameText);
            this.Controls.Add(this.VersionText);
            this.Controls.Add(this.CopyrightText);
            this.Controls.Add(this.CompanyNameText);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.AboutBox_Load);
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox logoPictureBox;
        internal System.Windows.Forms.Label ProductNameText;
        internal System.Windows.Forms.Label VersionText;
        internal System.Windows.Forms.Label CopyrightText;
        internal System.Windows.Forms.Label CompanyNameText;
        internal System.Windows.Forms.TextBox Description;
        private System.Windows.Forms.Button okButton;

    }
}
