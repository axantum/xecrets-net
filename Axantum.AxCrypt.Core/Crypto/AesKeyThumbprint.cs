#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Represent a salted thumbprint for a AES key. Instances of this class are immutable.
    /// </summary>
    public class AesKeyThumbprint
    {
        private byte[] _salt;
        private AesKey _key;

        /// <summary>
        /// Instantiate a thumbprint
        /// </summary>
        /// <param name="key">The key to thumbprint.</param>
        /// <param name="salt">The salt to use.</param>
        public AesKeyThumbprint(AesKey key, byte[] salt)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            _key = key;
            _salt = salt;
        }

        /// <summary>
        /// Get the thumbprint.
        /// </summary>
        /// <returns>Calculate and return the thumbprint.</returns>
        public byte[] GetThumbprintBytes()
        {
            HashAlgorithm hash = HashAlgorithm.Create("SHA256");
            hash.TransformBlock(_salt, 0, _salt.Length, null, 0);
            hash.TransformFinalBlock(_key.GetBytes(), 0, _key.Length);

            return hash.Hash;
        }
    }
}