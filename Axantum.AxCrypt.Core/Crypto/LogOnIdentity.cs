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
        public static readonly LogOnIdentity Empty = new LogOnIdentity(new UserKeyPair[0], Passphrase.Empty);

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
            : this(new UserKeyPair[0], passphrase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnIdentity"/> class.
        /// </summary>
        /// <param name="keyPairs">The user keys.</param>
        /// <param name="passphrase">The passphrase.</param>
        public LogOnIdentity(IEnumerable<UserKeyPair> keyPairs, Passphrase passphrase)
        {
            KeyPairs = keyPairs ?? throw new ArgumentNullException("userKeys");
            Passphrase = passphrase ?? Passphrase.Empty;
            UserEmail = (keyPairs.FirstOrDefault() ?? UserKeyPair.Empty).UserEmail;
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
        public IEnumerable<UserKeyPair> KeyPairs
        {
            get;
            private set;
        }

        public UserKeyPair ActiveEncryptionKeyPair
        {
            get
            {
                return KeyPairs.FirstOrDefault() ?? UserKeyPair.Empty;
            }
        }

        public IEnumerable<IAsymmetricPrivateKey> PrivateKeys
        {
            get
            {
                if (!KeyPairs.Any())
                {
                    return new IAsymmetricPrivateKey[0];
                }
                return KeyPairs.Select(uk => uk.KeyPair.PrivateKey);
            }
        }

        public IEnumerable<UserPublicKey> PublicKeys
        {
            get
            {
                if (!KeyPairs.Any())
                {
                    return new UserPublicKey[0];
                }
                return KeyPairs.Select(uk => new UserPublicKey(uk.UserEmail, uk.KeyPair.PublicKey));
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
            return UserEmail == other.UserEmail && Passphrase == other.Passphrase && KeyPairs.SequenceEqual(other.KeyPairs);
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
            return Passphrase.GetHashCode() ^ KeyPairs.GetHashCode();
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