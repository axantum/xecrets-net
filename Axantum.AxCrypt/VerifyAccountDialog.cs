using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class VerifyAccountDialog : StyledMessageBase
    {
        private VerifyAccountViewModel _viewModel;

        public VerifyAccountDialog()
        {
            InitializeComponent();
        }

        public VerifyAccountDialog(Form owner, VerifyAccountViewModel viewModel)
            : this()
        {
            InitializeStyle(owner);
            _viewModel = viewModel;
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogVerifyAccountTitle;

            PassphraseGroupBox.Text = Texts.PromptSetNewPassword;
            _showPassphrase.Text = Texts.ShowPasswordOptionPrompt;
            _verifyPasswordLabel.Text = Texts.VerifyPasswordPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _resendButton.Text = "&" + Texts.ResendButtonText;
            _resendButtonToolTip.SetToolTip(_resendButton, Texts.ResendButtonToolTip);
            _buttonOk.Enabled = false;
            _emailGroupBox.Text = Texts.PromptEmailText;
            _activationCodeGroupBox.Text = Texts.PromptActivationCode;
            _checkEmailLabel.Text = Texts.TextCheckEmailAndSpam;
        }

        private void VerifyAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _passwordStrengthMeter.MeterChanged += (ss, ee) => _buttonOk.Enabled = _passwordStrengthMeter.IsAcceptable;

            _passphrase.TextChanged += (s, ee) => { _viewModel.Passphrase = _passphrase.Text; };
            _passphrase.TextChanged += async (ss, ee) => { await _passwordStrengthMeter.MeterAsync(_passphrase.Text); };
            _passphraseVerification.TextChanged += (s, ee) => { _viewModel.VerificationPassphrase = _passphraseVerification.Text; };
            _activationCode.TextChanged += (s, ee) => { _viewModel.VerificationCode = _activationCode.Text; };
            _showPassphrase.CheckedChanged += (s, ee) => { _viewModel.ShowPassphrase = _showPassphrase.Checked; };

            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.ShowPassphrase), (bool show) => { _passphrase.UseSystemPasswordChar = _passphraseVerification.UseSystemPasswordChar = !(_showPassphrase.Checked = show); });
            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.UserEmail), (string u) => { _email.Text = u; });

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
            if (VerifyCode())
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
            return AdHocValidatePassphrase() & AdHocValidatePassphraseVerification() & AdHocValidateCode();
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.Passphrase)].Length > 0)
            {
                _errorProvider1.SetError(_passphrase, Texts.PasswordPolicyViolation);
                return false;
            }
            return true;
        }

        private bool AdHocValidatePassphraseVerification()
        {
            _errorProvider2.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.VerificationPassphrase)].Length > 0)
            {
                _errorProvider2.SetError(_passphraseVerification, Texts.PassphraseVerificationMismatch);
                return false;
            }
            return true;
        }

        private bool AdHocValidateCode()
        {
            _errorProvider3.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.VerificationCode)].Length > 0)
            {
                _errorProvider3.SetError(_activationCode, Texts.WrongVerificationCodeFormat);
                return false;
            }
            return true;
        }

        private bool VerifyCode()
        {
            _errorProvider3.Clear();
            if (_viewModel.ErrorMessage.Length > 0)
            {
                _errorProvider3.SetError(_activationCode, Texts.WrongVerificationCode);
                return false;
            }
            return true;
        }

        private void ResendButton_Click(object sender, EventArgs e)
        {
            UriBuilder url = new UriBuilder(Texts.ResendActivationHyperLink);
            url.Query = $"email={_viewModel.UserEmail}";
            Process.Start(url.ToString());
        }
    }
}