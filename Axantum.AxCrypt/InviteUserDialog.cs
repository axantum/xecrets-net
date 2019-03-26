using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Forms;
using System;
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

            EmailTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };
            EmailTextBox.Focus();
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
            if (String.IsNullOrEmpty(EmailTextBox.Text) || !EmailTextBox.Text.IsValidEmail())
            {
                _errorProvider1.SetError(EmailTextBox, Texts.BadEmail);
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
            EmailAddress inviteUserEmail = EmailAddress.Parse(EmailTextBox.Text);

            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            if (!string.IsNullOrEmpty(EmailTextBox.Text) && await accountService.IsAccountSourceLocalAsync())
            {
                await ShowOfflineOrLocalError();
                return;
            }

            AccountStorage accountStorage = new AccountStorage(accountService);
            AccountStatus accountStatus = await accountStorage.StatusAsync(inviteUserEmail).Free();
            if (accountStatus == AccountStatus.Offline || accountStatus == AccountStatus.Unknown)
            {
                await ShowOfflineOrLocalError();
                return;
            }
            if (accountStatus != AccountStatus.NotFound)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, "You can't invite, If the entered user email already having the AxCrypt Account.");
                return;
            }

            await GetInviteUserPublicKeyAsync(inviteUserEmail, accountStorage);
        }

        private async Task GetInviteUserPublicKeyAsync(EmailAddress inviteUserEmail, AccountStorage accountStorage)
        {
            try
            {
                UserPublicKey userPublicKey = await accountStorage.GetOtherUserPublicKeyAsync(inviteUserEmail).Free();
                if (userPublicKey != null)
                {
                    New<KnownPublicKeys>().AddOrReplace(userPublicKey);
                    New<UserPublicKeyUpdateStatus>().SetStatus(userPublicKey, PublicKeyUpdateStatus.RecentlyUpdated);
                    await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, "This user '{0}' was sucessfully invited".InvariantFormat(EmailTextBox.Text));
                }
                if (New<AxCryptOnlineState>().IsOffline)
                {
                    await ShowOfflineOrLocalError();
                }
            }
            catch (BadRequestApiException braex)
            {
                New<IReport>().Exception(braex);
                _errorProvider1.SetError(EmailTextBox, Texts.InvalidEmail);
                _errorProvider1.SetIconPadding(EmailTextBox, 3);
            }
        }

        private async Task ShowOfflineOrLocalError()
        {
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, "The invite user can't be added in local or offline state since there is no contact with the server.");
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