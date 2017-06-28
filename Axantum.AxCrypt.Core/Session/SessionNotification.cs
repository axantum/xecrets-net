#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotification : IEquatable<SessionNotification>
    {
        public LogOnIdentity Identity { get; private set; }

        public LicenseCapabilities Capabilities { get; private set; }

        public IEnumerable<string> FullNames { get; private set; }

        public SessionNotificationType NotificationType { get; private set; }

        public SessionNotification(SessionNotificationType notificationType, LogOnIdentity identity, IEnumerable<string> fullNames, LicenseCapabilities capabilities)
        {
            NotificationType = notificationType;
            Identity = identity;
            FullNames = fullNames.Select(fn => fn.NormalizeFilePath());
            Capabilities = capabilities;
        }

        public SessionNotification(SessionNotificationType notificationType, LogOnIdentity identity, IEnumerable<string> fullNames)
            : this(notificationType, identity, fullNames, New<LicensePolicy>().Capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, LogOnIdentity identity, string fullName)
            : this(notificationType, identity, new string[] { fullName }, New<LicensePolicy>().Capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, IEnumerable<string> fullNames)
            : this(notificationType, LogOnIdentity.Empty, fullNames, New<LicensePolicy>().Capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, string fullName)
            : this(notificationType, LogOnIdentity.Empty, new string[] { fullName }, New<LicensePolicy>().Capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, LogOnIdentity identity)
            : this(notificationType, identity, new string[0], New<LicensePolicy>().Capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, LogOnIdentity identity, LicenseCapabilities capabilities)
            : this(notificationType, identity, new string[0], capabilities)
        {
        }

        public SessionNotification(SessionNotificationType notificationType)
            : this(notificationType, LogOnIdentity.Empty, new string[0], New<LicensePolicy>().Capabilities)
        {
        }

        public bool Equals(SessionNotification other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return NotificationType == other.NotificationType && Identity == other.Identity && Capabilities == other.Capabilities && FullNames.SequenceEqual(other.FullNames);
        }

        public override bool Equals(object obj)
        {
            SessionNotification other = obj as SessionNotification;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return NotificationType.GetHashCode() ^ Identity.GetHashCode() ^ Capabilities.GetHashCode() ^ FullNames.Aggregate(0, (v, s) => v ^ s.GetHashCode());
        }

        public static bool operator ==(SessionNotification left, SessionNotification right)
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

        public static bool operator !=(SessionNotification left, SessionNotification right)
        {
            return !(left == right);
        }
    }
}