using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Service;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Encapsulates the key and identity data associated with a log on. It may or may not contain asymmetric keys, but there's
    /// always a passphrase.
    /// </summary>
    public class LogOnIdentity : IEquatable<LogOnIdentity>
    {
        /// <summary>
        /// The empty, or undefined, LogOnIdentity instance
        /// </summary>
        public static readonly LogOnIdentity Empty = new LogOnIdentity(UserKeyPair.Empty, Passphrase.Empty);

        public LogOnIdentity(string passphraseText)
            : this(new Passphrase(passphraseText))
        {
        }

        public LogOnIdentity(EmailAddress userEmail, Passphrase passphrase)
            : this(passphrase)
        {
            UserEmail = userEmail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnIdentity"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public LogOnIdentity(Passphrase passphrase)
            : this(UserKeyPair.Empty, passphrase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnIdentity"/> class.
        /// </summary>
        /// <param name="userKeys">The user keys.</param>
        /// <param name="passphrase">The passphrase.</param>
        public LogOnIdentity(UserKeyPair userKeys, Passphrase passphrase)
        {
            if (userKeys == null)
            {
                throw new ArgumentNullException("userKeys");
            }

            UserKeys = userKeys;
            Passphrase = passphrase ?? Passphrase.Empty;
            UserEmail = userKeys.UserEmail;
        }

        private Passphrase _passphrase;

        /// <summary>
        /// Gets the passphrase.
        /// </summary>
        /// <value>
        /// The passphrase or Passphrase.Empty if not known.
        /// </value>
        public Passphrase Passphrase
        {
            get
            {
                return _passphrase ?? Passphrase.Empty;
            }
            private set
            {
                _passphrase = value;
            }
        }

        /// <summary>
        /// Gets the user email associated with the identity, or Empty if none.
        /// </summary>
        /// <value>
        /// The user email.
        /// </value>
        public EmailAddress UserEmail
        {
            get; private set;
        }

        /// <summary>
        /// Gets the user keys.
        /// </summary>
        /// <value>
        /// The user keys or null if not known.
        /// </value>
        public UserKeyPair UserKeys
        {
            get;
            private set;
        }

        public IEnumerable<IAsymmetricPrivateKey> PrivateKeys
        {
            get
            {
                if (UserKeys == UserKeyPair.Empty)
                {
                    return new IAsymmetricPrivateKey[0];
                }
                return new IAsymmetricPrivateKey[] { UserKeys.KeyPair.PrivateKey, };
            }
        }

        public IEnumerable<UserPublicKey> PublicKeys
        {
            get
            {
                if (UserKeys == UserKeyPair.Empty)
                {
                    return new UserPublicKey[0];
                }
                return new UserPublicKey[] { new UserPublicKey(UserKeys.UserEmail, UserKeys.KeyPair.PublicKey), };
            }
        }

        public IdentityPublicTag Tag
        {
            get
            {
                return new IdentityPublicTag(this);
            }
        }

        public bool Equals(LogOnIdentity other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return Passphrase == other.Passphrase && UserKeys == other.UserKeys;
        }

        public override bool Equals(object obj)
        {
            LogOnIdentity other = obj as LogOnIdentity;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Passphrase.GetHashCode() ^ UserKeys.GetHashCode();
        }

        public static bool operator ==(LogOnIdentity left, LogOnIdentity right)
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

        public static bool operator !=(LogOnIdentity left, LogOnIdentity right)
        {
            return !(left == right);
        }
    }
}