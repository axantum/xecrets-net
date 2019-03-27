using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Texts = AxCrypt.Content.Texts;

namespace Axantum.AxCrypt
{
    public partial class InviteUserDialog : StyledMessageBase
    {
        public InviteUserDialog()
        {
            InitializeComponent();
        }

        public InviteUserDialog(Form parent)
            : this()
        {
            InitializeStyle(parent);
        }

        protected override void InitializeContentResources()
        {
            Text = "User Invitation";

            _inviteUserPromptlabel.Text = "Type an email address of a person you wish to invite to use AxCrypt.";
            _buttonOk.Text = "&" + Texts.ButtonOkText;
            _buttonCancel.Text = "&" + Texts.ButtonCancelText;
            _emailGroupBox.Text = Texts.PromptEmailText;
        }

        private void InviteUserDialog_Load(object sender, EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            emailTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };
            emailTextBox.Focus();
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
            if (String.IsNullOrEmpty(emailTextBox.Text) || !emailTextBox.Text.IsValidEmail())
            {
                _errorProvider1.SetError(emailTextBox, Texts.BadEmail);
                return false;
            }
            return true;
        }

        private async void _buttonOk_Click(object sender, EventArgs e)
        {
            if (!AdHocValidationDueToMonoLimitations())
            {
                DialogResult = DialogResult.None;
                return;
            }

            await GetInviteUserPublicKey();
        }

        private async Task GetInviteUserPublicKey()
        {
            EmailAddress inviteUserEmail = EmailAddress.Parse(emailTextBox.Text);

            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            if (!string.IsNullOrEmpty(emailTextBox.Text) && await accountService.IsAccountSourceLocalAsync())
            {
                ShowOfflineOrLocalError();
                return;
            }

            if (!await CheckAccountStatusAndShowInviteMessageDialog(inviteUserEmail, accountService))
            {
                return;
            }

            IEnumerable<UserPublicKey> userPublicKey = await new EmailAddress[] { inviteUserEmail }.ToKnownPublicKeysAsync(accountService.Identity);
            if (userPublicKey != null && userPublicKey.Any())
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, "This user '{0}' was sucessfully invited".InvariantFormat(emailTextBox.Text));
            }
        }

        private async void ShowOfflineOrLocalError()
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.AccountServiceLocalExceptionDialogText);
        }

        private async Task<bool> CheckAccountStatusAndShowInviteMessageDialog(EmailAddress inviteUserEmail, IAccountService accountService)
        {
            AccountStatus accountStatus = await accountService.StatusAsync(inviteUserEmail).Free();
            if (accountStatus == AccountStatus.Offline || accountStatus == AccountStatus.Unknown)
            {
                ShowOfflineOrLocalError();
                return false;
            }
            if (accountStatus != AccountStatus.NotFound)
            {
                return true;
            }

            using (KeySharingInviteUserDialog inviteDialog = new KeySharingInviteUserDialog(this))
            {
                if (inviteDialog.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }
            }
            return true;
        }

        private void _buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            return;
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }
    }
}