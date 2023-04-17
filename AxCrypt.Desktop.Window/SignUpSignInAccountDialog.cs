using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace AxCrypt.Desktop.Window
{
    public partial class SignUpSignInAccountDialog : StyledMessageBase
    {
        public SignUpSignInAccountDialog()
        {
            InitializeComponent();
        }

        private LogOnAccountViewModel _viewModel;

        public SignUpSignInAccountDialog(Form owner, LogOnAccountViewModel viewModel)
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
                UserEmailTextBox.Text = _viewModel.UserEmail;
                UserEmailTextBox.Enabled = false;
            }

            _userEmailGroupBox.Text = Texts.PromptEmailText;
            _passphraseGroupBox.Text = Texts.PassphrasePrompt;
            _showPassphrase.Text = Texts.ShowPasswordOptionPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonSwitchUser.Text = "&" + Texts.SwitchUserButtonText;
            _troubleRememberingLabel.Text = "&" + Texts.TroubleRememberingLabel;
            _createAccountLinkLabel.Text = "&" + Texts.RegisterLink;
        }

        private async void SignUpSignInAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _troubleRememberingPanel.Hide();
            _viewModel.TooManyTries += (s, ea) => { New<IUIThread>().PostTo(() => _troubleRememberingPanel.Show()); };

            UserEmailTextBox.LostFocus += (s, ea) => { _viewModel.UserEmail = UserEmailTextBox.Text; ClearErrorProviders(); };
            UserEmailTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };

            _passphrase.LostFocus += (s, ea) => { _viewModel.PasswordText = _passphrase.Text; };
            _passphrase.TextChanged += (s, ea) => { ClearErrorProviders(); };
            _passphrase.Validating += (s, ea) => { _viewModel.PasswordText = _passphrase.Text; };
            _showPassphrase.CheckedChanged += (s, ea) => { _viewModel.ShowPassword = _showPassphrase.Checked; };

            _viewModel.BindPropertyChanged(nameof(LogOnAccountViewModel.ShowPassword), (bool show) => { _passphrase.UseSystemPasswordChar = !(_showPassphrase.Checked = show); });

            if (New<Core.UI.UserSettings>().IsFirstSignIn)
            {
                _languagePanel.Visible = true;
                _languageSelectionTextLabel.Text = Texts.LanguageSelectionLabelText;
                _languageCultureDropDown.DataSource = SupportedLanguages();
                _languageCultureDropDown.SelectedValue = Resolve.UserSettings.CultureName;
            }
        }

        private IList<KeyValuePair<string, string>> SupportedLanguages()
        {
            IList<KeyValuePair<string, string>> supportedLanguages = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Texts.EnglishLanguageToolStripMenuItemText, "en-US"),
                new KeyValuePair<string, string>(Texts.GermanLanguageSelectionText, "de-DE"),
                new KeyValuePair<string, string>(Texts.DutchLanguageSelection, "nl-NL"),
                new KeyValuePair<string, string>(Texts.SpanishLanguageToolStripMenuItemText, "es-ES"),
                new KeyValuePair<string, string>(Texts.FrancaisLanguageToolStripMenuItemText, "fr-FR"),
                new KeyValuePair<string, string>(Texts.ItalianLanguageSelection, "it-IT"),
                new KeyValuePair<string, string>(Texts.KoreanLanguageSelection, "ko"),
                new KeyValuePair<string, string>(Texts.PolishLanguageSelection, "pl-PL"),
                new KeyValuePair<string, string>(Texts.PortugueseBrazilLanguageSelection, "pt-BR"),
                new KeyValuePair<string, string>(Texts.SwedishLanguageToolStripMenuItemText, "sv-SE"),
                new KeyValuePair<string, string>(Texts.TurkishLanguageToolStripMenuItemText, "tr-TR"),
                new KeyValuePair<string, string>(Texts.RussianLanguageSelection, "ru-RU"),
                new KeyValuePair<string, string>(Texts.ChineseLanguageSelectionText, "zh-CN"),
                new KeyValuePair<string, string>(Texts.ArabicLanguageSelectionText, "ar-AR"),
            };

            return supportedLanguages;
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
            if (!AdHocValidateUserEmail())
            {
                return false;
            }

            if (!string.IsNullOrEmpty(_viewModel.EncryptedFileFullName))
            {
                return await AdHocValidatePassphraseForFile();
            }

            if (!await AdHocValidatePassphrase())
            {
                return false;
            }

            return true;
        }

        private bool AdHocValidateUserEmail()
        {
            _errorProvider1.Clear();
            if (string.IsNullOrEmpty(_viewModel.UserEmail))
            {
                _errorProvider1.SetError(UserEmailTextBox, Texts.BadEmail);
                return false;
            }

            if (_viewModel[nameof(AccountEmailViewModel.UserEmail)].Length > 0)
            {
                _errorProvider1.SetError(UserEmailTextBox, Texts.BadEmail);
                return false;
            }

            return true;
        }

        private async Task<bool> AdHocValidatePassphrase()
        {
            _errorProvider2.Clear();
            if (!await _viewModel.ValidateItemAsync(nameof(LogOnAccountViewModel.PasswordText)))
            {
                _errorProvider2.SetError(_passphrase, Texts.WrongPassphrase);
                return false;
            }
            return true;
        }

        private async Task<bool> AdHocValidatePassphraseForFile()
        {
            _errorProvider2.Clear();
            if (!await _viewModel.ValidateItemAsync(nameof(LogOnAccountViewModel.EncryptedFileFullName)))
            {
                _errorProvider2.SetError(_passphrase, Texts.WrongPassphrase);
                return false;
            }
            return true;
        }

        private void SignUpSignInAccountDialog_Activated(object sender, EventArgs e)
        {
            BringToFront();

            if (string.IsNullOrEmpty(_viewModel.UserEmail))
            {
                UserEmailTextBox.Focus();
                return;
            }

            ResizeUserEmailBox();
            _buttonSwitchUser.Visible = true;
            _buttonSwitchUser.Font = New<FontLoader>().ContentText;

            _passphrase.Focus();
        }

        private void ResizeUserEmailBox()
        {
            UserEmailTextBox.MaximumSize = new System.Drawing.Size(280, 26);
            UserEmailTextBox.MinimumSize = new System.Drawing.Size(280, 26);
            UserEmailTextBox.Size = new System.Drawing.Size(280, 26);
        }

        private void PassphraseTextBox_Enter(object sender, EventArgs e)
        {
            _errorProvider2.Clear();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void troubleRememberingLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowseUtility.RedirectToAccountWebUrl(Texts.PasswordResetHyperLink);
        }

        private void createNewAccountLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            BrowseUtility.RedirectToAccountWebUrl(Texts.LinkToSignUpWebPage);
        }

        private void SignUpSignInDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            BrowseUtility.RedirectTo(Texts.LinkToGettingStarted);
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
            _errorProvider2.Clear();
        }

        private async void ButtonSwitchUser_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Retry;
        }

        private void LanguageCultureDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            CultureInfo selectedCulture = CultureInfo.CreateSpecificCulture(_languageCultureDropDown.SelectedValue.ToString());
            Resolve.UserSettings.CultureName = selectedCulture.Name;
        }
    }
}