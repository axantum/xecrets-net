using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManagerWithoutAutoTrial : PremiumManager
    {
        protected override Task<PlanState> TryPremium(LogOnIdentity identity, PlanInformation planInformation)
        {
            if (planInformation == null)
            {
                throw new ArgumentNullException(nameof(planInformation));
            }

            return Task.FromResult(planInformation.PlanState);
        }
    }
}