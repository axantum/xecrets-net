using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Service;
using System;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public abstract class PremiumManager
    {
        public async Task<bool> StartTrial(LogOnIdentity identity)
        {
            await BuyPremium(identity);

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
            await BuyPremium(identity, false);
        }

        public async Task BuyPremium(LogOnIdentity identity, bool fromRenewSubscriptionDialog)
        {
            string tag = string.Empty;
            if (New<KnownIdentities>().IsLoggedOn)
            {
                IAccountService accountService = New<LogOnIdentity, IAccountService>(identity);
                tag = (await accountService.AccountAsync()).Tag ?? string.Empty;
            }

            BrowseUtility.RedirectToPurchasePage(identity.UserEmail.Address, fromRenewSubscriptionDialog, tag);
        }
    }
}