namespace Axantum.AxCrypt
{
    partial class ManageAccountDialog
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
            this._changePassphraseButton = new System.Windows.Forms.Button();
            this._accountEmailsListView = new System.Windows.Forms.ListView();
            this._emailHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._dateHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // _changePassphraseButton
            // 
            this._changePassphraseButton.Location = new System.Drawing.Point(75, 118);
            this._changePassphraseButton.Name = "_changePassphraseButton";
            this._changePassphraseButton.Size = new System.Drawing.Size(135, 23);
            this._changePassphraseButton.TabIndex = 1;
            this._changePassphraseButton.Text = "Change Passphrase";
            this._changePassphraseButton.UseVisualStyleBackColor = true;
            this._changePassphraseButton.Click += new System.EventHandler(this._changePassphraseButton_Click);
            // 
            // _accountEmailsListView
            // 
            this._accountEmailsListView.AutoArrange = false;
            this._accountEmailsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._emailHeader,
            this._dateHeader});
            this._accountEmailsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._accountEmailsListView.Location = new System.Drawing.Point(12, 13);
            this._accountEmailsListView.Name = "_accountEmailsListView";
            this._accountEmailsListView.ShowGroups = false;
            this._accountEmailsListView.Size = new System.Drawing.Size(260, 97);
            this._accountEmailsListView.TabIndex = 2;
            this._accountEmailsListView.UseCompatibleStateImageBehavior = false;
            this._accountEmailsListView.View = System.Windows.Forms.View.Details;
            // 
            // _emailHeader
            // 
            this._emailHeader.Text = "Email";
            // 
            // _dateHeader
            // 
            this._dateHeader.Text = "Timestamp";
            // 
            // ManageAccountDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 153);
            this.Controls.Add(this._accountEmailsListView);
            this.Controls.Add(this._changePassphraseButton);
            this.Name = "ManageAccountDialog";
            this.Text = "Manage Account";
            this.Load += new System.EventHandler(this.ManageAccountDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button _changePassphraseButton;
        private System.Windows.Forms.ListView _accountEmailsListView;
        private System.Windows.Forms.ColumnHeader _emailHeader;
        private System.Windows.Forms.ColumnHeader _dateHeader;

    }
}