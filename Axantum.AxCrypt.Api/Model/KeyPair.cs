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
        [JsonConstructor()]
        private KeyPair()
        {
        }

        /// <summary>
        /// The empty instance.
        /// </summary>
        public static readonly KeyPair Empty = new KeyPair(String.Empty, String.Empty);

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPair"/> class.
        /// </summary>
        /// <param name="publicBytes">The public bytes.</param>
        /// <param name="privateBytes">The private bytes.</param>
        /// <exception cref="System.ArgumentNullException">
        /// publicBytes
        /// or
        /// privateBytes
        /// </exception>
        public KeyPair(string publicBytes, string privateBytes)
        {
            if (publicBytes == null)
            {
                throw new ArgumentNullException("publicBytes");
            }
            if (privateBytes == null)
            {
                throw new ArgumentNullException("privateBytes");
            }

            PublicBytes = publicBytes;
            PrivateBytes = privateBytes;
        }

        /// <summary>
        /// Gets the public key bytes.
        /// </summary>
        /// <value>
        /// The public key bytes, base64 encoded.
        /// </value>
        [JsonProperty("public")]
        public string PublicBytes { get; private set; }

        /// <summary>
        /// Gets the AxCrypt-encrypted private key bytes.
        /// </summary>
        /// <value>
        /// In order to minimize exposure of the keys on the server, the private key is stored as an
        /// AxCrypt-encrypted blob. This also enables the future possibility to have the server operate
        /// on zero knowledge of the private keys. It is Base64-encoded.
        /// </value>
        [JsonProperty("private")]
        public string PrivateBytes { get; private set; }

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
                return PublicBytes.Length == 0 && PrivateBytes.Length == 0;
            }
        }

        public bool Equals(KeyPair other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return PublicBytes == other.PublicBytes && PrivateBytes == other.PrivateBytes;
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
            return PublicBytes.GetHashCode() ^ PrivateBytes.GetHashCode();
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