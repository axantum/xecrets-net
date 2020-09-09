namespace AxCrypt.Desktop.Window
{
    partial class DebugLogOutputDialog
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
            this._logOutputTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // _logOutputTextBox
            // 
            this._logOutputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._logOutputTextBox.Location = new System.Drawing.Point(0, 0);
            this._logOutputTextBox.Multiline = true;
            this._logOutputTextBox.Name = "_logOutputTextBox";
            this._logOutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._logOutputTextBox.Size = new System.Drawing.Size(773, 261);
            this._logOutputTextBox.TabIndex = 0;
            // 
            // DebugLogOutputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 261);
            this.Controls.Add(this._logOutputTextBox);
            this.Name = "DebugLogOutputDialog";
            this.Text = "Debug Log Output";
            this.Load += new System.EventHandler(this.DebugLogOutputDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox _logOutputTextBox;
    }
}