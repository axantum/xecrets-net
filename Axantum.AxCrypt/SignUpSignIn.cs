using Axantum.AxCrypt.Abstractions;
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
            PopupButtons dialogResult;
            do
            {
                dialogResult = PopupButtons.Ok;
                status = await EnsureEmailAccountAsync(parent);
                if (StopAndExit)
                {
                    return;
                }

                switch (status)
                {
                    case AccountStatus.NotFound:
                        await New<LogOnIdentity, IAccountService>(LogOnIdentity.Empty).SignupAsync(EmailAddress.Parse(UserEmail));
                        New<IPopup>().Show(PopupButtons.Ok, Texts.MessageSigningUpTitle, Texts.MessageSigningUpText.InvariantFormat(UserEmail));
                        status = VerifyAccountOnline(parent);
                        break;

                    case AccountStatus.InvalidName:
                        dialogResult = New<IPopup>().Show(PopupButtons.OkCancelExit, Texts.MessageInvalidSignUpEmailTitle, Texts.MessageInvalidSignUpEmailText.InvariantFormat(UserEmail));
                        UserEmail = string.Empty;
                        break;

                    case AccountStatus.Unverified:
                        status = VerifyAccountOnline(parent);
                        break;

                    case AccountStatus.Verified:
                        break;

                    case AccountStatus.Offline:
                        New<AxCryptOnlineState>().IsOnline = New<IInternetState>().Connected;
                        using (CreateNewAccountDialog dialog = new CreateNewAccountDialog(parent, String.Empty, EmailAddress.Parse(UserEmail)))
                        {
                            DialogResult result = dialog.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                status = AccountStatus.Verified;
                            }
                        }
                        break;

                    case AccountStatus.Unknown:
                    case AccountStatus.Unauthenticated:
                    case AccountStatus.DefinedByServer:
                        dialogResult = New<IPopup>().Show(PopupButtons.OkCancelExit, Texts.MessageUnexpectedErrorTitle, Texts.MessageUnexpectedErrorText);
                        UserEmail = string.Empty;
                        break;

                    default:
                        break;
                }
                if (dialogResult == PopupButtons.Exit)
                {
                    StopAndExit = true;
                    return;
                }
                if (dialogResult == PopupButtons.Cancel)
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

        private static AccountStatus VerifyAccountOnline(Form parent)
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

            PopupButtons result = New<IPopup>().Show(PopupButtons.OkCancel, Texts.WelcomeToAxCryptTitle, Texts.WelcomeToAxCrypt);
            if (result == PopupButtons.Ok)
            {
                Process.Start(Texts.LinkToGettingStarted);
            }
            return AccountStatus.Verified;
        }
    }
}