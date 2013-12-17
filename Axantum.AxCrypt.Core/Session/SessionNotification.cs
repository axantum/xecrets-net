#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionNotification : IEquatable<SessionNotification>
    {
        public AesKey Key { get; private set; }

        public string FullName { get; private set; }

        public SessionNotificationType NotificationType { get; private set; }

        public SessionNotification(SessionNotificationType notificationType, AesKey key, string fullName)
        {
            NotificationType = notificationType;
            Key = key;
            FullName = fullName;
        }

        public SessionNotification(SessionNotificationType notificationType, string fullName)
            : this(notificationType, null, fullName)
        {
        }

        public SessionNotification(SessionNotificationType notificationType, AesKey key)
            : this(notificationType, key, null)
        {
        }

        public SessionNotification(SessionNotificationType notificationType)
            : this(notificationType, null, null)
        {
        }

        #region IEquatable<AesKey> Members

        public bool Equals(SessionNotification other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (other.NotificationType != NotificationType)
            {
                return false;
            }

            if (other.Key != Key)
            {
                return false;
            }

            if (other.FullName != FullName)
            {
                return false;
            }

            return true;
        }

        #endregion IEquatable<AesKey> Members

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(SessionNotification) != obj.GetType())
            {
                return false;
            }
            SessionNotification other = (SessionNotification)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode;

            hashcode = Key != null ? Key.GetHashCode() : 0;
            hashcode ^= FullName != null ? FullName.GetHashCode() : 0;
            hashcode ^= NotificationType.GetHashCode();

            return hashcode;
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