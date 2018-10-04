using System;
using System.Collections.Generic;
using System.Linq;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using AxCrypt.Content;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManagerWithAutoTrial : PremiumManager
    {
        protected override async Task<PlanState> TryPremium(LogOnIdentity identity, PlanInformation planInformation)
        {
            if (planInformation.PlanState != PlanState.CanTryPremium)
            {
                return planInformation.PlanState;
            }

            string result = await New<IPopup>().ShowAsync(new string[] { Texts.ButtonStartTrial, Texts.ButtonNotNow }, Texts.WelcomeMailSubject, Texts.MessageAskAboutStartTrialPremium, DoNotShowAgainOptions.TryPremium);
            if (result != Texts.ButtonStartTrial)
            {
                return planInformation.PlanState;
            }

            if (!await StartTrial(identity))
            {
                return PlanState.CanTryPremium;
            }

            return PlanState.HasPremium;
        }
    }
}