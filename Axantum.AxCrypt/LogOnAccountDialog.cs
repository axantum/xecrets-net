using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms.Style;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class LogOnAccountDialog : Form
    {
        public LogOnAccountDialog()
        {
            InitializeComponent();
            new Styling(Resources.axcrypticon).Style(this);
        }

        private LogOnAccountViewModel _viewModel;

        public LogOnAccountDialog(Form parent, IUserSettings userSettings)
            : this()
        {
            _viewModel = new LogOnAccountViewModel(userSettings);

            _viewModel.BindPropertyChanged("UserEmail", (string userEmail) => { EmailTextBox.Text = userEmail; });

            PassphraseTextBox.LostFocus += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            PassphraseTextBox.Validating += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
            EmailTextBox.LostFocus += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; AdHocValidateEmail(); };
            EmailTextBox.Validating += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; AdHocValidateEmail(); };

            Owner = parent;
            StartPosition = FormStartPosition.CenterParent;
        }

        private void LogOnAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged("ShowPassphrase", (bool show) => { PassphraseTextBox.UseSystemPasswordChar = !(ShowPassphraseCheckBox.Checked = show); });
            _viewModel.BindPropertyChanged("ShowEmail", (bool show) => { EmailPanel.Visible = show; });
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidatePassphrase() & AdHocValidateEmail();

            return validated;
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel["Passphrase"].Length != 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, EmailTextBox.Text.Length > 0 ? Resources.WrongPassphrase : Resources.UnkownLogOn);
                return false;
            }
            return true;
        }

        private bool AdHocValidateEmail()
        {
            _errorProvider2.Clear();
            if (EmailTextBox.Text.Length == 0 || _viewModel["UserEmail"].Length != 0)
            {
                _errorProvider2.SetError(EmailTextBox, Resources.BadEmail);
                return false;
            }
            return true;
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void LogOnAccountDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();
            if (String.IsNullOrEmpty(EmailTextBox.Text))
            {
                EmailTextBox.Focus();
            }
            else
            {
                PassphraseTextBox.Focus();
            }
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void LogOnAccountDialog_ResizeAndMoveEnd(object sender, EventArgs e)
        {
            CenterToParent();
        }

        private void _buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }
    }
}