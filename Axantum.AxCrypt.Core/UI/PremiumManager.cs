using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Service;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public abstract class PremiumManager
    {
        public async Task<bool> StartTrial(LogOnIdentity identity)
        {
            await DisplayPremiumPurchasePage(identity);

            return true;
        }

        public async Task<PlanState> PlanStateWithTryPremium(LogOnIdentity identity, NameOf startTrialMessage)
        {
            PlanInformation planInformation = await PlanInformation.CreateAsync(identity);
            return await TryPremium(identity, planInformation, startTrialMessage);
        }

        protected abstract Task<PlanState> TryPremium(LogOnIdentity identity, PlanInformation planInformation, NameOf startTrialMessage);

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

        public void RedirectToMyAxCryptIDPage()
        {
            string link = "{0}Home/Login?email={1}".QueryFormat(Resolve.UserSettings.AccountWebUrl, New<UserSettings>().UserEmail);
            New<IBrowser>().OpenUri(new Uri(link));
        }

        public async Task DisplayPremiumPurchasePage(LogOnIdentity identity)
        {
            IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
            string tag = New<KnownIdentities>().IsLoggedOn ? (await accountService.AccountAsync()).Tag ?? string.Empty : string.Empty;
            string link = Texts.LinkToAxCryptPremiumPurchasePage.QueryFormat(Resolve.UserSettings.AccountWebUrl, identity.UserEmail, tag);
            New<IBrowser>().OpenUri(new Uri(link));
        }
    }
}