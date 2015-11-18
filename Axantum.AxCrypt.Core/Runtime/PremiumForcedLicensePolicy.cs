using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class PremiumForcedLicensePolicy : LicensePolicy
    {
        public PremiumForcedLicensePolicy()
            : base(LogOnIdentity.Empty)
        {
        }

        protected override SubscriptionLevel SubscriptionLevel
        {
            get { return SubscriptionLevel.Premium; }
        }
    }
}