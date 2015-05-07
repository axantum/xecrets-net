#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public EncryptionParameters()
        {
            PublicKeys = new IAsymmetricPublicKey[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionParameters"/> class.
        /// </summary>
        /// <param name="passphrase">The passphrase.</param>
        public EncryptionParameters(Guid cryptoId, Passphrase passphrase)
            : this()
        {
            CryptoId = cryptoId;
            Passphrase = passphrase;
        }

        /// <summary>
        /// An empty set of encryption parameters.
        /// </summary>
        public static readonly EncryptionParameters Empty = new EncryptionParameters { Passphrase = Passphrase.Empty, CryptoId = Guid.Empty, PublicKeys = new IAsymmetricPublicKey[0] };

        /// <summary>
        /// Gets or sets the passphrase. A passphrase is always required.
        /// </summary>
        /// <value>
        /// The passphrase.
        /// </value>
        public Passphrase Passphrase { get; set; }

        /// <summary>
        /// Gets or sets the public keys to also use to encrypt the session key with.
        /// </summary>
        /// <value>
        /// The public keys. The enumeration may be empty.
        /// </value>
        public IEnumerable<IAsymmetricPublicKey> PublicKeys { get; set; }

        /// <summary>
        /// Gets or sets the crypto identifier to use for the encryption.
        /// </summary>
        /// <value>
        /// The crypto identifier.
        /// </value>
        public Guid CryptoId { get; set; }
    }
}