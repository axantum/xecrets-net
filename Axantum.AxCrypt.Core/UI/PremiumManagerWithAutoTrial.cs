using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;

using static Axantum.AxCrypt.Abstractions.TypeResolve;
using AxCrypt.Content;
using Axantum.AxCrypt.Common;

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

            if (premiumInfo.PremiumStatus != PremiumStatus.CanTryPremium)
            {
                return premiumInfo.PremiumStatus;
            }

            string result = await New<IPopup>().ShowAsync(new string[] { Texts.ButtonStartTrial, Texts.ButtonCancelText }, Texts.WelcomeMailSubject, Texts.MessageAskAboutStartTrial, DontShowAgain.TryPremium);
            if (result != Texts.ButtonStartTrial)
            {
                return premiumInfo.PremiumStatus;
            }

            if (!await StartTrial(identity))
            {
                return PremiumStatus.CanTryPremium;
            }

            return PremiumStatus.HasPremium;
        }
    }
}