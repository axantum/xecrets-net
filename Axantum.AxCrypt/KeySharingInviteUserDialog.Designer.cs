using AxCrypt.Content;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    partial class KeySharingInviteUserDialog
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
            this._okButton = new System.Windows.Forms.Button();
            this.DialogFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.InviteTextPanel = new System.Windows.Forms.Panel();
            this._keyShareInvitePromptlabel = new System.Windows.Forms.Label();
            this.LanguageCulturePanel = new System.Windows.Forms.Panel();
            this._languageCultureGroupBox = new System.Windows.Forms.GroupBox();
            this._languageCultureDropDown = new System.Windows.Forms.ComboBox();
            this.PersonalizedMessagePanel = new System.Windows.Forms.Panel();
            this._personalizedMessageGroupBox = new System.Windows.Forms.GroupBox();
            this._personalizedMessageTextBox = new System.Windows.Forms.TextBox();
            this.ButtonPanel = new System.Windows.Forms.Panel();
            this._cancelButton = new System.Windows.Forms.Button();
            this.DialogFlowLayoutPanel.SuspendLayout();
            this.InviteTextPanel.SuspendLayout();
            this.LanguageCulturePanel.SuspendLayout();
            this._languageCultureGroupBox.SuspendLayout();
            this.PersonalizedMessagePanel.SuspendLayout();
            this._personalizedMessageGroupBox.SuspendLayout();
            this.ButtonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._okButton.Location = new System.Drawing.Point(99, 11);
            this._okButton.Margin = new System.Windows.Forms.Padding(4);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(100, 28);
            this._okButton.TabIndex = 0;
            this._okButton.Text = "[OK]";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // DialogFlowLayoutPanel
            // 
            this.DialogFlowLayoutPanel.AutoSize = true;
            this.DialogFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DialogFlowLayoutPanel.Controls.Add(this.InviteTextPanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.LanguageCulturePanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.PersonalizedMessagePanel);
            this.DialogFlowLayoutPanel.Controls.Add(this.ButtonPanel);
            this.DialogFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DialogFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.DialogFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.DialogFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.DialogFlowLayoutPanel.Name = "DialogFlowLayoutPanel";
            this.DialogFlowLayoutPanel.Size = new System.Drawing.Size(426, 433);
            this.DialogFlowLayoutPanel.TabIndex = 0;
            // 
            // InviteTextPanel
            // 
            this.InviteTextPanel.Controls.Add(this._keyShareInvitePromptlabel);
            this.InviteTextPanel.Location = new System.Drawing.Point(4, 4);
            this.InviteTextPanel.Margin = new System.Windows.Forms.Padding(4);
            this.InviteTextPanel.Name = "InviteTextPanel";
            this.InviteTextPanel.Padding = new System.Windows.Forms.Padding(16, 15, 16, 0);
            this.InviteTextPanel.Size = new System.Drawing.Size(417, 136);
            this.InviteTextPanel.TabIndex = 0;
            // 
            // _keyShareInvitePromptlabel
            // 
            this._keyShareInvitePromptlabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._keyShareInvitePromptlabel.Location = new System.Drawing.Point(8, 11);
            this._keyShareInvitePromptlabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._keyShareInvitePromptlabel.Name = "_keyShareInvitePromptlabel";
            this._keyShareInvitePromptlabel.Size = new System.Drawing.Size(401, 114);
            this._keyShareInvitePromptlabel.TabIndex = 1;
            this._keyShareInvitePromptlabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._keyShareInvitePromptlabel.Text = "[You are about to share securely with someone who is not having an AxCrypt user.AxCrypt will send the invitation mail with instructions on how to proceed the recipient."
             + "You can customized the invitation by providing the following fields]";
            // 
            // LanguageCulturePanel
            // 
            this.LanguageCulturePanel.Controls.Add(this._languageCultureGroupBox);
            this.LanguageCulturePanel.Location = new System.Drawing.Point(4, 148);
            this.LanguageCulturePanel.Margin = new System.Windows.Forms.Padding(4);
            this.LanguageCulturePanel.Name = "LanguageCulturePanel";
            this.LanguageCulturePanel.Padding = new System.Windows.Forms.Padding(16, 15, 16, 0);
            this.LanguageCulturePanel.Size = new System.Drawing.Size(417, 92);
            this.LanguageCulturePanel.TabIndex = 1;
            // 
            // _languageCultureGroupBox
            // 
            this._languageCultureGroupBox.AutoSize = true;
            this._languageCultureGroupBox.Controls.Add(this._languageCultureDropDown);
            this._languageCultureGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._languageCultureGroupBox.Location = new System.Drawing.Point(16, 15);
            this._languageCultureGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this._languageCultureGroupBox.Name = "_languageCultureGroupBox";
            this._languageCultureGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this._languageCultureGroupBox.Size = new System.Drawing.Size(385, 77);
            this._languageCultureGroupBox.TabIndex = 0;
            this._languageCultureGroupBox.TabStop = false;
            this._languageCultureGroupBox.Text = "[Choose Language]";
            // 
            // _languageCultureDropDown
            // 
            this._languageCultureDropDown.DisplayMember = "Name";
            this._languageCultureDropDown.FormattingEnabled = true;
            this._languageCultureDropDown.Location = new System.Drawing.Point(10, 34);
            this._languageCultureDropDown.Name = "_languageCultureDropDown";
            this._languageCultureDropDown.Size = new System.Drawing.Size(363, 24);
            this._languageCultureDropDown.TabIndex = 0;
            this._languageCultureDropDown.ValueMember = "Tag";
            // 
            // PersonalizedMessagePanel
            // 
            this.PersonalizedMessagePanel.Controls.Add(this._personalizedMessageGroupBox);
            this.PersonalizedMessagePanel.Location = new System.Drawing.Point(4, 248);
            this.PersonalizedMessagePanel.Margin = new System.Windows.Forms.Padding(4);
            this.PersonalizedMessagePanel.Name = "PersonalizedMessagePanel";
            this.PersonalizedMessagePanel.Padding = new System.Windows.Forms.Padding(16, 15, 16, 0);
            this.PersonalizedMessagePanel.Size = new System.Drawing.Size(417, 117);
            this.PersonalizedMessagePanel.TabIndex = 2;
            // 
            // _personalizedMessageGroupBox
            // 
            this._personalizedMessageGroupBox.AutoSize = true;
            this._personalizedMessageGroupBox.Controls.Add(this._personalizedMessageTextBox);
            this._personalizedMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._personalizedMessageGroupBox.Location = new System.Drawing.Point(16, 15);
            this._personalizedMessageGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this._personalizedMessageGroupBox.Name = "_personalizedMessageGroupBox";
            this._personalizedMessageGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this._personalizedMessageGroupBox.Size = new System.Drawing.Size(385, 102);
            this._personalizedMessageGroupBox.TabIndex = 0;
            this._personalizedMessageGroupBox.TabStop = false;
            this._personalizedMessageGroupBox.Text = "[Invitation Personalized Message]";
            // 
            // _personalizedMessageTextBox
            // 
            this._personalizedMessageTextBox.Location = new System.Drawing.Point(10, 25);
            this._personalizedMessageTextBox.Margin = new System.Windows.Forms.Padding(4);
            this._personalizedMessageTextBox.Multiline = true;
            this._personalizedMessageTextBox.Name = "_personalizedMessageTextBox";
            this._personalizedMessageTextBox.Size = new System.Drawing.Size(363, 57);
            this._personalizedMessageTextBox.TabIndex = 0;
            // 
            // ButtonPanel
            // 
            this.ButtonPanel.Controls.Add(this._okButton);
            this.ButtonPanel.Controls.Add(this._cancelButton);
            this.ButtonPanel.Location = new System.Drawing.Point(4, 373);
            this.ButtonPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ButtonPanel.Name = "ButtonPanel";
            this.ButtonPanel.Padding = new System.Windows.Forms.Padding(16, 15, 16, 0);
            this.ButtonPanel.Size = new System.Drawing.Size(417, 49);
            this.ButtonPanel.TabIndex = 3;
            // 
            // _cancelButton
            // 
            this._cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._cancelButton.Location = new System.Drawing.Point(219, 11);
            this._cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(100, 28);
            this._cancelButton.TabIndex = 1;
            this._cancelButton.Text = "[Cancel]";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // KeySharingInviteUserDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(426, 433);
            this.Controls.Add(this.DialogFlowLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeySharingInviteUserDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[Invite User Message]";
            this.DialogFlowLayoutPanel.ResumeLayout(false);
            this.InviteTextPanel.ResumeLayout(false);
            this.LanguageCulturePanel.ResumeLayout(false);
            this.LanguageCulturePanel.PerformLayout();
            this._languageCultureGroupBox.ResumeLayout(false);
            this.PersonalizedMessagePanel.ResumeLayout(false);
            this.PersonalizedMessagePanel.PerformLayout();
            this._personalizedMessageGroupBox.ResumeLayout(false);
            this._personalizedMessageGroupBox.PerformLayout();
            this.ButtonPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel LanguageCulturePanel;
        private System.Windows.Forms.Panel InviteTextPanel;
        private System.Windows.Forms.FlowLayoutPanel DialogFlowLayoutPanel;
        private System.Windows.Forms.Panel ButtonPanel;
        private System.Windows.Forms.Panel PersonalizedMessagePanel;
        private System.Windows.Forms.GroupBox _personalizedMessageGroupBox;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.GroupBox _languageCultureGroupBox;
        private System.Windows.Forms.TextBox _personalizedMessageTextBox;
        private System.Windows.Forms.ComboBox _languageCultureDropDown;
        private System.Windows.Forms.Label _keyShareInvitePromptlabel;
    }
}