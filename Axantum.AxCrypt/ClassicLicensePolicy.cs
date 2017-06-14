using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt
{
    internal class ClassicLicensePolicy : LicensePolicy
    {
        private static readonly HashSet<LicenseCapability> _classicAdditionalFreeCapabilitySet = new HashSet<LicenseCapability>(new LicenseCapability[]
        {
            LicenseCapability.EncryptNewFiles,
            LicenseCapability.EditExistingFiles,
        });

        private readonly HashSet<LicenseCapability> _classicFreeCapabilitySet;

        public ClassicLicensePolicy() : base(true)
        {
            _classicFreeCapabilitySet = new HashSet<LicenseCapability>(FreeCapabilitySet.Union(_classicAdditionalFreeCapabilitySet));
        }

        protected override LicenseCapabilities FreeCapabilities
        {
            get
            {
                return new LicenseCapabilities(_classicFreeCapabilitySet);
            }
        }
    }
}