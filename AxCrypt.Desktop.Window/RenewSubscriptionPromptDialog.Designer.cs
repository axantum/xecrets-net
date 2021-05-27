
namespace AxCrypt.Desktop.Window
{
    partial class RenewSubscriptionPromptDialog
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
			this._panelContent = new System.Windows.Forms.Panel();
			this._labelPromptText = new System.Windows.Forms.Label();
			this._panelAction = new System.Windows.Forms.Panel();
			this._buttonRenewNow = new System.Windows.Forms.Button();
			this._buttonNotNow = new System.Windows.Forms.Button();
			this._checkboxDontShowThisAgain = new System.Windows.Forms.CheckBox();

			this._panelContent.SuspendLayout();
			this.SuspendLayout();
			// 
			// _panelContent
			// 
			this._panelContent.Location = new System.Drawing.Point(10, 10);
			this._panelContent.Controls.Add(this._labelPromptText);
			this._panelContent.Name = "contentPanel";
			this._panelContent.Size = new System.Drawing.Size(540, 190);
			this._panelContent.TabIndex = 3;
			// 
			// _labelPromptText
			// 
			this._labelPromptText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._labelPromptText.Name = "_labelPromptText";
			this._labelPromptText.Size = new System.Drawing.Size(540, 190);
			this._labelPromptText.TabIndex = 4;
			this._labelPromptText.Text = "[Your subscription has expired. Renew today to keep using premium features like: \n \n Mobile apps \n 256-bit AES \n Master Key \n Key Sharing \n Secured Folders \n Password Management \n Cloud Storage Awareness]";
			this._labelPromptText.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// _panelAction
			// 
			_panelAction.Location = new System.Drawing.Point(10, 200);
			_panelAction.Controls.Add(this._buttonRenewNow);
			_panelAction.Controls.Add(this._buttonNotNow);
			_panelAction.Controls.Add(this._checkboxDontShowThisAgain);
			_panelAction.Name = "_panelAction";
			_panelAction.Size = new System.Drawing.Size(540, 80);
			_panelAction.TabIndex = 5;
			// 
			// _buttonRenewNow
			// 
			this._buttonRenewNow.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this._buttonRenewNow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._buttonRenewNow.Location = new System.Drawing.Point(130, 0);
			this._buttonRenewNow.Name = "_buttonRenewNow";
			this._buttonRenewNow.Size = new System.Drawing.Size(125, 35);
			this._buttonRenewNow.TabIndex = 0;
			this._buttonRenewNow.Text = "[Renew]";
			this._buttonRenewNow.UseVisualStyleBackColor = true;
			// 
			// _buttonCancel
			// 
			this._buttonNotNow.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this._buttonNotNow.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._buttonNotNow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._buttonNotNow.Location = new System.Drawing.Point(290, 0);
			this._buttonNotNow.Name = "_buttonNotNow";
			this._buttonNotNow.Size = new System.Drawing.Size(125, 35);
			this._buttonNotNow.TabIndex = 1;
			this._buttonNotNow.Text = "[NotNow]";
			this._buttonNotNow.UseVisualStyleBackColor = true;
			// 
			// dontShowThisAgain
			// 
			this._checkboxDontShowThisAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this._checkboxDontShowThisAgain.AutoSize = true;
			this._checkboxDontShowThisAgain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._checkboxDontShowThisAgain.Location = new System.Drawing.Point(3, 50);
			this._checkboxDontShowThisAgain.Name = "_checkboxDontShowThisAgain";
			this._checkboxDontShowThisAgain.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
			this._checkboxDontShowThisAgain.Size = new System.Drawing.Size(286, 17);
			this._checkboxDontShowThisAgain.TabIndex = 2;
			this._checkboxDontShowThisAgain.Text = "[Don\'t show this again]";
			this._checkboxDontShowThisAgain.UseVisualStyleBackColor = true;
			// 
			// InitializeComponent
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(550, 290);
			this.Controls.Add(this._panelContent);
			this.Controls.Add(this._panelAction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(530, 280);
			this.Name = "RenewSubscriptionPromptDialog";
			this.Text = "[Renew your subscription!]";
			_panelContent.ResumeLayout(false);
			_panelAction.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

        #endregion

        private System.Windows.Forms.Panel _panelContent;
        private System.Windows.Forms.Label _labelPromptText;
        private System.Windows.Forms.Panel _panelAction;
        private System.Windows.Forms.Button _buttonRenewNow;
        private System.Windows.Forms.Button _buttonNotNow;
        private System.Windows.Forms.CheckBox _checkboxDontShowThisAgain;
    }
}