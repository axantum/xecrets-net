using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

using Content = AxCrypt.Content.Content;

namespace Axantum.AxCrypt
{
    public partial class DebugOptionsDialog : StyledMessageBase
    {
        public DebugOptionsDialog()
        {
            InitializeComponent();
        }

        public DebugOptionsDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            Text = Content.DialogDebugLogTitle;

            _okButton.Text = Content.ButtonOkText;
            _cancelButton.Text = Content.ButtonCancelText;
            _restApiBaseUrlLabel.Text = Content.DialogDebugOptionsRestApiUrlPrompt;
            _restApiTimeoutLabel.Text = Content.DialogDebugOptionsRestApiTimeoutPrompt;
        }

        private void RestApiBaseUrl_Validating(object sender, CancelEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(_restApiBaseUrl.Text, UriKind.Absolute))
            {
                e.Cancel = true;
                _restApiBaseUrl.SelectAll();
                _errorProvider2.SetError(_restApiBaseUrl, Content.Invalid_URL);
            }
            TimeSpan timeout;
            if (!TimeSpan.TryParse(_timeoutTimeSpan.Text, out timeout))
            {
                e.Cancel = true;
                _timeoutTimeSpan.SelectAll();
                _errorProvider3.SetError(_timeoutTimeSpan, Content.Invalid_TimeSpan);
            }
        }

        private void RestApiBaseUrl_Validated(object sender, EventArgs e)
        {
            _errorProvider2.SetError(_restApiBaseUrl, String.Empty);
            _errorProvider3.SetError(_timeoutTimeSpan, String.Empty);
        }
    }
}