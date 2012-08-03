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
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
            this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.ProductNameText, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.VersionText, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.CopyrightText, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.CompanyNameText, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.Description, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.okButton, 1, 5);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 6;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(417, 265);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoPictureBox.Image = global::Axantum.AxCrypt.Properties.Resources.AxCryptIcon128;
            this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
            this.logoPictureBox.Name = "logoPictureBox";
            this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 6);
            this.logoPictureBox.Size = new System.Drawing.Size(131, 259);
            this.logoPictureBox.TabIndex = 12;
            this.logoPictureBox.TabStop = false;
            // 
            // ProductNameText
            // 
            this.ProductNameText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProductNameText.Location = new System.Drawing.Point(143, 0);
            this.ProductNameText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.ProductNameText.MaximumSize = new System.Drawing.Size(0, 17);
            this.ProductNameText.Name = "ProductNameText";
            this.ProductNameText.Size = new System.Drawing.Size(271, 17);
            this.ProductNameText.TabIndex = 19;
            this.ProductNameText.Text = "AxCrypt File Encryption";
            this.ProductNameText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VersionText
            // 
            this.VersionText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VersionText.Location = new System.Drawing.Point(143, 26);
            this.VersionText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.VersionText.MaximumSize = new System.Drawing.Size(0, 17);
            this.VersionText.Name = "VersionText";
            this.VersionText.Size = new System.Drawing.Size(271, 17);
            this.VersionText.TabIndex = 0;
            this.VersionText.Text = "Version";
            this.VersionText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CopyrightText
            // 
            this.CopyrightText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CopyrightText.Location = new System.Drawing.Point(143, 52);
            this.CopyrightText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CopyrightText.MaximumSize = new System.Drawing.Size(0, 17);
            this.CopyrightText.Name = "CopyrightText";
            this.CopyrightText.Size = new System.Drawing.Size(271, 17);
            this.CopyrightText.TabIndex = 21;
            this.CopyrightText.Text = "Copyright 2012 Axantum Software AB";
            this.CopyrightText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CompanyNameText
            // 
            this.CompanyNameText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CompanyNameText.Location = new System.Drawing.Point(143, 78);
            this.CompanyNameText.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CompanyNameText.MaximumSize = new System.Drawing.Size(0, 17);
            this.CompanyNameText.Name = "CompanyNameText";
            this.CompanyNameText.Size = new System.Drawing.Size(271, 17);
            this.CompanyNameText.TabIndex = 22;
            this.CompanyNameText.Text = "Axantum Software AB";
            this.CompanyNameText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // Description
            // 
            this.Description.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Description.Location = new System.Drawing.Point(143, 107);
            this.Description.Margin = new System.Windows.Forms.Padding(6, 3, 3, 3);
            this.Description.Multiline = true;
            this.Description.Name = "Description";
            this.Description.ReadOnly = true;
            this.Description.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Description.Size = new System.Drawing.Size(271, 126);
            this.Description.TabIndex = 23;
            this.Description.TabStop = false;
            this.Description.Text = "Description";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(339, 239);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            // 
            // AboutBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 283);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutBox";
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
