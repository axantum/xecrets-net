

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace AxCrypt.Desktop.Window
{
    partial class KeyShareDialog
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeyShareDialog));
            this._shareKeyAddUserInfo = new System.Windows.Forms.LinkLabel();
            this._sharedKeyUsersListTitle = new System.Windows.Forms.Label();
            this._addNewUserTextBox = new System.Windows.Forms.TextBox();
            this._addButton = new System.Windows.Forms.Button();
            this._applyButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._sharedWithUsersListLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._selectedFileListLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._selectedSharedKeyUserPopupLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this._shareAccessTitlePanel = new System.Windows.Forms.Panel();
            this._sharedWithUsersListLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this._shareAccessTitle = new System.Windows.Forms.Label();
            this._moreFilesInfo = new System.Windows.Forms.Label();
            this._userEmailAutoSuggestionLayout = new System.Windows.Forms.FlowLayoutPanel();
            this._userEmailAutoSuggestionLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();

            // 
            // _shareAccessTitle
            // 
            this._shareAccessTitle.AutoSize = true;
            this._shareAccessTitle.Font = new System.Drawing.Font("Open Sans", 10.8F, System.Drawing.FontStyle.Bold);
            this._shareAccessTitle.ForeColor = System.Drawing.Color.White;
            this._shareAccessTitle.Location = new System.Drawing.Point(250, 2);
            this._shareAccessTitle.Name = "_shareAccessTitle";
            this._shareAccessTitle.Size = new System.Drawing.Size(250, 24);
            this._shareAccessTitle.TabIndex = 0;
            this._shareAccessTitle.Text = "[Share access]";
            // 
            // _shareAccessTitlePanel
            // 
            this._shareAccessTitlePanel.Controls.Add(this._shareAccessTitle);
            this._shareAccessTitlePanel.BackColor = System.Drawing.Color.FromArgb(72, 119, 44);
            this._shareAccessTitlePanel.Location = new System.Drawing.Point(0, 0);
            this._shareAccessTitlePanel.Name = "_shareAccessTitlePanel";
            this._shareAccessTitlePanel.Size = new System.Drawing.Size(670, 25);
            this._shareAccessTitlePanel.TabIndex = 7;
            this._shareAccessTitlePanel.Paint += DrawBorder;
            // 
            // _shareKeyAddUserInfo
            // 
            this._shareKeyAddUserInfo.Font = FormStyles.OpenSans12Regular;
            this._shareKeyAddUserInfo.ForeColor = FormStyles.ColTxtBusBd;
            this._shareKeyAddUserInfo.AutoSize = true;
            this._shareKeyAddUserInfo.Location = new System.Drawing.Point(55, 40);
            this._shareKeyAddUserInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this._shareKeyAddUserInfo.Name = "_shareKeyAddUserInfo";
            this._shareKeyAddUserInfo.MaximumSize = new System.Drawing.Size(550, 50);
            this._shareKeyAddUserInfo.Text = "Add User(s) or Group(s) with whom you want to share the file(s). If you don`t know about the Groups yet, learn more and create your first one by the link Create a Group!";
            this._shareKeyAddUserInfo.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this._shareKeyAddUserInfo.LinkColor = FormStyles.ColHglhgt;
            // 
            // _addNewUserTextBox
            // 
            this._addNewUserTextBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this._addNewUserTextBox.Multiline = true;
            this._addNewUserTextBox.MinimumSize = new System.Drawing.Size(425, 25);
            this._addNewUserTextBox.Size = new System.Drawing.Size(425, 27);
            this._addNewUserTextBox.Location = new System.Drawing.Point(55, 97);
            this._addNewUserTextBox.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
            this._addNewUserTextBox.Name = "_addNewUserTextBox";
            this._addNewUserTextBox.TabIndex = 1;
            this._addNewUserTextBox.Text = "Type user email,or a group name";
            // 
            // _userEmailAutoSuggestionLayoutPanel
            // 
            this._userEmailAutoSuggestionLayoutPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._userEmailAutoSuggestionLayoutPanel.BackColor = FormStyles.LightGreyColor;
            this._userEmailAutoSuggestionLayoutPanel.Location = new System.Drawing.Point(55, 122);
            this._userEmailAutoSuggestionLayoutPanel.Name = "_userEmailAutoSuggestionLayoutPanel";
            this._userEmailAutoSuggestionLayoutPanel.Size = new System.Drawing.Size(425, 165);
            this._userEmailAutoSuggestionLayoutPanel.Controls.Add(_userEmailAutoSuggestionLayout);
            this._userEmailAutoSuggestionLayoutPanel.Visible = false;
            // 
            // _userEmailAutoSuggestionLayout
            // 
            this._userEmailAutoSuggestionLayout.AutoScroll = true;
            this._userEmailAutoSuggestionLayout.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._userEmailAutoSuggestionLayout.BackColor = System.Drawing.Color.White;
            this._userEmailAutoSuggestionLayout.Name = "_userEmailAutoSuggestionLayout";
            this._userEmailAutoSuggestionLayout.Margin = new Padding(1, 1, 1, 1);
            this._userEmailAutoSuggestionLayout.Size = new System.Drawing.Size(423, 163);
            // 
            // _errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // _addButton
            // 
            this._addButton.BackColor = System.Drawing.Color.FromArgb(72, 119, 44);
            this._addButton.Font = FormStyles.OpenSans12Bold;
            this._addButton.Location = new System.Drawing.Point(499, 97);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new System.Drawing.Size(100, 27);
            this._addButton.TabIndex = 2;
            this._addButton.Text = "[Add]";
            this._addButton.UseVisualStyleBackColor = true;
            this._addButton.Enabled = true;
            // 
            // _selectedFileListLayout
            // 
            this._selectedFileListLayout.AutoScroll = true;
            this._selectedFileListLayout.Location = new System.Drawing.Point(55, 132);
            this._selectedFileListLayout.Name = "_selectedFileListLayout";
            this._selectedFileListLayout.Size = new System.Drawing.Size(550, 35);
            // 
            // _moreFilesInfo
            // 
            this._moreFilesInfo.AutoSize = true;
            this._moreFilesInfo.BackColor = FormStyles.GreenColor;
            this._moreFilesInfo.ForeColor = System.Drawing.Color.Black;
            this._moreFilesInfo.Location = new System.Drawing.Point(490, 165);
            this._moreFilesInfo.Name = "_moreFilesInfo";
            this._moreFilesInfo.Size = new System.Drawing.Size(101, 17);
            this._moreFilesInfo.TabIndex = 9;
            this._moreFilesInfo.Text = "[+ 0 more files]";
            this._moreFilesInfo.Visible = false;
            // 
            // _sharedKeyUsersListTitle
            // 
            this._sharedKeyUsersListTitle.AutoSize = true;
            this._sharedKeyUsersListTitle.Font = FormStyles.OpenSans12Bold;
            this._sharedKeyUsersListTitle.ForeColor = System.Drawing.Color.FromArgb(1, 72, 119, 44);
            this._sharedKeyUsersListTitle.Location = new System.Drawing.Point(55, 182);
            this._sharedKeyUsersListTitle.Name = "_sharedKeyUsersListTitle";
            this._sharedKeyUsersListTitle.Size = new System.Drawing.Size(387, 24);
            this._sharedKeyUsersListTitle.TabIndex = 0;
            this._sharedKeyUsersListTitle.Text = "[All users with access in selected files (0)]";
            // 
            // _sharedWithUsersListLayoutPanel
            // 
            this._sharedWithUsersListLayoutPanel.AutoScroll = false;
            this._sharedWithUsersListLayoutPanel.AutoSize = true;
            this._sharedWithUsersListLayoutPanel.Dock = DockStyle.None;
            this._sharedWithUsersListLayoutPanel.BackColor = System.Drawing.Color.White;
            this._sharedWithUsersListLayoutPanel.Location = new System.Drawing.Point(55, 215);
            this._sharedWithUsersListLayoutPanel.Name = "_sharedWithUsersListLayoutPanel";
            this._sharedWithUsersListLayoutPanel.Size = new System.Drawing.Size(530, 180);
            this._sharedWithUsersListLayoutPanel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            this._sharedWithUsersListLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            this._sharedWithUsersListLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this._sharedWithUsersListLayoutPanel.Controls.Add(this._sharedWithUsersListLayout);
            // 
            // _sharedWithUsersListLayout
            // 
            this._sharedWithUsersListLayout.BackColor = Color.White;
            this._sharedWithUsersListLayout.Name = "_sharedWithUsersListLayout";
            this._sharedWithUsersListLayout.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this._sharedWithUsersListLayout.Padding = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this._sharedWithUsersListLayout.AutoScroll = true;
            this._sharedWithUsersListLayout.Size = new System.Drawing.Size(530, 180);
            // 
            // _selectedSharedKeyUserPopupLayout
            // 
            this._selectedSharedKeyUserPopupLayout.ForeColor = System.Drawing.Color.Black;
            this._selectedSharedKeyUserPopupLayout.BackColor = System.Drawing.Color.White;
            this._selectedSharedKeyUserPopupLayout.Name = "_selectedSharedKeyUserPopupLayout";
            this._selectedSharedKeyUserPopupLayout.Size = new System.Drawing.Size(130, 64);
            this._selectedSharedKeyUserPopupLayout.Visible = false;
            // 
            // _cancelButton
            // 
            this._cancelButton.BackColor = System.Drawing.Color.White;
            this._cancelButton.ForeColor = Color.FromArgb(26, 26, 26);
            this._cancelButton.FlatAppearance.BorderColor = Color.FromArgb(153, 153, 153);
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(55, 422);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(95, 28);
            this._cancelButton.TabIndex = 9;
            this._cancelButton.Text = "[Cancel]";
            this._cancelButton.UseVisualStyleBackColor = false;
            // 
            // _applyButton
            // 
            this._applyButton.BackColor = System.Drawing.Color.FromArgb(1, 72, 119, 44);
            this._applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._applyButton.Location = new System.Drawing.Point(160, 422);
            this._applyButton.Name = "_applyButton";
            this._applyButton.Size = new System.Drawing.Size(95, 28);
            this._applyButton.TabIndex = 8;
            this._applyButton.Text = "[Apply]";
            this._applyButton.UseVisualStyleBackColor = true;
            this._applyButton.Enabled = false;
            // 
            // KeyShareDialogRedesign
            // 
            this.AcceptButton = this._addButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(247, 247, 247);
            this.ClientSize = new System.Drawing.Size(670, 480);
            this.MinimumSize = new System.Drawing.Size(670, 480);
            this.Controls.Add(this._shareAccessTitlePanel);
            this.Controls.Add(this._shareKeyAddUserInfo);
            this.Controls.Add(this._selectedFileListLayout);
            this.Controls.Add(this._moreFilesInfo);
            this.Controls.Add(this._selectedSharedKeyUserPopupLayout);
            this.Controls.Add(this._sharedKeyUsersListTitle);
            this.Controls.Add(this._addNewUserTextBox);
            this.Controls.Add(this._userEmailAutoSuggestionLayoutPanel);
            this.Controls.Add(this._addButton);
            this.Controls.Add(this._sharedWithUsersListLayoutPanel);
            this.Controls.Add(this._applyButton);
            this.Controls.Add(this._cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoSize = false;
            this.Name = "KeyShareDialogRedesign";
            this.Text = "[Share access]";
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.LinkLabel _shareKeyAddUserInfo;
        private System.Windows.Forms.TextBox _addNewUserTextBox;
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _sharedKeyUsersListTitle;
        private System.Windows.Forms.FlowLayoutPanel _sharedWithUsersListLayout;
        private System.Windows.Forms.FlowLayoutPanel _selectedFileListLayout;
        private System.Windows.Forms.FlowLayoutPanel _selectedSharedKeyUserPopupLayout;
        private System.Windows.Forms.FlowLayoutPanel _userEmailAutoSuggestionLayout;
        private System.Windows.Forms.FlowLayoutPanel _userEmailAutoSuggestionLayoutPanel;
        private Panel _shareAccessTitlePanel;
        private TableLayoutPanel _sharedWithUsersListLayoutPanel;
        private Label _shareAccessTitle;
        private Label _moreFilesInfo;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
    }

}