using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Session
{
    public class SessionEvent : IEquatable<SessionEvent>
    {
        public AesKey Key { get; private set; }

        public string FullName { get; private set; }

        public SessionEventType SessionEventType { get; private set; }

        public SessionEvent(SessionEventType sessionEventType, AesKey key, string fullName)
        {
            SessionEventType = sessionEventType;
            Key = key;
            FullName = fullName;
        }

        public SessionEvent(SessionEventType sessionEventType, string fullName)
            : this(sessionEventType, null, fullName)
        {
        }

        public SessionEvent(SessionEventType sessionEventType, AesKey key)
            : this(sessionEventType, key, null)
        {
        }

        public SessionEvent(SessionEventType sessionEventType)
            : this(sessionEventType, null, null)
        {
        }

        #region IEquatable<AesKey> Members

        public bool Equals(SessionEvent other)
        {
            if ((object)other == null)
            {
                return false;
            }

            if (other.SessionEventType != SessionEventType)
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
            if (obj == null || typeof(SessionEvent) != obj.GetType())
            {
                return false;
            }
            SessionEvent other = (SessionEvent)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            int hashcode;
            
            hashcode = Key != null ? Key.GetHashCode() : 0;
            hashcode ^= FullName != null ? FullName.GetHashCode() : 0;
            hashcode ^= SessionEventType.GetHashCode();

            return hashcode;
        }

        public static bool operator ==(SessionEvent left, SessionEvent right)
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

        public static bool operator !=(SessionEvent left, SessionEvent right)
        {
            return !(left == right);
        }
    }
}
