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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.ProductNameText = new System.Windows.Forms.Label();
            this.VersionText = new System.Windows.Forms.Label();
            this.CopyrightText = new System.Windows.Forms.Label();
            this.CompanyNameText = new System.Windows.Forms.Label();
            this.Description = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            resources.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
            this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.ProductNameText, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.VersionText, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.CopyrightText, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.CompanyNameText, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.Description, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.okButton, 1, 5);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            // 
            // logoPictureBox
            // 
            resources.ApplyResources(this.logoPictureBox, "logoPictureBox");
            this.logoPictureBox.Image = global::Axantum.AxCrypt.Properties.Resources.AxCryptIcon128;
            this.logoPictureBox.Name = "logoPictureBox";
            this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 6);
            this.logoPictureBox.TabStop = false;
            // 
            // ProductNameText
            // 
            resources.ApplyResources(this.ProductNameText, "ProductNameText");
            this.ProductNameText.MaximumSize = new System.Drawing.Size(0, 17);
            this.ProductNameText.Name = "ProductNameText";
            // 
            // VersionText
            // 
            resources.ApplyResources(this.VersionText, "VersionText");
            this.VersionText.MaximumSize = new System.Drawing.Size(0, 17);
            this.VersionText.Name = "VersionText";
            // 
            // CopyrightText
            // 
            resources.ApplyResources(this.CopyrightText, "CopyrightText");
            this.CopyrightText.MaximumSize = new System.Drawing.Size(0, 17);
            this.CopyrightText.Name = "CopyrightText";
            // 
            // CompanyNameText
            // 
            resources.ApplyResources(this.CompanyNameText, "CompanyNameText");
            this.CompanyNameText.MaximumSize = new System.Drawing.Size(0, 17);
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
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Button okButton;
        internal System.Windows.Forms.Label ProductNameText;
        internal System.Windows.Forms.Label VersionText;
        internal System.Windows.Forms.Label CopyrightText;
        internal System.Windows.Forms.Label CompanyNameText;
        internal System.Windows.Forms.TextBox Description;
    }
}
