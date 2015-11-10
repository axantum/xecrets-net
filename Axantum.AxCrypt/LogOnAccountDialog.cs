using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class LogOnAccountDialog : StyledMessageBase
    {
        public LogOnAccountDialog()
        {
            InitializeComponent();
        }

        private LogOnAccountViewModel _viewModel;

        public LogOnAccountDialog(Form owner, LogOnAccountViewModel viewModel)
            : this()
        {
            InitializeStyle(owner);
            _viewModel = viewModel;
        }

        private void LogOnAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged(nameof(LogOnAccountViewModel.UserEmail), (string userEmail) => { _email.Text = userEmail; });

            _passphrase.LostFocus += (s, ea) => { _viewModel.Passphrase = _passphrase.Text; };
            _passphrase.Validating += (s, ea) => { _viewModel.Passphrase = _passphrase.Text; };
            _showPassphrase.CheckedChanged += (s, ea) => { _viewModel.ShowPassphrase = _showPassphrase.Checked; };
            _email.LostFocus += (s, ea) => { _viewModel.UserEmail = _email.Text; AdHocValidateEmail(); };
            _email.Validating += (s, ea) => { _viewModel.UserEmail = _email.Text; AdHocValidateEmail(); };

            _viewModel.BindPropertyChanged(nameof(LogOnAccountViewModel.ShowPassphrase), (bool show) => { _passphrase.UseSystemPasswordChar = !(_showPassphrase.Checked = show); });
            _viewModel.BindPropertyChanged(nameof(LogOnAccountViewModel.ShowEmail), (bool show) => { EmailPanel.Visible = show; });
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
            if (_viewModel[nameof(LogOnAccountViewModel.Passphrase)].Length != 0)
            {
                _errorProvider1.SetError(_passphrase, _email.Text.Length > 0 ? Resources.WrongPassphrase : Resources.UnkownLogOn);
                return false;
            }
            return true;
        }

        private bool AdHocValidateEmail()
        {
            _errorProvider2.Clear();
            if (_email.Text.Length == 0 || _viewModel[nameof(LogOnAccountViewModel.UserEmail)].Length != 0)
            {
                _errorProvider2.SetError(_email, Resources.BadEmail);
                return false;
            }
            return true;
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            _viewModel.UserEmail = String.Empty;
            DialogResult = DialogResult.Retry;
        }

        private void LogOnAccountDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();
            if (String.IsNullOrEmpty(_email.Text))
            {
                _email.Focus();
            }
            else
            {
                _passphrase.Focus();
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