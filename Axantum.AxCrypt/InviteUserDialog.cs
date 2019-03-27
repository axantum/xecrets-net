using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Forms;
using System;
using System.Collections.Generic;
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

            _buttonOk.Click += async (sender, e) =>
            {
                if (!AdHocValidationDueToMonoLimitations())
                {
                    DialogResult = DialogResult.None;
                    return;
                }

                IEnumerable<UserPublicKey> userPublicKey = await EnsureUserAccountStatusAndGetInviteUserPublicKey();
                if (userPublicKey != null)
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
            };
        }

        protected override void InitializeContentResources()
        {
            Text = Texts.DialogInviteUserTitle;

            _inviteUserPromptlabel.Text = Texts.DialogInviteUserPromptLabelText;
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

        private async Task<IEnumerable<UserPublicKey>> EnsureUserAccountStatusAndGetInviteUserPublicKey()
        {
            string invitingUserName = _emailTextBox.Text;
            AccountStatus accountStatus = await invitingUserName.CheckUserAccountStatusAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
            if (!ShowInviteUserDialog(accountStatus))
            {
                return null;
            }

            return await GetUserPublicKeys(invitingUserName, accountStatus);
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
            _emailTextBox.Enabled = false;
            _emailTextBox.Text = $"[{Texts.OfflineIndicatorText}]";
            _errorProvider1.SetError(_emailTextBox, Texts.KeySharingOffline);
        }

        private async Task<IEnumerable<UserPublicKey>> GetUserPublicKeys(string inviteUserEmail, AccountStatus accountStatus)
        {
            IEnumerable<EmailAddress> inviteUserEmails = new EmailAddress[] { EmailAddress.Parse(inviteUserEmail) };
            return await inviteUserEmails.ToKnownPublicKeysAsync(New<KnownIdentities>().DefaultEncryptionIdentity);
        }

        private void ClearErrorProviders()
        {
            _errorProvider1.Clear();
        }
    }
}