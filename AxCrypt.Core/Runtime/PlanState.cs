using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.Runtime
{
    public enum PlanState
    {
        Unknown = 0,
        NoPremium,
        HasPremium,
        CanTryPremium,
        OfflineNoPremium,
        HasBusiness,
        HasPasswordManager,
    }
}