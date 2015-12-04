using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

using Content = AxCrypt.Content.Content;

namespace Axantum.AxCrypt
{
    public partial class DebugOptionsDialog : Form
    {
        public DebugOptionsDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
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