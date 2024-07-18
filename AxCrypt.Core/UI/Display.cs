using AxCrypt.Common;
using AxCrypt.Content;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
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

        public async Task UpdateCheckPopups(bool alwaysDisplay, DownloadVersion downloadVersion)
        {
            if (await CriticalUpdateWarningPopupAsync(downloadVersion))
            {
                return;
            }
            if (await NewVersionAvailablePopupAsync(alwaysDisplay, downloadVersion))
            {
                return;
            }
            await NoUpdateAvailablePopupAsync(alwaysDisplay, downloadVersion);
        }

        private async Task<bool> CriticalUpdateWarningPopupAsync(DownloadVersion downloadVersion)
        {
            string msg = GetCriticalUpdateWarning(downloadVersion.Level);
            if (msg.Length == 0)
            {
                return false;
            }

            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, msg);
            BrowseUtility.RedirectTo(Resolve.UserSettings.UpdateUrl.ToString());
            return true;
        }

        private static string GetCriticalUpdateWarning(UpdateLevels level)
        {
            if (level.HasFlag(UpdateLevels.Security))
            {
                return Texts.SecurityUpdateAvailableWarning;
            }
            if (level.HasFlag(UpdateLevels.Reliability))
            {
                return Texts.ReliabilityUpdateAvailableWarning;
            }
            return string.Empty;
        }

        private async Task<bool> NewVersionAvailablePopupAsync(bool alwaysDisplay, DownloadVersion downloadVersion)
        {
            Version version = downloadVersion.Version;
            if (New<IVersion>().Current >= version)
            {
                return false;
            }

            if (!alwaysDisplay && New<UserSettings>().MostRecentVersionInformed == version.ToString())
            {
                return false;
            }
            New<UserSettings>().MostRecentVersionInformed = version.ToString();

            PopupButtons result = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.InformationTitle, Texts.NewVersionIsAvailableText.InvariantFormat(version));
            if (result == PopupButtons.Ok)
            {
                BrowseUtility.RedirectTo(Resolve.UserSettings.UpdateUrl.ToString());
            }

            return true;
        }

        private async Task<bool> NoUpdateAvailablePopupAsync(bool alwaysDisplay, DownloadVersion downloadVersion)
        {
            if (!alwaysDisplay)
            {
                return false;
            }
            if (New<IVersion>().Current.ToString() != downloadVersion.Version.ToString())
            {
                return false;
            }

            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.LatestVersionAlreadyPresentText);
            return false;
        }

        private static string GetLicenseStatus(bool isLoggedOn)
        {
            if (!isLoggedOn)
            {
                return string.Empty;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Business))
            {
                return Texts.LicenseBusinessNameText;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Premium))
            {
                return Texts.LicensePremiumNameText;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.PasswordManager))
            {
                return Texts.PromptPasswordManager;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Viewer))
            {
                return Texts.LicenseViewerNameText;
            }

            return Texts.LicenseFreeNameText;
        }

        public string GetLicenseStatusAndExpiration()
        {
            if (!New<KnownIdentities>().IsLoggedOn)
            {
                return string.Empty;
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Business))
            {
                return string.Format(Texts.SubscriptionPlanValidUntilFormat, Texts.LicenseBusinessNameText, New<LicensePolicy>().Expiration.ToString("d", CultureInfo.CurrentCulture));
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.Premium))
            {
                return string.Format(Texts.SubscriptionPlanValidUntilFormat, Texts.LicensePremiumNameText, New<LicensePolicy>().Expiration.ToString("d", CultureInfo.CurrentCulture));
            }

            if (New<LicensePolicy>().Capabilities.Has(LicenseCapability.PasswordManager))
            {
                return string.Format(Texts.SubscriptionPlanValidUntilFormat, Texts.PromptPasswordManager, New<LicensePolicy>().Expiration.ToString("d", CultureInfo.CurrentCulture));
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
                UserKeyPair activeKeyPair = Resolve.KnownIdentities.DefaultEncryptionIdentity.ActiveEncryptionKeyPair;
                return activeKeyPair != UserKeyPair.Empty ? Texts.AccountLoggedOnStatusText.InvariantFormat(activeKeyPair.UserEmail) : Texts.LoggedOnStatusText;
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

        public async Task LocalSignInWarningPopUpAsync(bool isLoggedOn)
        {
            if (New<AxCryptOnlineState>().IsOffline)
            {
                return;
            }

            if (!isLoggedOn)
            {
                return;
            }

            if (!await IsAccountSourceLocal())
            {
                return;
            }

            PopupButtons click = await New<IPopup>().ShowAsync(PopupButtons.OkCancel, Texts.InformationTitle, Texts.LocalSignInWarningPopUpText);
            if (click == PopupButtons.Ok)
            {
                BrowseUtility.RedirectToAccountWebUrl();
            }
        }

        private static async Task<bool> IsAccountSourceLocal()
        {
            IAccountService accountService = New<LogOnIdentity, IAccountService>(New<KnownIdentities>().DefaultEncryptionIdentity);

            return await accountService.IsAccountSourceLocalAsync();
        }
    }
}