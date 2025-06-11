﻿#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// Collect the parameters required for encryption, especially the public keys and passphrase.
    /// </summary>
    public class EncryptionParameters
    {
        private LogOnIdentity? _identity;

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionParameters"/> class.
        /// </summary>
        public EncryptionParameters(Guid cryptoId)
        {
            CryptoId = cryptoId;
            _publicKeys = new List<UserPublicKey>();
            _masterPublicKeys = new List<IAsymmetricPublicKey>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionParameters"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public EncryptionParameters(Guid cryptoId, Passphrase passphrase)
            : this(cryptoId)
        {
            _identity = new LogOnIdentity(passphrase);
        }

        public EncryptionParameters(Guid cryptoId, LogOnIdentity identity)
            : this(cryptoId)
        {
            if (identity == null)
            {
                throw new ArgumentNullException("identity");
            }

            _identity = identity;
            _publicKeys.AddRange(identity.PublicKeys);
        }

        public Task AddAsync(IEnumerable<UserPublicKey> publicKeys)
        {
            AddOrReplace(publicKeys);

            return Constant.CompletedTask;
        }

        public void AddOrReplace(IEnumerable<UserPublicKey> publicKeys)
        {
            foreach (UserPublicKey userPublicKey in publicKeys)
            {
                UserPublicKey? existingKey = _publicKeys.FirstOrDefault(pk => pk.Email == userPublicKey.Email);
                if (existingKey != null)
                {
                    _ = _publicKeys.Remove(existingKey);
                }
                _publicKeys.Add(userPublicKey);
            }
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
        public Passphrase Passphrase { get { return _identity!.Passphrase; } }

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

        public IAsymmetricPublicKey? MasterPublicKey { get; set; }


        private List<IAsymmetricPublicKey> _masterPublicKeys;

        /// <summary>
        /// Gets or sets the business group master public key to use for the encryption.
        /// </summary>
        /// <value>
        /// The master key.
        /// </value>
        public IEnumerable<IAsymmetricPublicKey> MasterPublicKeys
        {
            get
            {
                return _masterPublicKeys;
            }
        }

        public Task AddMasterPublicKeyAsync(IEnumerable<IAsymmetricPublicKey> masterPublicKeys)
        {
            foreach (IAsymmetricPublicKey userPublicKey in masterPublicKeys)
            {
                IAsymmetricPublicKey? existingKey = _masterPublicKeys.FirstOrDefault(pk => pk.Thumbprint == userPublicKey.Thumbprint);
                if (existingKey != null)
                {
                    _masterPublicKeys.Remove(existingKey);
                }
                _masterPublicKeys.Add(userPublicKey);
            }

            return Constant.CompletedTask;
        }
    }
}
