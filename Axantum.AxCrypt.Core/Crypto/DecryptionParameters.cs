using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Collect the parameters for decryption
    /// </summary>
    public class DecryptionParameters
    {
        /// <summary>
        /// Gets or sets the passphrase. A passphrase is optional.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; set; }

        /// <summary>
        /// Gets or sets the private keys to also try to use for decryption.
        /// </summary>
        /// <value>
        /// The private keys. The enumeration may be empty.
        /// </value>
        public IEnumerable<IAsymmetricPrivateKey> PrivateKeys { get; set; }

        /// <summary>
        /// Gets or sets the crypto identifiers to try to use for the decryption.
        /// </summary>
        /// <value>
        /// The crypto identifiers.
        /// </value>
        public IEnumerable<Guid> CryptoIds { get; set; }
    }
}