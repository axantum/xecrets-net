using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using AxCrypt.Content;
using System;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManagerWithAutoTrial : PremiumManager
    {
        protected override async Task<PlanState> TryPremium(LogOnIdentity identity, PlanInformation planInformation, NameOf startTrialMessage)
        {
            if (planInformation.PlanState != PlanState.CanTryPremium)
            {
                return planInformation.PlanState;
            }

            string messageText = Texts.MessageAskAboutStartTrial + Environment.NewLine + Environment.NewLine + Texts.ResourceManager.GetString(startTrialMessage.Name);
            string result = await New<IPopup>().ShowAsync(new string[] { Texts.ButtonStartTrial, Texts.ButtonNotNow }, Texts.WelcomeMailSubject, messageText, DoNotShowAgainOptions.TryPremium);
            if (result == Texts.ButtonStartTrial)
            {
                await identity.DisplayPremiumPurchasePage();
            }

            return planInformation.PlanState;
        }
    }
}