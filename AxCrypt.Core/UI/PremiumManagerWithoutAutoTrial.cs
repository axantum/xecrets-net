using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AxCrypt.Core.UI
{
    public class PremiumManagerWithoutAutoTrial : PremiumManager
    {
        protected override Task<PlanState> TryPremium(LogOnIdentity identity, PlanInformation planInformation, NameOf startTrialMessage)
        {
            if (planInformation == null!)
            {
                throw new ArgumentNullException(nameof(planInformation));
            }

            return Task.FromResult(planInformation.PlanState);
        }
    }
}
