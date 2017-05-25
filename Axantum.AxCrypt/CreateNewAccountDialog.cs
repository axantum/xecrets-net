using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class CreateNewAccountDialog : StyledMessageBase
    {
        private CreateNewAccountViewModel _viewModel;

        private bool _isCreating = false;

        public CreateNewAccountDialog()
        {
            InitializeComponent();
        }

        public CreateNewAccountDialog(Form parent, string passphrase, EmailAddress email)
            : this()
        {
            InitializeStyle(parent);

            _viewModel = new CreateNewAccountViewModel(passphrase, email);
            PassphraseTextBox.TextChanged += (sender, e) => { _viewModel.Passphrase = PassphraseTextBox.Text; AdHocValidatePassphrase();};
            PassphraseTextBox.TextChanged += async (sender, e) => { await _passwordStrengthMeter.MeterAsync(PassphraseTextBox.Text); };
            VerifyPassphraseTextbox.TextChanged += (sender, e) => { _viewModel.Verification = VerifyPassphraseTextbox.Text; AdHocValidateVerfication();};
            EmailTextBox.LostFocus += (sender, e) => { _viewModel.UserEmail = EmailTextBox.Text; AdHocValidateUserEmail(); AdHocValidateUserEmail(); };
            ShowPassphraseCheckBox.CheckedChanged += (sender, e) => { _viewModel.ShowPassphrase = ShowPassphraseCheckBox.Checked; };
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.TitleCreateAxCryptIdTitle;

            _passphraseGroupBox.Text = Texts.PassphrasePrompt;
            ShowPassphraseCheckBox.Text = Texts.ShowPasswordOptionPrompt;
            _verifyPasswordLabel.Text = Texts.VerifyPasswordPrompt;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _helpButton.Text = "&" + Texts.ButtonHelpText;
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonOk.Enabled = true;
            _emailGroupBox.Text = Texts.PromptEmailText;
        }

        private void CreateNewAccountDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            _viewModel.BindPropertyChanged(nameof(CreateNewAccountViewModel.ShowPassphrase), (bool show) => { PassphraseTextBox.UseSystemPasswordChar = VerifyPassphraseTextbox.UseSystemPasswordChar = !(ShowPassphraseCheckBox.Checked = show); });
            _viewModel.BindPropertyChanged(nameof(CreateNewAccountViewModel.Passphrase), (string p) => { PassphraseTextBox.Text = p; });
            _viewModel.BindPropertyChanged(nameof(CreateNewAccountViewModel.Verification), (string v) => { VerifyPassphraseTextbox.Text = v; });
            _viewModel.BindPropertyChanged(nameof(CreateNewAccountViewModel.UserEmail), (string u) => { EmailTextBox.Text = u; });

            EmailTextBox.Enabled = EmailTextBox.Text.Length == 0;
            if (EmailTextBox.Enabled)
            {
                EmailTextBox.Focus();
            }
            else
            {
                PassphraseTextBox.Focus();
            }
        }

        private async void _buttonOk_Click(object sender, EventArgs e)
        {
            if (_isCreating || !AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }

            if (!New<KeyPairService>().IsAnyAvailable)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.OfflineAccountTitle, Texts.OfflineAccountBePatient);
            }
            CreateAccountAsync();
        }

        private async void CreateAccountAsync()
        {
            UseWaitCursor = true;

            EmailTextBox.Enabled = false;
            PassphraseTextBox.Enabled = false;
            VerifyPassphraseTextbox.Enabled = false;
            ShowPassphraseCheckBox.Enabled = false;
            _buttonOk.Enabled = false;
            _buttonCancel.Enabled = false;

            _isCreating = true;

            try
            {
                await Task.Run(() => _viewModel.CreateAccount.Execute(null));
            }
            finally
            {
                _isCreating = false;
                UseWaitCursor = false;
            }

            DialogResult = DialogResult.OK;
        }

        private bool AdHocValidationDueToMonoLimitations()
        {
            bool validated = AdHocValidateAllFieldsIndependently();
            return validated;
        }

        private bool AdHocValidateAllFieldsIndependently()
        {
            return AdHocValidatePassphrase() & AdHocValidateVerfication() & AdHocValidateUserEmail();
        }

        private bool AdHocValidatePassphrase()
        {
            _errorProvider1.Clear();
            if (_viewModel[nameof(CreateNewAccountViewModel.Passphrase)].Length > 0)
            {
                _errorProvider1.SetError(PassphraseTextBox, Texts.PasswordPolicyViolation);
                return false;
            }
            return true;
        }

        private bool AdHocValidateVerfication()
        {
            _errorProvider2.Clear();
            if (_viewModel[nameof(CreateNewAccountViewModel.Verification)].Length > 0)
            {
                _errorProvider2.SetError(VerifyPassphraseTextbox, Texts.PassphraseVerificationMismatch);
                return false;
            }
            return true;
        }

        private bool AdHocValidateUserEmail()
        {
            _errorProvider3.Clear();
            if (_viewModel[nameof(CreateNewAccountViewModel.UserEmail)].Length > 0)
            {
                _errorProvider3.SetError(EmailTextBox, Texts.BadEmail);
                return false;
            }
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            if (_isCreating)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private async void _helpButton_Click(object sender, EventArgs e)
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.DialogVerifyAccountTitle, Texts.PasswordRulesInfo);
        }
    }
}