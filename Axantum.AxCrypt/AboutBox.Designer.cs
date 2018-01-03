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
            this.SubscriptionStatusAndExpirationText = new System.Windows.Forms.Label();
            this.Description = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Image = global::Axantum.AxCrypt.Properties.Resources.axcrypticon128;
            this.logoPictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
            this.logoPictureBox.Name = "logoPictureBox";
            this.logoPictureBox.Size = new System.Drawing.Size(123, 134);
            this.logoPictureBox.TabIndex = 26;
            this.logoPictureBox.TabStop = false;
            // 
            // ProductNameText
            // 
            this.ProductNameText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ProductNameText.Location = new System.Drawing.Point(143, 6);
            this.ProductNameText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.ProductNameText.Name = "ProductNameText";
            this.ProductNameText.Size = new System.Drawing.Size(271, 17);
            this.ProductNameText.TabIndex = 27;
            this.ProductNameText.Text = "[AxCrypt File Encryption]";
            this.ProductNameText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VersionText
            // 
            this.VersionText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VersionText.Location = new System.Drawing.Point(143, 30);
            this.VersionText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.VersionText.Name = "VersionText";
            this.VersionText.Size = new System.Drawing.Size(271, 17);
            this.VersionText.TabIndex = 25;
            this.VersionText.Text = "[Version]";
            this.VersionText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CopyrightText
            // 
            this.CopyrightText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CopyrightText.Location = new System.Drawing.Point(143, 54);
            this.CopyrightText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CopyrightText.Name = "CopyrightText";
            this.CopyrightText.Size = new System.Drawing.Size(271, 17);
            this.CopyrightText.TabIndex = 28;
            this.CopyrightText.Text = "[Copyright 2015 AxCrypt AB]";
            this.CopyrightText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CompanyNameText
            // 
            this.CompanyNameText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompanyNameText.Location = new System.Drawing.Point(143, 78);
            this.CompanyNameText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CompanyNameText.Name = "CompanyNameText";
            this.CompanyNameText.Size = new System.Drawing.Size(271, 17);
            this.CompanyNameText.TabIndex = 29;
            this.CompanyNameText.Text = "[AxCrypt AB]";
            this.CompanyNameText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SubscriptionStatusAndExpirationText
            // 
            this.SubscriptionStatusAndExpirationText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SubscriptionStatusAndExpirationText.Location = new System.Drawing.Point(143, 102);
            this.SubscriptionStatusAndExpirationText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.SubscriptionStatusAndExpirationText.Name = "SubscriptionStatusAndExpirationText";
            this.SubscriptionStatusAndExpirationText.Size = new System.Drawing.Size(271, 17);
            this.SubscriptionStatusAndExpirationText.TabIndex = 30;
            this.SubscriptionStatusAndExpirationText.Text = "[Free]";
            this.SubscriptionStatusAndExpirationText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SubscriptionStatusAndExpirationText.VisibleChanged += new System.EventHandler(this.SubscriptionStatusText_Visible);
            // 
            // Description
            // 
            this.Description.Location = new System.Drawing.Point(143, 136);
            this.Description.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.Description.Multiline = true;
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Description.Size = new System.Drawing.Size(271, 126);
            this.Description.TabIndex = 31;
            this.Description.TabStop = false;
            this.Description.Text = "[Description]";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.okButton.Location = new System.Drawing.Point(339, 268);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 32;
            this.okButton.Text = "[&OK]";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // AboutBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 310);
            this.Controls.Add(this.logoPictureBox);
            this.Controls.Add(this.ProductNameText);
            this.Controls.Add(this.VersionText);
            this.Controls.Add(this.CopyrightText);
            this.Controls.Add(this.CompanyNameText);
            this.Controls.Add(this.SubscriptionStatusAndExpirationText);
            this.Controls.Add(this.Description);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutBox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AboutBox_FormClosing);
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
        internal System.Windows.Forms.Label SubscriptionStatusAndExpirationText;
        internal System.Windows.Forms.TextBox Description;
        private System.Windows.Forms.Button okButton;

    }
}
