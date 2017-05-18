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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
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
    public class LicensePolicy : IDisposable, IEquatable<LicensePolicy>
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

        public LicensePolicy()
        {
            New<SessionNotify>().AddPriorityCommand(LicensePolicy_CommandAsync);
        }

        private async Task LicensePolicy_CommandAsync(SessionNotification notification)
        {
            switch (notification.NotificationType)
            {
                case SessionNotificationType.LogOn:
                case SessionNotificationType.LicensePolicyChange:
                    await RefreshAsync(notification.Identity);
                    break;

                case SessionNotificationType.LogOff:
                case SessionNotificationType.SessionStart:
                    await RefreshAsync(LogOnIdentity.Empty);
                    break;

                default:
                    break;
            }
        }

        protected virtual async Task RefreshAsync(LogOnIdentity identity)
        {
            Capabilities = new LicenseCapabilities(await CapabilitiesAsync(identity).Free());
        }

        protected async Task<UserAccount> UserAccountAsync(LogOnIdentity identity)
        {
            return await New<LogOnIdentity, IAccountService>(identity).AccountAsync().Free();
        }

        /// <summary>
        /// Gets the time left offline at this subscription level until a revalidation is required.
        /// </summary>
        /// <value>
        /// The time left offline.
        /// </value>
        private async Task<TimeSpan> TimeLeftOfflineAsync(LogOnIdentity identity)
        {
            DateTime utcNow = New<INow>().Utc;
            if (utcNow >= await SubscriptionExpirationAsync(identity).Free())
            {
                return TimeSpan.Zero;
            }

            TimeSpan untilExpiration = await SubscriptionExpirationAsync(identity).Free() - utcNow;
            if (untilExpiration > TimeSpan.FromDays(7))
            {
                return TimeSpan.FromDays(7);
            }
            return untilExpiration;
        }

        private async Task<ISet<LicenseCapability>> CapabilitiesAsync(LogOnIdentity identity)
        {
            return await SubscriptionLevelAsync(identity) == SubscriptionLevel.Premium && await TimeLeftOfflineAsync(identity).Free() > TimeSpan.Zero ? _premiumCapabilities : _freeCapabilities;
        }

        private async Task<SubscriptionLevel> SubscriptionLevelAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return SubscriptionLevel.Unknown;
            }
            return (await UserAccountAsync(identity).Free()).SubscriptionLevel;
        }

        private async Task<DateTime> SubscriptionExpirationAsync(LogOnIdentity identity)
        {
            if (identity == LogOnIdentity.Empty)
            {
                return DateTime.MaxValue;
            }
            return (await UserAccountAsync(identity).Free()).LevelExpiration;
        }

        protected static LicenseCapabilities FreeCapabilities = new LicenseCapabilities(_freeCapabilities);

        protected static LicenseCapabilities PremiumCapabilities = new LicenseCapabilities(_premiumCapabilities);

        public LicenseCapabilities Capabilities { get; protected set; } = FreeCapabilities;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                New<SessionNotify>().RemovePriorityCommand(LicensePolicy_CommandAsync);
                disposed = true;
            }
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