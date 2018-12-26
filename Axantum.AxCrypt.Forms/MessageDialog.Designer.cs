namespace Axantum.AxCrypt.Forms
{
    partial class MessageDialog
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
            this._button0 = new System.Windows.Forms.Button();
            this._button1 = new System.Windows.Forms.Button();
            this._button2 = new System.Windows.Forms.Button();
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
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(292, 113);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // dontShowThisAgain
            // 
            this.dontShowThisAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.dontShowThisAgain.AutoSize = true;
            this.dontShowThisAgain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.dontShowThisAgain.Location = new System.Drawing.Point(3, 89);
            this.dontShowThisAgain.Name = "dontShowThisAgain";
            this.dontShowThisAgain.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.dontShowThisAgain.Size = new System.Drawing.Size(286, 17);
            this.dontShowThisAgain.TabIndex = 3;
            this.dontShowThisAgain.Text = "[Don\'t show this again]";
            this.dontShowThisAgain.UseVisualStyleBackColor = true;
            // 
            // Message
            // 
            this.Message.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Message.AutoSize = true;
            this.Message.Location = new System.Drawing.Point(21, 15);
            this.Message.Margin = new System.Windows.Forms.Padding(5);
            this.Message.MaximumSize = new System.Drawing.Size(500, 0);
            this.Message.MinimumSize = new System.Drawing.Size(250, 0);
            this.Message.Name = "Message";
            this.Message.Size = new System.Drawing.Size(250, 13);
            this.Message.TabIndex = 1;
            this.Message.Text = "[Text]";
            this.Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this._button0);
            this.flowLayoutPanel1.Controls.Add(this._button1);
            this.flowLayoutPanel1.Controls.Add(this._button2);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(24, 51);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(243, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // _button0
            // 
            this._button0.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._button0.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._button0.Location = new System.Drawing.Point(3, 3);
            this._button0.Name = "_button0";
            this._button0.Size = new System.Drawing.Size(75, 23);
            this._button0.TabIndex = 0;
            this._button0.Text = "[OK]";
            this._button0.UseVisualStyleBackColor = true;
            // 
            // _button1
            // 
            this._button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._button1.Location = new System.Drawing.Point(84, 3);
            this._button1.Name = "_button1";
            this._button1.Size = new System.Drawing.Size(80, 23);
            this._button1.TabIndex = 1;
            this._button1.Text = "[Cancel]";
            this._button1.UseVisualStyleBackColor = true;
            // 
            // _button2
            // 
            this._button2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._button2.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this._button2.Location = new System.Drawing.Point(170, 3);
            this._button2.Name = "_button2";
            this._button2.Size = new System.Drawing.Size(75, 23);
            this._button2.TabIndex = 2;
            this._button2.Text = "[Exit]";
            this._button2.UseVisualStyleBackColor = true;
            // 
            // MessageDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(292, 113);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "[Message Dialog]";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button _button0;
        private System.Windows.Forms.Button _button1;
        private System.Windows.Forms.Button _button2;
        internal System.Windows.Forms.Label Message;
        internal System.Windows.Forms.CheckBox dontShowThisAgain;
    }
}