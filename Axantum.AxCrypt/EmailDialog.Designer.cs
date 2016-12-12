namespace Axantum.AxCrypt
{
    partial class EmailDialog
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
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this._firstTimePromptlabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonOk = new System.Windows.Forms.Button();
            this.EmailPanel = new System.Windows.Forms.Panel();
            this._emailGroupBox = new System.Windows.Forms.GroupBox();
            this.EmailTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this._panel1.SuspendLayout();
            this.EmailPanel.SuspendLayout();
            this._emailGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._firstTimePromptlabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(307, 55);
            this.panel1.TabIndex = 4;
            // 
            // _firstTimePromptlabel
            // 
            this._firstTimePromptlabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._firstTimePromptlabel.Location = new System.Drawing.Point(3, 7);
            this._firstTimePromptlabel.Name = "_firstTimePromptlabel";
            this._firstTimePromptlabel.Size = new System.Drawing.Size(301, 41);
            this._firstTimePromptlabel.TabIndex = 0;
            this._firstTimePromptlabel.Text = "[The first time you start AxCrypt, Internet access and a real email address is re" +
    "quired. Click help for more information.]";
            this._firstTimePromptlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this._panel1);
            this.panel2.Controls.Add(this.EmailPanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 55);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(307, 100);
            this.panel2.TabIndex = 5;
            // 
            // _panel1
            // 
            this._panel1.AutoSize = true;
            this._panel1.Controls.Add(this._buttonOk);
            this._panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panel1.Location = new System.Drawing.Point(0, 59);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(307, 41);
            this._panel1.TabIndex = 7;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this._buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(116, 6);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(75, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            this._buttonOk.Click += new System.EventHandler(this._buttonOk_Click);
            // 
            // EmailPanel
            // 
            this.EmailPanel.AutoSize = true;
            this.EmailPanel.Controls.Add(this._emailGroupBox);
            this.EmailPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EmailPanel.Location = new System.Drawing.Point(0, 0);
            this.EmailPanel.Name = "EmailPanel";
            this.EmailPanel.Size = new System.Drawing.Size(307, 59);
            this.EmailPanel.TabIndex = 6;
            // 
            // _emailGroupBox
            // 
            this._emailGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailGroupBox.Controls.Add(this.EmailTextBox);
            this._emailGroupBox.Location = new System.Drawing.Point(12, 12);
            this._emailGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._emailGroupBox.Name = "_emailGroupBox";
            this._emailGroupBox.Size = new System.Drawing.Size(282, 44);
            this._emailGroupBox.TabIndex = 0;
            this._emailGroupBox.TabStop = false;
            this._emailGroupBox.Text = "[Email address]";
            // 
            // EmailTextBox
            // 
            this.EmailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EmailTextBox.Location = new System.Drawing.Point(9, 18);
            this.EmailTextBox.Name = "EmailTextBox";
            this.EmailTextBox.Size = new System.Drawing.Size(248, 20);
            this.EmailTextBox.TabIndex = 0;
            // 
            // EmailDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 155);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(270, 140);
            this.Name = "EmailDialog";
            this.Text = "[Account Email]";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.EmailDialog_HelpButtonClicked);
            this.Load += new System.EventHandler(this.EmailDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.EmailPanel.ResumeLayout(false);
            this._emailGroupBox.ResumeLayout(false);
            this._emailGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _firstTimePromptlabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Panel EmailPanel;
        private System.Windows.Forms.GroupBox _emailGroupBox;
        internal System.Windows.Forms.TextBox EmailTextBox;
    }
}