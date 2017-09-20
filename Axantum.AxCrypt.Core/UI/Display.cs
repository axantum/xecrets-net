using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class Display
    {
        public async Task<string> WindowTitleTextAsync(bool isLoggedOn)
        {
            string licenseStatus = GetLicenseStatus(isLoggedOn);
            string logonStatus = GetLogonStatus(isLoggedOn);

            string title = Texts.TitleMainWindow.InvariantFormat(New<AboutAssembly>().AssemblyProduct, New<AboutAssembly>().AssemblyVersion, String.IsNullOrEmpty(New<AboutAssembly>().AssemblyDescription) ? string.Empty : " " + New<AboutAssembly>().AssemblyDescription);
            string text = Texts.TitleWindowSignInStatus.InvariantFormat(title, licenseStatus, logonStatus);
            text = await AddIndicatorsAsync(isLoggedOn, text);

            return text;
        }

        private static string GetLicenseStatus(bool isLoggedOn)
        {
            if (!isLoggedOn)
            {
                return string.Empty;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Premium))
            {
                return Texts.LicensePremiumNameText;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Viewer))
            {
                return Texts.LicenseViewerNameText;
            }

            return Texts.LicenseFreeNameText;
        }

        private static string GetLogonStatus(bool isLoggedOn)
        {
            if (isLoggedOn)
            {
                UserKeyPair userKeys = Resolve.KnownIdentities.DefaultEncryptionIdentity.UserKeys;
                return userKeys != UserKeyPair.Empty ? Texts.AccountLoggedOnStatusText.InvariantFormat(userKeys.UserEmail) : Texts.LoggedOnStatusText;
            }
            return Texts.LoggedOffStatusText;
        }

        private static async Task<string> AddIndicatorsAsync(bool isLoggedOn, string text)
        {
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return $"{text} [{Texts.OfflineIndicatorText}]";
            }

            if (!isLoggedOn)
            {
                return text;
            }

            if (await IsAccountSourceLocal())
            {
                return $"{text} [{Texts.LocalIndicatorText}]";
            }
            return text;
        }

        public async Task LocalSignInWarningPopUp(bool loggedOn)
        {
            if (!loggedOn)
            {
                return;
            }
            if (await IsAccountSourceLocal())
            {
                PopupButtons click = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.InformationTitle, "There is a password or account mismatch between local and online which may cause unexpected results. Please ensure you are using the same password and account online");
                if (click == PopupButtons.Ok)
                {
                    New<IBrowser>().OpenUri(New<UserSettings>().AccountWebUrl);
                }
            }
        }

        private static async Task<bool> IsAccountSourceLocal()
        {
            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            UserAccount userAccount = await accountService.AccountAsync();

            if (userAccount.AccountSource == AccountSource.Local)
            {
                return true;
            }
            return false;
        }
    }
}