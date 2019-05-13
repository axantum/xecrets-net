using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class VerifyAccountDialog : StyledMessageBase
    {
        private VerifyAccountViewModel _viewModel;

        private ToolTip _toolTip = new ToolTip();

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
            _helpButton.Text = "&" + Texts.ButtonHelpText;
            _resendButton.Text = "&" + Texts.ResendButtonText;
            _resendButtonToolTip.SetToolTip(_resendButton, Texts.ResendButtonToolTip);
            _buttonOk.Enabled = true;
            _activationCodeGroupBox.Text = Texts.PromptActivationCode;
            _checkEmailLabel.Text = Texts.TextCheckEmailAndSpam;
        }

        private void VerifyAccountDialog_Load(object s, EventArgs ee)
        {
            if (DesignMode)
            {
                return;
            }

            _passwordStrengthMeter.MeterChanged += (sender, e) =>
            {
                _toolTip.SetToolTip(_passphrase, _passwordStrengthMeter.StrengthTip);
            };

            _passphraseVerification.ShortcutsEnabled = false;
            _passphraseVerification.ContextMenuStrip = null;
            _passphrase.TextChanged += (sender, e) => { _viewModel.PasswordText = _passphrase.Text; ClearErrorProviders(); };
            _passphrase.TextChanged += async (sender, e) => { await _passwordStrengthMeter.MeterAsync(_passphrase.Text); };
            _passphraseVerification.TextChanged += (sender, e) => { _viewModel.Verification = _passphraseVerification.Text; ClearErrorProviders(); };
            _activationCode.TextChanged += (sender, e) => { _viewModel.VerificationCode = _activationCode.Text; ClearErrorProviders(); };
            _showPassphrase.CheckedChanged += (sender, e) => { _viewModel.ShowPassword = _showPassphrase.Checked; };

            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.ShowPassword), (bool show) => { _passphrase.UseSystemPasswordChar = _passphraseVerification.UseSystemPasswordChar = !(_showPassphrase.Checked = show); });
            _viewModel.BindPropertyChanged(nameof(VerifyAccountViewModel.UserEmail), (string u) => { _promptTextLabel.Text = Texts.MessageSigningUpText.InvariantFormat(u); });

            Visible = true;
            _activationCode.Focus();
        }

        private void _buttonOk_Click(object sender, EventArgs e)
        {
            if (!IsAllValid())
            {
                DialogResult = DialogResult.None;
            }
        }

        private bool IsAllValid()
        {
            TaskRunner.WaitFor(() => _viewModel.CheckAccountStatus.ExecuteAsync(null));
            if (_viewModel.AlreadyVerified)
            {
                return true;
            }

            if (!AdHocValidationDueToMonoLimitations())
            {
                return false;
            }

            TaskRunner.WaitFor(() => _viewModel.VerifyAccount.ExecuteAsync(null));
            if (!VerifyCode())
            {
                _viewModel.ErrorMessage = string.Empty;
                return false;
            }

            if (!string.IsNullOrEmpty(_viewModel.ApiErrorMessage))
            {
                New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.OfflineApiExceptionDialogText);
                _viewModel.ApiErrorMessage = string.Empty;
                return false;
            }

            return true;
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
            if (_viewModel[nameof(VerifyAccountViewModel.PasswordText)].Length > 0)
            {
                _errorProvider1.SetError(_passphrase, Texts.PasswordPolicyViolation);
                return false;
            }
            return true;
        }

        private bool AdHocValidatePassphraseVerification()
        {
            _errorProvider2.Clear();
            if (_viewModel[nameof(VerifyAccountViewModel.Verification)].Length > 0)
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
            if (_viewModel[nameof(VerifyAccountViewModel.ErrorMessage)].Length > 0)
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

        private async void _helpButton_Click(object sender, EventArgs e)
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.DialogVerifyAccountTitle, Texts.PasswordRulesInfo);
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
            _errorProvider2.Clear();
            _errorProvider3.Clear();
            _errorProvider4.Clear();
        }
    }
}