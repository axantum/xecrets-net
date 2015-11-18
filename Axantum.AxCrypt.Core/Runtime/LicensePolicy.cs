using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    /// <summary>
    /// Handle license policy based on the current licensing and subscription status. All instance methods are thread safe.
    /// </summary>
    public class LicensePolicy
    {
        private static readonly HashSet<LicenseCapability> _freeCapabilities = new HashSet<LicenseCapability>(new LicenseCapability[]
        {
            LicenseCapability.StandardEncryption,
            LicenseCapability.AccountKeyBackup,
            LicenseCapability.CommunitySupport,
        });

        private static readonly HashSet<LicenseCapability> _premiumCapabilities = new HashSet<LicenseCapability>(new LicenseCapability[]
        {
            LicenseCapability.SecureWipe,
            LicenseCapability.StrongerEncryption,
            LicenseCapability.KeySharing,
            LicenseCapability.RandomRename,
            LicenseCapability.SecureFolders,
            LicenseCapability.CloudStorageAwareness,
            LicenseCapability.PasswordManagement,
            LicenseCapability.PasswordGeneration,
            LicenseCapability.DirectSupport,
            LicenseCapability.StandardEncryption,
            LicenseCapability.AccountKeyBackup,
            LicenseCapability.CommunitySupport,
        });

        private LogOnIdentity _identity;

        public LicensePolicy(LogOnIdentity identity)
        {
            _identity = identity;
        }

        /// <summary>
        /// Gets the time left at the current subscription level.
        /// </summary>
        /// <value>
        /// The time left.
        /// </value>
        private TimeSpan TimeLeft
        {
            get
            {
                return TimeSpan.MaxValue;
            }
        }

        /// <summary>
        /// Gets the time left offline at this subscription level until a revalidation is required.
        /// </summary>
        /// <value>
        /// The time left offline.
        /// </value>
        private TimeSpan TimeLeftOffline
        {
            get
            {
                return TimeSpan.MaxValue;
            }
        }

        public ISet<LicenseCapability> Capabilities
        {
            get
            {
                return IsPremium ? _premiumCapabilities : _freeCapabilities;
            }
        }

        public bool IsPremium
        {
            get { return SubscriptionLevel == SubscriptionLevel.Premium; }
        }

        public virtual SubscriptionLevel SubscriptionLevel
        {
            get { return Task.Run(() => New<LogOnIdentity, IAccountService>(_identity).LevelAsync()).Result; }
        }

        public bool Has(LicenseCapability capability)
        {
            return Capabilities.Contains(capability);
        }

        public ICryptoPolicy CryptoPolicy
        {
            get
            {
                return IsPremium ? New<CryptoPolicy>().CreateDefault() : New<CryptoPolicy>().Create(new FreeCryptoPolicy().Name);
            }
        }
    }
}