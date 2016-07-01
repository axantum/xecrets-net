using Axantum.AxCrypt.Forms;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

using Texts = AxCrypt.Content.Texts;

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
            Text = Texts.DialogDebugLogTitle;

            _okButton.Text = Texts.ButtonOkText;
            _cancelButton.Text = Texts.ButtonCancelText;
            _restApiBaseUrlLabel.Text = Texts.DialogDebugOptionsRestApiUrlPrompt;
            _restApiTimeoutLabel.Text = Texts.DialogDebugOptionsRestApiTimeoutPrompt;
        }

        private void RestApiBaseUrl_Validating(object sender, CancelEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(_restApiBaseUrl.Text, UriKind.Absolute))
            {
                e.Cancel = true;
                _restApiBaseUrl.SelectAll();
                _errorProvider2.SetError(_restApiBaseUrl, Texts.Invalid_URL);
            }
            TimeSpan timeout;
            if (!TimeSpan.TryParse(_timeoutTimeSpan.Text, out timeout))
            {
                e.Cancel = true;
                _timeoutTimeSpan.SelectAll();
                _errorProvider3.SetError(_timeoutTimeSpan, Texts.Invalid_TimeSpan);
            }
        }

        private void RestApiBaseUrl_Validated(object sender, EventArgs e)
        {
            _errorProvider2.SetError(_restApiBaseUrl, String.Empty);
            _errorProvider3.SetError(_timeoutTimeSpan, String.Empty);
        }
    }
}