using AxCrypt.Api.Model;
using AxCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Runtime
{
    public class PremiumForcedLicensePolicy : LicensePolicy
    {
        public PremiumForcedLicensePolicy() : base(false)
        {
        }

        public override LicenseCapabilities Capabilities
        {
            get
            {
                return PremiumCapabilities;
            }

            protected set
            {
            }
        }
    }
}