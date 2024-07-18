#region Coypright and License

/*
 * AxCrypt - Copyright 2017, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Runtime
{
    /// <summary>
    /// Handle license policy based on the current licensing and subscription status. All instance methods are thread safe.
    /// </summary>
    public class LicensePolicy : IDisposable, IEquatable<LicensePolicy>
    {
        protected static readonly HashSet<LicenseCapability> FreeCapabilitySet = new HashSet<LicenseCapability>(new LicenseCapability[]
        {
            LicenseCapability.StandardEncryption,
            LicenseCapability.AccountKeyBackup,
            LicenseCapability.CommunitySupport,
            LicenseCapability.EncryptNewFiles,
            LicenseCapability.EditExistingFiles,
            LicenseCapability.ShareSecretFree,
        });

        protected static readonly HashSet<LicenseCapability> PasswordManagerCapabilitySet = new HashSet<LicenseCapability>(FreeCapabilitySet.Concat(new LicenseCapability[]
        {
            LicenseCapability.PasswordManagement,
            LicenseCapability.PasswordGeneration,
            LicenseCapability.PasswordManager,
            LicenseCapability.ShareSecretPasswordManager,
        }));

        protected static readonly HashSet<LicenseCapability> PremiumCapabilitySet = new HashSet<LicenseCapability>(FreeCapabilitySet.Concat(new LicenseCapability[]
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
            LicenseCapability.IncludeSubfolders,
            LicenseCapability.Premium,
            LicenseCapability.InactivitySignOut,
            LicenseCapability.ShareSecretPremium,
        }));

        protected static readonly HashSet<LicenseCapability> BusinessCapabilitySet = new HashSet<LicenseCapability>(PremiumCapabilitySet.Concat(new LicenseCapability[]
        {
            LicenseCapability.Business,
        }));

        public LicensePolicy() : this(true)
        {
        }

        private bool _handleNotifications;

        protected LicensePolicy(bool handleNotifications)
        {
            _handleNotifications = handleNotifications;
            if (handleNotifications)
            {
                New<SessionNotify>().AddPriorityCommand(LicensePolicy_CommandAsync);
            }
        }

        private async Task LicensePolicy_CommandAsync(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.RefreshLicensePolicy:
                case SessionNotificationType.SignIn:
                    await RefreshAsync(notification.Identity);
                    break;

                case SessionNotificationType.SessionStart:
                case SessionNotificationType.SignOut:
                    await RefreshAsync(LogOnIdentity.Empty);
                    break;

                default:
                    break;
            }
        }

        private LicenseCapabilities _mostRecentCapabilities = null;

        public async Task RefreshAsync(LogOnIdentity identity)
        {
            Expiration = await SubscriptionExpirationAsync(identity).Free();

            Capabilities = await CapabilitiesAsync(identity).Free();
            if (Capabilities != _mostRecentCapabilities)
            {
                _mostRecentCapabilities = Capabilities;
                await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.LicensePolicyChanged, identity, Capabilities));
            }
        }

        private async Task<UserAccount> UserAccountAsync(LogOnIdentity identity)
        {
            return await New<LogOnIdentity, IAccountService>(identity).AccountAsync().Free();
        }

        /// <summary>
        /// Gets the time left at this subscription level
        /// </summary>
        /// <value>
        /// The time left offline.
        /// </value>
        private async Task<TimeSpan> TimeUntilSubscriptionExpiration(LogOnIdentity identity)
        {
            DateTime utcNow = New<INow>().Utc;
            if (utcNow >= await SubscriptionExpirationAsync(identity).Free())
            {
                return TimeSpan.Zero;
            }

            TimeSpan untilExpiration = await SubscriptionExpirationAsync(identity).Free() - utcNow;
            return untilExpiration;
        }

        private async Task<LicenseCapabilities> CapabilitiesAsync(LogOnIdentity identity)
        {
            SubscriptionLevel level = await SubscriptionLevelAsync(identity).Free();
            switch (level)
            {
                case SubscriptionLevel.Business:
                    if (await TimeUntilSubscriptionExpiration(identity).Free() > TimeSpan.Zero)
                    {
                        return BusinessCapabilities;
                    }
                    break;

                case SubscriptionLevel.Premium:
                    if (await TimeUntilSubscriptionExpiration(identity).Free() > TimeSpan.Zero)
                    {
                        return PremiumCapabilities;
                    }
                    break;

                case SubscriptionLevel.PasswordManager:
                    if (await TimeUntilSubscriptionExpiration(identity).Free() > TimeSpan.Zero)
                    {
                        return PasswordManagerCapabilities;
                    }
                    break;

                default:
                    break;
            }

            return FreeCapabilities;
        }

        private async Task<SubscriptionLevel> SubscriptionLevelAsync(LogOnIdentity identity)
        {
            if (identity.UserEmail == EmailAddress.Empty)
            {
                return SubscriptionLevel.Unknown;
            }
            return (await (await UserAccountAsync(identity).Free()).ValidatedLevelAsync().Free());
        }

        private async Task<DateTime> SubscriptionExpirationAsync(LogOnIdentity identity)
        {
            if (identity.UserEmail == EmailAddress.Empty)
            {
                return DateTime.MaxValue;
            }
            return (await UserAccountAsync(identity).Free()).LevelExpiration;
        }

        protected virtual LicenseCapabilities FreeCapabilities { get { return new LicenseCapabilities(FreeCapabilitySet); } }

        protected virtual LicenseCapabilities PasswordManagerCapabilities { get { return new LicenseCapabilities(PasswordManagerCapabilitySet); } }

        protected virtual LicenseCapabilities PremiumCapabilities { get { return new LicenseCapabilities(PremiumCapabilitySet); } }

        protected virtual LicenseCapabilities BusinessCapabilities { get { return new LicenseCapabilities(BusinessCapabilitySet); } }

        private LicenseCapabilities _currentLicenseCapabilities = null;

        public virtual LicenseCapabilities Capabilities
        {
            get
            {
                if (_currentLicenseCapabilities == null)
                {
                    _currentLicenseCapabilities = FreeCapabilities;
                }
                return _currentLicenseCapabilities;
            }
            protected set
            {
                _currentLicenseCapabilities = value;
            }
        }

        public DateTime Expiration { get; private set; } = DateTime.MaxValue;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || disposed)
            {
                return;
            }

            if (_handleNotifications)
            {
                New<SessionNotify>().RemovePriorityCommand(LicensePolicy_CommandAsync);
                _handleNotifications = false;
            }
            disposed = true;
        }

        #region IEquatable<LicensePolicy> Members

        public bool Equals(LicensePolicy other)
        {
            if ((object)other == null)
            {
                return false;
            }
            if (Capabilities != other.Capabilities)
            {
                return false;
            }
            return true;
        }

        #endregion IEquatable<LicensePolicy> Members

        public override bool Equals(object obj)
        {
            LicensePolicy other = obj as LicensePolicy;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Capabilities.GetHashCode();
        }

        public static bool operator ==(LicensePolicy left, LicensePolicy right)
        {
            if (Object.ReferenceEquals(left, right))
            {
                return true;
            }
            if ((object)left == null)
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(LicensePolicy left, LicensePolicy right)
        {
            return !(left == right);
        }
    }
}