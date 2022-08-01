using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Forms;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;

using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    public partial class LegacyLogOnAccountDialog : StyledMessageBase

    {
        public LegacyLogOnAccountDialog()
        {
            InitializeComponent();
        }

        private LogOnAccountViewModel _viewModel;

        public LegacyLogOnAccountDialog(Form owner, LogOnAccountViewModel viewModel)
            : this()
        {
            InitializeStyle(owner);
            _viewModel = viewModel;
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.TitleAxCryptIdSignInText;
            if (!string.IsNullOrEmpty(_viewModel.UserEmail))
            {
                Text = $"{Text} - {_viewModel.UserEmail}";
            }
            _passphraseGroupBox.Text = Texts.PassphrasePrompt;
            _showPassphrase.Text = Texts.ShowPasswordOptionPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonReset.Text = "&" + Texts.SwitchUserButtonText;
            _troubleRememberingLabel.Text = "&" + Texts.TroubleRememberingLabel;
        }

        private async void LogOnAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }
            _troubleRememberingPanel.Hide();
            _viewModel.TooManyTries += (s, ea) => { New<IUIThread>().PostTo(() => _troubleRememberingPanel.Show()); };

            _passphrase.LostFocus += (s, ea) => { _viewModel.PasswordText = _passphrase.Text; };
            _passphrase.TextChanged += (s, ea) => { ClearErrorProviders(); };
            _passphrase.Validating += (s, ea) => { _viewModel.PasswordText = _passphrase.Text; };
            _showPassphrase.CheckedChanged += (s, ea) => { _viewModel.ShowPassword = _showPassphrase.Checked; };

            _viewModel.BindPropertyChanged(nameof(LogOnAccountViewModel.ShowPassword), (bool show) => { _passphrase.UseSystemPasswordChar = !(_showPassphrase.Checked = show); });
            EmailAddress email;
            LogOnIdentity identity = LogOnIdentity.Empty;
            if (EmailAddress.TryParse(_viewModel.UserEmail, out email))
            {
                identity = new LogOnIdentity(email, Passphrase.Empty);
            }
            await _premiumLinkLabel.ConfigureAsync(identity);
        }

        private async void ButtonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;

            if (!await AdHocValidationDueToMonoLimitations())
            {
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private async Task<bool> AdHocValidationDueToMonoLimitations()
        {
            bool validated = await AdHocValidatePassphrase() || await AdHocValidatePassphraseForFile();

            return validated;
        }

        private async Task<bool> AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (!await _viewModel.ValidateItemAsync(nameof(LogOnAccountViewModel.PasswordText)))
            {
                _errorProvider1.SetError(_passphrase, Texts.WrongPassphrase);
                return false;
            }
            return true;
        }

        private async Task<bool> AdHocValidatePassphraseForFile()
        {
            _errorProvider1.Clear();
            if (!await _viewModel.ValidateItemAsync(nameof(LogOnAccountViewModel.EncryptedFileFullName)))
            {
                _errorProvider1.SetError(_passphrase, Texts.WrongPassphrase);
                return false;
            }
            return true;
        }

        private void LogOnAccountDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();
            _passphrase.Focus();
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider1.Clear();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void troubleRememberingLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Core.BrowseUtility.RedirectToAccountWebUrl(Texts.PasswordResetHyperLink);
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
            _errorProvider2.Clear();
        }

        private async void ButtonReset_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }
    }
}