using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManager
    {
        public async Task<bool> StartTrial(LogOnIdentity identity)
        {
            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
            try
            {
                using (await New<IProgressDialog>().Show(Texts.ProgressIndicatorTrialMessage, Texts.ProgressIndicatorWaitMessage))
                {
                    await accountService.StartPremiumTrialAsync();
                }
            }
            catch (Exception ex)
            {
                if (New<AxCryptOnlineState>().IsOffline)
                {
                    await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.MessageErrorTitle, Texts.NoInternetErrorMessage);
                    return false;
                }

                await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.MessageUnexpectedErrorTitle, Texts.MessageUnexpectedErrorText.InvariantFormat(ex.Innermost().Message));
                return false;
            }

            await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.LicensePolicyChange, identity));
            await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.InformationTitle, Texts.TrialPremiumStartInfo);

            return true;
        }

        public async Task<PremiumStatus> PremiumStatusWithTryPremium(LogOnIdentity identity)
        {
            PremiumInfo premiumInfo = await PremiumInfo.CreateAsync(identity);
            if (premiumInfo.PremiumStatus == PremiumStatus.HasPremium)
            {
                return PremiumStatus.HasPremium;
            }

            if (premiumInfo.PremiumStatus == PremiumStatus.CanTryPremium)
            {
                await New<IPopup>().ShowAsync(new string[] { Texts.ButtonStartTrial }, Texts.WelcomeMailSubject, Texts.MessageAskAboutStartTrial);
                if (!await StartTrial(identity))
                {
                    return PremiumStatus.CanTryPremium;
                }

                return PremiumStatus.HasPremium;
            }

            return premiumInfo.PremiumStatus;
        }

        public async Task BuyPremium(LogOnIdentity identity)
        {
            string tag = string.Empty;
            if (New<KnownIdentities>().IsLoggedOn)
            {
                IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
                tag = (await accountService.AccountAsync()).Tag ?? string.Empty;
            }

            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(Resolve.UserSettings.AccountWebUrl, identity.UserEmail, tag);
            New<IBrowser>().OpenUri(new Uri(link));
        }
    }
}