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
        /// Initializes a new instance of the <see cref="DecryptionParameters"/> class.
        /// </summary>
        /// <param name="cryptoId">The crypto identifier.</param>
        /// <param name="passphrase">The passphrase.</param>
        public DecryptionParameters(Passphrase passphrase, IEnumerable<IAsymmetricPrivateKey> privateKeys, IEnumerable<Guid> cryptoIds)
        {
            Passphrase = passphrase;
            _privateKeys.AddRange(privateKeys);
            _cryptoIds.AddRange(cryptoIds);
        }

        /// <summary>
        /// Gets or sets the passphrase. A passphrase is optional.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; set; }

        private List<IAsymmetricPrivateKey> _privateKeys = new List<IAsymmetricPrivateKey>();

        /// <summary>
        /// Gets or sets the private keys to also try to use for decryption.
        /// </summary>
        /// <value>
        /// The private keys. The enumeration may be empty.
        /// </value>
        public IEnumerable<IAsymmetricPrivateKey> PrivateKeys
        {
            get
            {
                return _privateKeys;
            }
        }

        private List<Guid> _cryptoIds = new List<Guid>();

        /// <summary>
        /// Gets the crypto identifiers to try to use for the decryption.
        /// </summary>
        /// <value>
        /// The crypto identifiers.
        /// </value>
        public IEnumerable<Guid> CryptoIds
        {
            get
            {
                return _cryptoIds;
            }
        }
    }
}