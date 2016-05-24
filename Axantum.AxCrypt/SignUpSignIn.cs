using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Core.UI.ViewModel;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt
{
    internal class SignUpSignIn
    {
        public string UserEmail { get; private set; }

        public bool StopAndExit { get; private set; }

        public SignUpSignIn(string userEmail)
        {
            UserEmail = userEmail;
        }

        public async Task DialogsAsync(Form parent)
        {
            AccountStatus status;
            DialogResult dialogResult;
            do
            {
                dialogResult = DialogResult.OK;
                status = await EnsureEmailAccountAsync(parent);
                if (StopAndExit)
                {
                    return;
                }

                switch (status)
                {
                    case AccountStatus.NotFound:
                        await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).SignupAsync(EmailAddress.Parse(UserEmail));
                        MessageDialog.ShowOk(parent, Texts.MessageSigningUpTitle, Texts.MessageSigningUpText.InvariantFormat(UserEmail));
                        status = VerifyAccountOnline(parent);
                        break;

                    case AccountStatus.InvalidName:
                        dialogResult = MessageDialog.ShowOkCancelExit(parent, Texts.MessageInvalidSignUpEmailTitle, Texts.MessageInvalidSignUpEmailText.InvariantFormat(UserEmail));
                        UserEmail = string.Empty;
                        break;

                    case AccountStatus.Unverified:
                        status = VerifyAccountOnline(parent);
                        break;

                    case AccountStatus.Verified:
                        break;

                    case AccountStatus.Offline:
                        dialogResult = MessageDialog.ShowOkCancelExit(parent, Texts.MessageSignUpInternetRequiredTitle, Texts.MessageSignUpInternetRequiredText);
                        New<AxCryptOnlineState>().IsOnline = true;
                        break;

                    case AccountStatus.Unknown:
                    case AccountStatus.Unauthenticated:
                    case AccountStatus.DefinedByServer:
                        dialogResult = MessageDialog.ShowOkCancelExit(parent, Texts.MessageUnexpectedErrorTitle, Texts.MessageUnexpectedErrorText);
                        UserEmail = string.Empty;
                        break;

                    default:
                        break;
                }
                if (dialogResult == DialogResult.Abort)
                {
                    StopAndExit = true;
                    return;
                }
                if (dialogResult == DialogResult.Cancel)
                {
                    return;
                }
            } while (status != AccountStatus.Verified);
        }

        private async Task<AccountStatus> EnsureEmailAccountAsync(Form parent)
        {
            AccountStatus status;
            if (!string.IsNullOrEmpty(UserEmail))
            {
                status = await GetCurrentStatusAsync();
                switch (status)
                {
                    case AccountStatus.Unknown:
                    case AccountStatus.InvalidName:
                    case AccountStatus.Unverified:
                    case AccountStatus.NotFound:
                    case AccountStatus.Offline:
                        break;

                    default:
                        return status;
                }
            }

            AskForEmailAddressToUse(parent);
            if (StopAndExit)
            {
                return AccountStatus.Unknown;
            }

            status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).StatusAsync(EmailAddress.Parse(UserEmail));
            return status;
        }

        private async Task<AccountStatus> GetCurrentStatusAsync()
        {
            EmailAddress email;
            if (!EmailAddress.TryParse(UserEmail, out email))
            {
                return AccountStatus.InvalidName;
            }
            AccountStatus status = await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).StatusAsync(email);
            return status;
        }

        private void AskForEmailAddressToUse(Form parent)
        {
            using (EmailDialog dialog = new EmailDialog(parent))
            {
                dialog.EmailTextBox.Text = UserEmail;
                DialogResult result = dialog.ShowDialog(parent);
                if (result != DialogResult.OK)
                {
                    StopAndExit = true;
                    return;
                }
                UserEmail = dialog.EmailTextBox.Text;
            }
        }

        private AccountStatus VerifyAccountOnline(Form parent)
        {
            VerifyAccountViewModel viewModel = new VerifyAccountViewModel(EmailAddress.Parse(Resolve.UserSettings.UserEmail));
            using (VerifyAccountDialog dialog = new VerifyAccountDialog(parent, viewModel))
            {
                DialogResult dialogResult = dialog.ShowDialog(parent);
                if (dialogResult != DialogResult.OK)
                {
                    return AccountStatus.Unverified;
                }
            }
            LogOnIdentity identity = new LogOnIdentity(EmailAddress.Parse(viewModel.UserEmail), Passphrase.Create(viewModel.Passphrase));
            AccountStorage store = new AccountStorage(New<LogOnIdentity, IAccountService>(identity));

            DialogResult result = MessageDialog.ShowOkCancel(parent, Texts.WelcomeToAxCryptTitle, Texts.WelcomeToAxCrypt);
            if (result == DialogResult.OK)
            {
                Process.Start(Texts.LinkToGettingStarted);
            }
            return AccountStatus.Verified;
        }
    }
}