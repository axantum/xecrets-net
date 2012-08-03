using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class DebugOptionsDialog : Form
    {
        public DebugOptionsDialog()
        {
            InitializeComponent();
        }

        private void UpdateCheckServiceUrl_Validating(object sender, CancelEventArgs e)
        {
            if (!Uri.IsWellFormedUriString(UpdateCheckServiceUrl.Text, UriKind.Absolute))
            {
                e.Cancel = true;
                UpdateCheckServiceUrl.SelectAll();
                errorProvider1.SetError(UpdateCheckServiceUrl, "Invalid URL.");
            }
        }

        private void UpdateCheckServiceUrl_Validated(object sender, EventArgs e)
        {
            errorProvider1.SetError(UpdateCheckServiceUrl, String.Empty);
        }
    }
}