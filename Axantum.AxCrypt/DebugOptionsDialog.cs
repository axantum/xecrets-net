using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

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
                _errorProvider1.SetError(_restApiBaseUrl, Axantum.AxCrypt.Properties.Resources.Invalid_URL);
            }
        }

        private void RestApiBaseUrl_Validated(object sender, EventArgs e)
        {
            _errorProvider1.SetError(_restApiBaseUrl, String.Empty);
        }
    }
}