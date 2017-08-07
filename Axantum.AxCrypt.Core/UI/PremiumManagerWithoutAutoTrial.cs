using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Core.UI
{
    public class PremiumManagerWithoutAutoTrial : PremiumManager
    {
        protected override Task<PremiumStatus> TryPremium(LogOnIdentity identity, PremiumInfo premiumInfo)
        {
            if (premiumInfo == null)
            {
                throw new ArgumentNullException(nameof(premiumInfo));
            }

            return Task.FromResult(premiumInfo.PremiumStatus);
        }
    }
}