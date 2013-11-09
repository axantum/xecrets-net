namespace Axantum.AxCrypt
{
    partial class DebugOptionsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DebugOptionsDialog));
            this._label1 = new System.Windows.Forms.Label();
            this.UpdateCheckServiceUrl = new System.Windows.Forms.TextBox();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this._label1, "label1");
            this._label1.Name = "label1";
            // 
            // UpdateCheckServiceUrl
            // 
            resources.ApplyResources(this.UpdateCheckServiceUrl, "UpdateCheckServiceUrl");
            this.UpdateCheckServiceUrl.Name = "UpdateCheckServiceUrl";
            this.UpdateCheckServiceUrl.Validating += new System.ComponentModel.CancelEventHandler(this.UpdateCheckServiceUrl_Validating);
            this.UpdateCheckServiceUrl.Validated += new System.EventHandler(this.UpdateCheckServiceUrl_Validated);
            // 
            // okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._okButton, "okButton");
            this._okButton.Name = "okButton";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._cancelButton, "cancelButton");
            this._cancelButton.Name = "cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this._errorProvider1.ContainerControl = this;
            // 
            // DebugOptionsDialog
            // 
            this.AcceptButton = this._okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this.UpdateCheckServiceUrl);
            this.Controls.Add(this._label1);
            this.Name = "DebugOptionsDialog";
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _label1;
        internal System.Windows.Forms.TextBox UpdateCheckServiceUrl;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.ErrorProvider _errorProvider1;
    }
}