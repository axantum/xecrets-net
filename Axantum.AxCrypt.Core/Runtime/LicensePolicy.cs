using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
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
            LicenseCapability.Premium,
        });

        private LogOnIdentity _identity;

        public LicensePolicy(LogOnIdentity identity)
        {
            _identity = identity;
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
                DateTime utcNow = New<INow>().Utc;
                if (utcNow >= SubscriptionExpiration)
                {
                    return TimeSpan.Zero;
                }

                TimeSpan untilExpiration = SubscriptionExpiration - utcNow;
                if (untilExpiration > TimeSpan.FromDays(7))
                {
                    return TimeSpan.FromDays(7);
                }
                return untilExpiration;
            }
        }

        private ISet<LicenseCapability> Capabilities
        {
            get
            {
                return SubscriptionLevel == SubscriptionLevel.Premium && TimeLeftOffline > TimeSpan.Zero ? _premiumCapabilities : _freeCapabilities;
            }
        }

        protected virtual SubscriptionLevel SubscriptionLevel
        {
            get
            {
                if (_identity == LogOnIdentity.Empty)
                {
                    return SubscriptionLevel.Unknown;
                }
                return Account.SubscriptionLevel;
            }
        }

        protected virtual DateTime SubscriptionExpiration
        {
            get
            {
                if (_identity == LogOnIdentity.Empty)
                {
                    return DateTime.MaxValue;
                }
                return Account.LevelExpiration;
            }
        }

        private UserAccount Account
        {
            get
            {
                UserAccount account = Task.Run(async () => await New<LogOnIdentity, IAccountService>(_identity).AccountAsync().Free()).Result;
                return account;
            }
        }

        public bool Has(LicenseCapability capability)
        {
            return Capabilities.Contains(capability);
        }

        public TimeSpan SubscriptionWarningTime
        {
            get
            {
                SubscriptionLevel level = SubscriptionLevel;
                if (level == SubscriptionLevel.Unknown)
                {
                    return TimeSpan.MaxValue;
                }

                if (level == SubscriptionLevel.Free)
                {
                    return TimeSpan.Zero;
                }

                DateTime expiration = SubscriptionExpiration;
                if (expiration == DateTime.MaxValue || expiration == DateTime.MinValue)
                {
                    return TimeSpan.MaxValue;
                }
                DateTime utcNow = New<INow>().Utc;
                expiration = expiration < utcNow ? utcNow : expiration;

                TimeSpan timeLeft = expiration - utcNow;
                if (timeLeft < TimeSpan.FromDays(32))
                {
                    return timeLeft;
                }

                return TimeSpan.MaxValue;
            }
        }

        public ICryptoPolicy CryptoPolicy
        {
            get
            {
                return Has(LicenseCapability.StrongerEncryption) ? new ProCryptoPolicy() as ICryptoPolicy : new FreeCryptoPolicy() as ICryptoPolicy;
            }
        }
    }
}