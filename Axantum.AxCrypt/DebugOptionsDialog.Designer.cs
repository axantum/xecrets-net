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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._restApiBaseUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._timeoutTimeSpan = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this._errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this._okButton, "_okButton");
            this._okButton.Name = "_okButton";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this._cancelButton, "_cancelButton");
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _restApiBaseUrl
            // 
            resources.ApplyResources(this._restApiBaseUrl, "_restApiBaseUrl");
            this._restApiBaseUrl.Name = "_restApiBaseUrl";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // _timeoutTimeSpan
            // 
            resources.ApplyResources(this._timeoutTimeSpan, "_timeoutTimeSpan");
            this._timeoutTimeSpan.Name = "_timeoutTimeSpan";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // _errorProvider3
            // 
            this._errorProvider3.ContainerControl = this;
            // 
            // DebugOptionsDialog
            // 
            this.AcceptButton = this._okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.Controls.Add(this._timeoutTimeSpan);
            this.Controls.Add(this.label2);
            this.Controls.Add(this._restApiBaseUrl);
            this.Controls.Add(this.label1);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Name = "DebugOptionsDialog";
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        internal System.Windows.Forms.TextBox _restApiBaseUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        internal System.Windows.Forms.TextBox _timeoutTimeSpan;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ErrorProvider _errorProvider3;
    }
}