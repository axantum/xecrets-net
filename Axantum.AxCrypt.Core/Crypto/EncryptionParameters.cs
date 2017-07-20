#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Extensions;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Collect the parameters required for encryption, especially the public keys and passphrase.
    /// </summary>
    public class EncryptionParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionParameters"/> class.
        /// </summary>
        public EncryptionParameters(Guid cryptoId)
        {
            CryptoId = cryptoId;
            _publicKeys = new List<UserPublicKey>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionParameters"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public EncryptionParameters(Guid cryptoId, Passphrase passphrase)
            : this(cryptoId)
        {
            Passphrase = passphrase;
        }

        public EncryptionParameters(Guid cryptoId, LogOnIdentity identity)
            : this(cryptoId)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            Passphrase = identity.Passphrase;
            Add(identity.PublicKeys);
        }

        public static async Task<EncryptionParameters> CreateAsync(Guid cryptoId, LogOnIdentity identity, IEnumerable<EmailAddress> shares)
        {
            EncryptionParameters _encryptionParameters = new EncryptionParameters(cryptoId, identity);

            if (!shares.Any())
            {
                return _encryptionParameters;
            }

            List<UserPublicKey> keyShares = new List<UserPublicKey>();
            using (KnownPublicKeys knownPublicKeys = New<KnownPublicKeys>())
            {
                foreach (EmailAddress email in shares)
                {
                    UserPublicKey key = await knownPublicKeys.GetAsync(email);
                    if (key != null)
                    {
                        keyShares.Add(key);
                    }
                }
                _encryptionParameters.Add(keyShares);
            }
            return _encryptionParameters;
        }

       

        public void Add(IEnumerable<UserPublicKey> publicKeys)
        {
            _publicKeys.AddRange(publicKeys.Where(pk => !_publicKeys.Contains(pk)));
        }

        

        /// <summary>
        /// An empty set of encryption parameters.
        /// </summary>
        public static readonly EncryptionParameters Empty = new EncryptionParameters(Guid.Empty, Passphrase.Empty);

        /// <summary>
        /// Gets or sets the passphrase. A passphrase is always required.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; set; }

        private List<UserPublicKey> _publicKeys;

        /// <summary>
        /// Gets or sets the public keys to also use to encrypt the session key with.
        /// </summary>
        /// <value>
        /// The public keys. The enumeration may be empty.
        /// </value>
        public IEnumerable<UserPublicKey> PublicKeys
        {
            get
            {
                return _publicKeys;
            }
        }

        /// <summary>
        /// Gets or sets the crypto identifier to use for the encryption.
        /// </summary>
        /// <value>
        /// The crypto identifier.
        /// </value>
        public Guid CryptoId { get; set; }
    }
}