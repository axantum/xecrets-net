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
        public FreeForcedLicensePolicy() : base(false)
        {
        }

        public override LicenseCapabilities Capabilities
        {
            get
            {
                return FreeCapabilities;
            }

            protected set
            {
            }
        }
    }
}