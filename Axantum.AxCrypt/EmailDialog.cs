using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class EmailDialog : Form
    {
        private AccountEmailViewModel _viewModel;

        public EmailDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        public EmailDialog(Form parent)
            : this()
        {
            _viewModel = new AccountEmailViewModel();

            EmailTextBox.LostFocus += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; AdHocValidateUserEmail(); };

            Owner = parent;
            Owner.Activated += (sender, e) => Activate();
            StartPosition = FormStartPosition.CenterParent;
        }

        private void EmailDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            EmailTextBox.Focus();
        }

        private void EmailDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            Process.Start(@"http://www.axcrypt.net/support/faq/");
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidateUserEmail();
        }

        private bool AdHocValidateUserEmail()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(AccountEmailViewModel.UserEmail)].Length > 0)
            {
                _errorProvider1.SetError(EmailTextBox, Resources.BadEmail);
                return false;
            }
            return true;
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
        }
    }
}