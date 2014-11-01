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

using Axantum.AxCrypt.Core.Algorithm;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Implements V2 AES Cryptography
    /// </summary>
    public class V2AesCrypto : V2CryptoBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="V2AesCrypto"/> class.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="key">The key.</param>
        /// <param name="iv">The iv, or null for none.</param>
        /// <param name="keyStreamOffset">The key stream offset.</param>
        public V2AesCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            using (SymmetricAlgorithm algorithm = CreateAlgorithmInternal())
            {
                Initialize(key, iv, keyStreamOffset, algorithm);
            }
        }

        protected override SymmetricAlgorithm CreateAlgorithm()
        {
            return CreateAlgorithmInternal();
        }

        private static SymmetricAlgorithm CreateAlgorithmInternal()
        {
            return TypeMap.Resolve.New<Aes>();
        }
    }
}