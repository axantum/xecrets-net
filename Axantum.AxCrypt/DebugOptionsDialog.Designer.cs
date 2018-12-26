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
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._restApiBaseUrl = new System.Windows.Forms.TextBox();
            this._restApiBaseUrlLabel = new System.Windows.Forms.Label();
            this._errorProvider2 = new System.Windows.Forms.ErrorProvider(this.components);
            this._timeoutTimeSpan = new System.Windows.Forms.TextBox();
            this._restApiTimeoutLabel = new System.Windows.Forms.Label();
            this._errorProvider3 = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).BeginInit();
            this.SuspendLayout();
            // 
            // _okButton
            // 
            this._okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._okButton.Location = new System.Drawing.Point(231, 88);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(80, 23);
            this._okButton.TabIndex = 2;
            this._okButton.Text = "[OK]";
            this._okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            this._cancelButton.CausesValidation = false;
            this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._cancelButton.Location = new System.Drawing.Point(322, 88);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(80, 23);
            this._cancelButton.TabIndex = 3;
            this._cancelButton.Text = "[Cancel]";
            this._cancelButton.UseVisualStyleBackColor = true;
            // 
            // _restApiBaseUrl
            // 
            this._restApiBaseUrl.Location = new System.Drawing.Point(169, 10);
            this._restApiBaseUrl.Name = "_restApiBaseUrl";
            this._restApiBaseUrl.Size = new System.Drawing.Size(441, 20);
            this._restApiBaseUrl.TabIndex = 5;
            this._restApiBaseUrl.Validating += new System.ComponentModel.CancelEventHandler(this.RestApiBaseUrl_Validating);
            this._restApiBaseUrl.Validated += new System.EventHandler(this.RestApiBaseUrl_Validated);
            // 
            // _restApiBaseUrlLabel
            // 
            this._restApiBaseUrlLabel.AutoSize = true;
            this._restApiBaseUrlLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._restApiBaseUrlLabel.Location = new System.Drawing.Point(12, 13);
            this._restApiBaseUrlLabel.Name = "_restApiBaseUrlLabel";
            this._restApiBaseUrlLabel.Size = new System.Drawing.Size(105, 13);
            this._restApiBaseUrlLabel.TabIndex = 4;
            this._restApiBaseUrlLabel.Text = "[REST API Base Url]";
            // 
            // _errorProvider2
            // 
            this._errorProvider2.ContainerControl = this;
            // 
            // _timeoutTimeSpan
            // 
            this._timeoutTimeSpan.Location = new System.Drawing.Point(169, 46);
            this._timeoutTimeSpan.Name = "_timeoutTimeSpan";
            this._timeoutTimeSpan.Size = new System.Drawing.Size(166, 20);
            this._timeoutTimeSpan.TabIndex = 7;
            // 
            // _restApiTimeoutLabel
            // 
            this._restApiTimeoutLabel.AutoSize = true;
            this._restApiTimeoutLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._restApiTimeoutLabel.Location = new System.Drawing.Point(12, 49);
            this._restApiTimeoutLabel.Name = "_restApiTimeoutLabel";
            this._restApiTimeoutLabel.Size = new System.Drawing.Size(71, 13);
            this._restApiTimeoutLabel.TabIndex = 6;
            this._restApiTimeoutLabel.Text = "[API Timeout]";
            // 
            // _errorProvider3
            // 
            this._errorProvider3.ContainerControl = this;
            // 
            // DebugOptionsDialog
            // 
            this.AcceptButton = this._okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this._cancelButton;
            this.ClientSize = new System.Drawing.Size(628, 129);
            this.Controls.Add(this._timeoutTimeSpan);
            this.Controls.Add(this._restApiTimeoutLabel);
            this.Controls.Add(this._restApiBaseUrl);
            this.Controls.Add(this._restApiBaseUrlLabel);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._okButton);
            this.Name = "DebugOptionsDialog";
            this.Text = "[Debugging Options]";
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._errorProvider3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
        internal System.Windows.Forms.TextBox _restApiBaseUrl;
        private System.Windows.Forms.Label _restApiBaseUrlLabel;
        private System.Windows.Forms.ErrorProvider _errorProvider2;
        internal System.Windows.Forms.TextBox _timeoutTimeSpan;
        private System.Windows.Forms.Label _restApiTimeoutLabel;
        private System.Windows.Forms.ErrorProvider _errorProvider3;
    }
}