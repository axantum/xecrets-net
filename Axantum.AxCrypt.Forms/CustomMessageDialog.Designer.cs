namespace Axantum.AxCrypt.Forms
{
    partial class CustomMessageDialog
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.dontShowThisAgain = new System.Windows.Forms.CheckBox();
            this.Message = new System.Windows.Forms.Label();
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._customOkButton = new System.Windows.Forms.Button();
            this._customCancelButton = new System.Windows.Forms.Button();
            this._customAbortButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            this.flowLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.dontShowThisAgain, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.Message, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.flowLayoutPanel, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(389, 139);
            this.tableLayoutPanel.TabIndex = 2;
            // 
            // dontShowThisAgain
            // 
            this.dontShowThisAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dontShowThisAgain.AutoSize = true;
            this.dontShowThisAgain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dontShowThisAgain.Location = new System.Drawing.Point(4, 110);
            this.dontShowThisAgain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dontShowThisAgain.Name = "dontShowThisAgain";
            this.dontShowThisAgain.Padding = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.dontShowThisAgain.Size = new System.Drawing.Size(381, 21);
            this.dontShowThisAgain.TabIndex = 3;
            this.dontShowThisAgain.Text = "[Don\'t show this again]";
            this.dontShowThisAgain.UseVisualStyleBackColor = true;
            // 
            // Message
            // 
            this.Message.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(28, 18);
            this.Message.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.Message.MaximumSize = new System.Drawing.Size(333, 0);
            this.Message.MinimumSize = new System.Drawing.Size(333, 0);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(333, 17);
            this.Message.TabIndex = 1;
            this.Message.Text = "[Text]";
            this.Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel.AutoSize = true;
            this.flowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel.Controls.Add(this._customOkButton);
            this.flowLayoutPanel.Controls.Add(this._customCancelButton);
            this.flowLayoutPanel.Controls.Add(this._customAbortButton);
            this.flowLayoutPanel.Location = new System.Drawing.Point(32, 62);
            this.flowLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanel.Name = "flowLayoutPanel1";
            this.flowLayoutPanel.Size = new System.Drawing.Size(324, 36);
            this.flowLayoutPanel.TabIndex = 2;
            // 
            // _customOkButton
            // 
            this._customOkButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customOkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._customOkButton.Location = new System.Drawing.Point(4, 4);
            this._customOkButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customOkButton.Name = "_customOkButton";
            this._customOkButton.Size = new System.Drawing.Size(100, 28);
            this._customOkButton.TabIndex = 0;
            this._customOkButton.Text = "[Ok]";
            this._customOkButton.UseVisualStyleBackColor = true;
            // 
            // _customCancelButton
            // 
            this._customCancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._customCancelButton.Location = new System.Drawing.Point(112, 4);
            this._customCancelButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customCancelButton.Name = "_customCancelButton";
            this._customCancelButton.Size = new System.Drawing.Size(100, 28);
            this._customCancelButton.TabIndex = 1;
            this._customCancelButton.Text = "[Cancel]";
            this._customCancelButton.UseVisualStyleBackColor = true;
            // 
            // _customAbortButton
            // 
            this._customAbortButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customAbortButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this._customAbortButton.Location = new System.Drawing.Point(220, 4);
            this._customAbortButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customAbortButton.Name = "_customAbortButton";
            this._customAbortButton.Size = new System.Drawing.Size(100, 28);
            this._customAbortButton.TabIndex = 2;
            this._customAbortButton.Text = "[Abort]";
            this._customAbortButton.UseVisualStyleBackColor = true;
            // 
            // CustomMessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(389, 139);
            this.Controls.Add(this.tableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMessageDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[Custom Message Dialog]";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.flowLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Button _customOkButton;
        private System.Windows.Forms.Button _customCancelButton;
        private System.Windows.Forms.Button _customAbortButton;
        internal System.Windows.Forms.Label Message;
        internal System.Windows.Forms.CheckBox dontShowThisAgain;
    }
}