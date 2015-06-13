using Axantum.AxCrypt.Core;
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
            new Styling().Style(this);
        }

        private void UpdateCheckServiceUrl_Validating(object sender, CancelEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(UpdateCheckServiceUrl.Text, UriKind.Absolute))
            {
                e.Cancel = true;
                UpdateCheckServiceUrl.SelectAll();
                _errorProvider1.SetError(UpdateCheckServiceUrl, Axantum.AxCrypt.Properties.Resources.Invalid_URL);
            }
        }

        private void UpdateCheckServiceUrl_Validated(object sender, EventArgs e)
        {
            _errorProvider1.SetError(UpdateCheckServiceUrl, String.Empty);
        }
    }
}