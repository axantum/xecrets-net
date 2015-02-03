using Axantum.AxCrypt.Core.Crypto.Asymmetric;
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
        public static readonly LogOnIdentity Empty = new LogOnIdentity(Passphrase.Empty);

        public LogOnIdentity(string passphraseText)
            : this(new Passphrase(passphraseText))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnIdentity"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public LogOnIdentity(Passphrase passphrase)
            : this(passphrase, EmailAddress.Empty, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogOnIdentity"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        /// <param name="userEmail">The user email.</param>
        /// <param name="keyPair">The key pair.</param>
        public LogOnIdentity(Passphrase passphrase, EmailAddress userEmail, IAsymmetricKeyPair keyPair)
        {
            Passphrase = passphrase;
            UserEmail = userEmail;
            KeyPair = keyPair;
        }

        /// <summary>
        /// Gets the passphrase.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; private set; }

        /// <summary>
        /// Gets the user email.
        /// </summary>
        /// <value>
        /// The user email or EmailAddress.Empty if none.
        /// </value>
        public EmailAddress UserEmail { get; private set; }

        /// <summary>
        /// Gets the key pair.
        /// </summary>
        /// <value>
        /// The key pair, or null if none.
        /// </value>
        public IAsymmetricKeyPair KeyPair { get; private set; }

        public IEnumerable<IAsymmetricPublicKey> PublicKeys
        {
            get
            {
                if (KeyPair == null)
                {
                    return new IAsymmetricPublicKey[0];
                }

                return new IAsymmetricPublicKey[] { KeyPair.PublicKey };
            }
        }

        public IEnumerable<IAsymmetricPrivateKey> PrivateKeys
        {
            get
            {
                if (KeyPair == null)
                {
                    return new IAsymmetricPrivateKey[0];
                }

                return new IAsymmetricPrivateKey[] { KeyPair.PrivateKey };
            }
        }

        public bool Equals(LogOnIdentity other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return Passphrase.Equals(other.Passphrase) && UserEmail.Equals(other.UserEmail) && (Object.ReferenceEquals(KeyPair, other.KeyPair) || (KeyPair != null && KeyPair.Equals(other.KeyPair)));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !typeof(LogOnIdentity).IsAssignableFrom(obj.GetType()))
            {
                return false;
            }
            LogOnIdentity other = (LogOnIdentity)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Passphrase.GetHashCode() ^ UserEmail.GetHashCode() ^ KeyPair.GetHashCode();
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