using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KeyPair : IEquatable<KeyPair>
    {
        /// <summary>
        /// The empty instance.
        /// </summary>
        public static readonly KeyPair Empty = new KeyPair(String.Empty, String.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPair"/> class.
        /// </summary>
        /// <param name="public">The public key as a PEM string.</param>
        /// <param name="private">The private key as a Basea64-encoded AxCrypt-encrypted PEM string.</param>
        /// <exception cref="System.ArgumentNullException">
        /// publicPem
        /// or
        /// privateAxCryptPem
        /// </exception>
        [JsonConstructor]
        public KeyPair(string @public, string @private)
        {
            if (@public == null)
            {
                throw new ArgumentNullException(nameof(@public));
            }
            if (@private == null)
            {
                throw new ArgumentNullException(nameof(@private));
            }

            PublicPem = @public;
            PrivateEncryptedPem = @private;
        }

        /// <summary>
        /// Gets the public key bytes.
        /// </summary>
        /// <value>
        /// The public key bytes, base64 encoded.
        /// </value>
        [JsonProperty("public")]
        public string PublicPem { get; private set; }

        /// <summary>
        /// Gets the AxCrypt-encrypted private key PEM.
        /// </summary>
        /// <value>
        /// In order to minimize exposure of the keys on the server, the private key is stored as an
        /// encrypted blob. This also enables the future possibility to have the server operate
        /// on zero knowledge of the private keys. It is Base64-encoded.
        /// </value>
        [JsonProperty("private")]
        public string PrivateEncryptedPem { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get
            {
                return PublicPem.Length == 0 && PrivateEncryptedPem.Length == 0;
            }
        }

        public bool Equals(KeyPair other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return PublicPem == other.PublicPem && PrivateEncryptedPem.Length == other.PrivateEncryptedPem.Length;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || typeof(KeyPair) != obj.GetType())
            {
                return false;
            }
            KeyPair other = (KeyPair)obj;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return PublicPem.GetHashCode();
        }

        public static bool operator ==(KeyPair left, KeyPair right)
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

        public static bool operator !=(KeyPair left, KeyPair right)
        {
            return !(left == right);
        }
    }
}