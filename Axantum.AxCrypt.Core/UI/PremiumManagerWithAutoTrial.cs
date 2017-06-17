using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;

using static Axantum.AxCrypt.Abstractions.TypeResolve;
using AxCrypt.Content;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManagerWithAutoTrial : PremiumManager
    {
        protected override async Task<PremiumStatus> TryPremium(LogOnIdentity identity, PremiumInfo premiumInfo)
        {
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
    }
}