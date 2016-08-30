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

        private Task<UserAccount> _accountTask;

        protected async Task<UserAccount> UserAccountAsync()
        {
            if (_accountTask == null)
            {
                _accountTask = New<LogOnIdentity, IAccountService>(_identity).AccountAsync();
            }

            return await _accountTask.Free();
        }

        /// <summary>
        /// Gets the time left offline at this subscription level until a revalidation is required.
        /// </summary>
        /// <value>
        /// The time left offline.
        /// </value>
        private async Task<TimeSpan> TimeLeftOfflineAsync()
        {
            DateTime utcNow = New<INow>().Utc;
            if (utcNow >= await SubscriptionExpirationAsync().Free())
            {
                return TimeSpan.Zero;
            }

            TimeSpan untilExpiration = await SubscriptionExpirationAsync().Free() - utcNow;
            if (untilExpiration > TimeSpan.FromDays(7))
            {
                return TimeSpan.FromDays(7);
            }
            return untilExpiration;
        }

        private async Task<ISet<LicenseCapability>> CapabilitiesAsync()
        {
            return await SubscriptionLevelAsync() == SubscriptionLevel.Premium && await TimeLeftOfflineAsync().Free() > TimeSpan.Zero ? _premiumCapabilities : _freeCapabilities;
        }

        protected virtual async Task<SubscriptionLevel> SubscriptionLevelAsync()
        {
            if (_identity == LogOnIdentity.Empty)
            {
                return SubscriptionLevel.Unknown;
            }
            return (await UserAccountAsync().Free()).SubscriptionLevel;
        }

        protected virtual async Task<DateTime> SubscriptionExpirationAsync()
        {
            if (_identity == LogOnIdentity.Empty)
            {
                return DateTime.MaxValue;
            }
            return (await UserAccountAsync().Free()).LevelExpiration;
        }

        public async Task<bool> HasAsync(LicenseCapability capability)
        {
            return (await CapabilitiesAsync()).Contains(capability);
        }

        public async Task<TimeSpan> SubscriptionWarningTimeAsync()
        {
            SubscriptionLevel level = await SubscriptionLevelAsync().Free();
            if (level == SubscriptionLevel.Unknown)
            {
                return TimeSpan.Zero;
            }

            if (level == SubscriptionLevel.Free)
            {
                return TimeSpan.Zero;
            }

            DateTime expiration = await SubscriptionExpirationAsync().Free();
            if (expiration == DateTime.MaxValue || expiration == DateTime.MinValue)
            {
                return TimeSpan.MaxValue;
            }
            DateTime utcNow = New<INow>().Utc;
            expiration = expiration < utcNow ? utcNow : expiration;

            TimeSpan timeLeft = expiration - utcNow;
            if (timeLeft < TimeSpan.FromDays(15))
            {
                return timeLeft;
            }

            return TimeSpan.MaxValue;
        }

        public virtual async Task<bool> IsTrialAvailableAsync()
        {
            if (_identity == LogOnIdentity.Empty)
            {
                return false;
            }
            bool trialAvailable = !(await UserAccountAsync().Free()).Offers.HasFlag(Offers.AxCryptTrial);
            return trialAvailable;
        }

        public async Task<ICryptoPolicy> CryptoPolicyAsync()
        {
            return await HasAsync(LicenseCapability.StrongerEncryption).Free() ? new ProCryptoPolicy() as ICryptoPolicy : new FreeCryptoPolicy() as ICryptoPolicy;
        }
    }
}