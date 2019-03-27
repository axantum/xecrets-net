using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
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

            _emailTextBox.TextChanged += (s, ea) => { ClearErrorProviders(); };
            _emailTextBox.Focus();
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
            if (String.IsNullOrEmpty(_emailTextBox.Text) || !_emailTextBox.Text.IsValidEmail())
            {
                _errorProvider1.SetError(_emailTextBox, Texts.BadEmail);
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

            await EnsureUserAccountStatusAndGetInviteUserPublicKey();
        }

        private async Task EnsureUserAccountStatusAndGetInviteUserPublicKey()
        {
            string invitingUserName = _emailTextBox.Text;
            AccountStatus accountStatus = await invitingUserName.CheckUserAccountStatusAsync(New<KnownIdentities>().DefaultEncryptionIdentity).Free();
            if (!ShowInviteUserDialog(accountStatus))
            {
                return;
            }

            IEnumerable<UserPublicKey> userPublicKey = await GetUserPublicKeys(invitingUserName, accountStatus);
            if (userPublicKey != null)
            {
                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, "This user '{0}' was sucessfully invited".InvariantFormat(_emailTextBox.Text));
            }
        }

        private bool ShowInviteUserDialog(AccountStatus accountStatus)
        {
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

        private void ShowOfflineOrLocalError()
        {
            _emailTextBox.Enabled = !New<AxCryptOnlineState>().IsOffline;
            _emailTextBox.Text = $"[{Texts.OfflineIndicatorText}]";
            _errorProvider1.SetError(_emailTextBox, Texts.KeySharingOffline);
        }

        private async Task<IEnumerable<UserPublicKey>> GetUserPublicKeys(string inviteUserEmail, AccountStatus accountStatus)
        {
            IEnumerable<EmailAddress> inviteUserEmails = new EmailAddress[] { EmailAddress.Parse(inviteUserEmail) };
            return await inviteUserEmails.ToKnownPublicKeysAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
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