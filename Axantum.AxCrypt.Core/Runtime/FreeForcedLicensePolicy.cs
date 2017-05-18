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
        public FreeForcedLicensePolicy()
        {
            Capabilities = FreeCapabilities;
        }

        protected override Task RefreshAsync(LogOnIdentity identity)
        {
            return Task.FromResult<object>(null);
        }
    }
}