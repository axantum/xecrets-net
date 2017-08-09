using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using static Axantum.AxCrypt.Abstractions.TypeResolve;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Api.Model;

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

            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);
            UserAccount userAccount = await accountService.AccountAsync();

            if (userAccount.AccountSource == AccountSource.Local)
            {
                return $"{text} [{Texts.LocalIndicatorText}]";
            }
            return text;
        }
    }
}