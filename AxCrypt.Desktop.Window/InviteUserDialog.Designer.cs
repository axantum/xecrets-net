namespace AxCrypt.Desktop.Window
{
    partial class InviteUserDialog
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
            this._inviteUserPromptlabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this._panel1 = new System.Windows.Forms.Panel();
            this._buttonOk = new System.Windows.Forms.Button();
            this._buttonCancel = new System.Windows.Forms.Button();
            this.emailPanel = new System.Windows.Forms.Panel();
            this._emailGroupBox = new System.Windows.Forms.GroupBox();
            this._emailTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this._panel1.SuspendLayout();
            this.emailPanel.SuspendLayout();
            this._emailGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this._inviteUserPromptlabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(307, 55);
            this.panel1.TabIndex = 4;
            // 
            // _inviteUserPromptlabel
            // 
            this._inviteUserPromptlabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._inviteUserPromptlabel.Location = new System.Drawing.Point(3, 7);
            this._inviteUserPromptlabel.Name = "_inviteUserPromptlabel";
            this._inviteUserPromptlabel.Size = new System.Drawing.Size(301, 41);
            this._inviteUserPromptlabel.TabIndex = 0;
            this._inviteUserPromptlabel.Text = "[Type an email address of a person you wish to invite to use AxCrypt.]";
            this._inviteUserPromptlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this._panel1);
            this.panel2.Controls.Add(this.emailPanel);
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
            this._panel1.Controls.Add(this._buttonCancel);
            this._panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._panel1.Location = new System.Drawing.Point(0, 59);
            this._panel1.Name = "_panel1";
            this._panel1.Size = new System.Drawing.Size(307, 41);
            this._panel1.TabIndex = 7;
            // 
            // _buttonOk
            // 
            this._buttonOk.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this._buttonOk.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonOk.Location = new System.Drawing.Point(63, 6);
            this._buttonOk.Name = "_buttonOk";
            this._buttonOk.Size = new System.Drawing.Size(80, 23);
            this._buttonOk.TabIndex = 0;
            this._buttonOk.Text = "[OK]";
            this._buttonOk.UseVisualStyleBackColor = true;
            // 
            // _buttonCancel
            // 
            this._buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this._buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._buttonCancel.Location = new System.Drawing.Point(163, 6);
            this._buttonCancel.Name = "_buttonCancel";
            this._buttonCancel.Size = new System.Drawing.Size(80, 23);
            this._buttonCancel.TabIndex = 0;
            this._buttonCancel.Text = "[CANCEL]";
            this._buttonCancel.UseVisualStyleBackColor = true;
            // 
            // emailPanel
            // 
            this.emailPanel.AutoSize = true;
            this.emailPanel.Controls.Add(this._emailGroupBox);
            this.emailPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.emailPanel.Location = new System.Drawing.Point(0, 0);
            this.emailPanel.Name = "EmailPanel";
            this.emailPanel.Size = new System.Drawing.Size(307, 59);
            this.emailPanel.TabIndex = 6;
            // 
            // _emailGroupBox
            // 
            this._emailGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailGroupBox.Controls.Add(this._emailTextBox);
            this._emailGroupBox.Location = new System.Drawing.Point(12, 12);
            this._emailGroupBox.Margin = new System.Windows.Forms.Padding(3, 3, 13, 3);
            this._emailGroupBox.Name = "_emailGroupBox";
            this._emailGroupBox.Size = new System.Drawing.Size(282, 44);
            this._emailGroupBox.TabIndex = 0;
            this._emailGroupBox.TabStop = false;
            this._emailGroupBox.Text = "[Email address]";
            // 
            // _emailTextBox
            // 
            this._emailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this._emailTextBox.Location = new System.Drawing.Point(9, 18);
            this._emailTextBox.Name = "_emailTextBox";
            this._emailTextBox.Size = new System.Drawing.Size(248, 20);
            this._emailTextBox.TabIndex = 0;
            // 
            // InviteUserDialog
            // 
            this.AcceptButton = this._buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 155);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(270, 140);
            this.Name = "InviteUserDialog";
            this.Text = "[Invite User]";
            this.Load += new System.EventHandler(this.InviteUserDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this._panel1.ResumeLayout(false);
            this.emailPanel.ResumeLayout(false);
            this._emailGroupBox.ResumeLayout(false);
            this._emailGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ErrorProvider _errorProvider1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label _inviteUserPromptlabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel _panel1;
        private System.Windows.Forms.Button _buttonOk;
        private System.Windows.Forms.Button _buttonCancel;
        private System.Windows.Forms.Panel emailPanel;
        private System.Windows.Forms.GroupBox _emailGroupBox;
        private System.Windows.Forms.TextBox _emailTextBox;
    }
}