using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class FreeForcedLicensePolicy : LicensePolicy
    {
        public FreeForcedLicensePolicy() : base(LogOnIdentity.Empty)
        {
        }

        protected override Task<SubscriptionLevel> SubscriptionLevelAsync()
        {
            return Task.FromResult(SubscriptionLevel.Free);
        }

        protected override Task<DateTime> SubscriptionExpirationAsync()
        {
            return Task.FromResult(DateTime.MaxValue);
        }

        public override Task<bool> IsTrialAvailableAsync()
        {
            return Task.FromResult(false);
        }
    }
}