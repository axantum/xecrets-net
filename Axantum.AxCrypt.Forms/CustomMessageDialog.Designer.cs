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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dontShowThisAgain = new System.Windows.Forms.CheckBox();
            this.Message = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this._customButton1 = new System.Windows.Forms.Button();
            this._customButton2 = new System.Windows.Forms.Button();
            this._customButton3 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dontShowThisAgain, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.Message, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(389, 139);
            this.tableLayoutPanel1.TabIndex = 2;
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._customButton1);
            this.flowLayoutPanel1.Controls.Add(this._customButton2);
            this.flowLayoutPanel1.Controls.Add(this._customButton3);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(32, 62);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(324, 36);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // _customButton1
            // 
            this._customButton1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._customButton1.Location = new System.Drawing.Point(4, 4);
            this._customButton1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customButton1.Name = "_customButton1";
            this._customButton1.Size = new System.Drawing.Size(100, 28);
            this._customButton1.TabIndex = 0;
            this._customButton1.Text = "[Custom Button]";
            this._customButton1.UseVisualStyleBackColor = true;
            // 
            // _customButton2
            // 
            this._customButton2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customButton2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._customButton2.Location = new System.Drawing.Point(112, 4);
            this._customButton2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customButton2.Name = "_customButton2";
            this._customButton2.Size = new System.Drawing.Size(100, 28);
            this._customButton2.TabIndex = 1;
            this._customButton2.Text = "[Custom Button]";
            this._customButton2.UseVisualStyleBackColor = true;
            // 
            // _customButton3
            // 
            this._customButton3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._customButton3.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this._customButton3.Location = new System.Drawing.Point(220, 4);
            this._customButton3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this._customButton3.Name = "_customButton3";
            this._customButton3.Size = new System.Drawing.Size(100, 28);
            this._customButton3.TabIndex = 2;
            this._customButton3.Text = "[Custom Button]";
            this._customButton3.UseVisualStyleBackColor = true;
            // 
            // CustomMessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(389, 139);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMessageDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[Custom Message Dialog]";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button _customButton1;
        private System.Windows.Forms.Button _customButton2;
        private System.Windows.Forms.Button _customButton3;
        internal System.Windows.Forms.Label Message;
        internal System.Windows.Forms.CheckBox dontShowThisAgain;
    }
}