using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KeyPair
    {
        public static readonly KeyPair Empty = new KeyPair();

        public KeyPair()
            : this(String.Empty, String.Empty)
        {
        }

        public KeyPair(string thumbprint, string axCryptBytes)
        {
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }
            if (axCryptBytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            Thumbprint = thumbprint;
            AxCryptBytes = axCryptBytes;
        }

        /// <summary>
        /// Gets the timestamp, when the key pair was last encrypted (not generated). The thumbprint
        /// remains as the unique identifier to recognize identical key pairs, but the timestamp
        /// determines the most recent encryption, i.e. the most recent should be preferred.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Gets the thumbprint.
        /// </summary>
        /// <value>
        /// The thumbprint of the public key in the key pair for identification purposes.
        /// </value>
        [JsonProperty("thumbprint")]
        public string Thumbprint { get; private set; }

        /// <summary>
        /// Gets the AxCrypt bytes.
        /// </summary>
        /// <value>
        /// In order to minimize exposure of the keys on the server, the key pair is stored as an
        /// AxCrypt-encrypted blob. This also enables the future possibility to have the server operate
        /// on zero knowledge of the private keys.
        /// </value>
        [JsonProperty("bytes")]
        public string AxCryptBytes { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return Thumbprint.Length == 0 && AxCryptBytes.Length == 0;
            }
        }
    }
}