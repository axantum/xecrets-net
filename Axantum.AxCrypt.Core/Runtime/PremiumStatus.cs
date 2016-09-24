using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public enum PremiumStatus
    {
        Unknown = 0,
        NoPremium,
        HasPremium,
        CanTryPremium,
        OfflineNoPremium,
    }
}