using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    public partial class VerifyAccountDialog : StyledMessageBase
    {
        private VerifyAccountViewModel _viewModel;

        public VerifyAccountDialog(Form owner, VerifyAccountViewModel viewModel)
        {
            InitializeComponent();
            InitializeStyle(owner);
            _viewModel = viewModel;
        }

        private void VerifyAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            PassphraseTextBox.TextChanged += (s, ee) => { _viewModel.Passphrase = PassphraseTextBox.Text; };
            VerifyPassphraseTextbox.TextChanged += (s, ee) => { _viewModel.VerificationPassphrase = PassphraseTextBox.Text; };
            _activationCode.TextChanged += (s, ee) => { _viewModel.VerificationCode = _activationCode.Text; };
            ShowPassphraseCheckBox.CheckedChanged += (s, ee) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };

            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.ShowPassphrase), (bool show) => { PassphraseTextBox.UseSystemPasswordChar = VerifyPassphraseTextbox.UseSystemPasswordChar = !(ShowPassphraseCheckBox.Checked = show); });
            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.UserEmail), (string u) => { EmailTextBox.Text = u; });

            _activationCode.Focus();
        }

        private async void _buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;
            if (!AdHocValidationDueToMonoLimitations())
            {
                return;
            }

            await _viewModel.VerifyAccount.ExecuteAsync(null);
            if (AdHocValidateVerificationCode())
            {
                DialogResult = DialogResult.OK;
            }
            return;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidatePassphrase() & AdHocValidateVerfication();
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.Passphrase)].Length > 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, Resources.WrongPassphrase);
                return false;
            }
            return true;
        }

        private bool AdHocValidateVerfication()
        {
            _errorProvider2.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.VerificationPassphrase)].Length > 0)
            {
                _errorProvider2.SetError(VerifyPassphraseTextbox, Resources.PassphraseVerificationMismatch);
                return false;
            }
            return true;
        }

        private bool AdHocValidateVerificationCode()
        {
            _errorProvider3.Clear();
            if (_viewModel.ErrorMessage.Length > 0)
            {
                _errorProvider3.SetError(_activationCode, Resources.BadEmail);
                return false;
            }
            return true;
        }
    }
}