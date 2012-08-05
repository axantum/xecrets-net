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
using System.Collections.Generic;
using System.Security.Cryptography;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Hold a key for AES. Instances of this class are immutable.
    /// </summary>
    public class AesKey : IEquatable<AesKey>
    {
        private static ICollection<int> _validAesKeySizes = ValidAesKeySizes();

        private byte[] _aesKey;

        /// <summary>
        /// Instantiate a random key.
        /// </summary>
        public AesKey()
        {
            _aesKey = AxCryptEnvironment.Current.GetRandomBytes(16);
        }

        /// <summary>
        /// Instantiate a key.
        /// </summary>
        /// <param name="key">The key to use. The length can be any that is valid for the algorithm.</param>

        public AesKey(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!IsValidKeyLength(key.Length))
            {
                throw new InternalErrorException("Invalid AES key size");
            }
            _aesKey = (byte[])key.Clone();
        }

        /// <summary>
        /// Get the actual key bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return (byte[])_aesKey.Clone();
        }

        /// <summary>
        /// Check if a key length is valid for AES
        /// </summary>
        /// <param name="length">The length in bytes</param>
        /// <returns>true if the length in bytes is a valid key length for AES</returns>
        public static bool IsValidKeyLength(int length)
        {
            return _validAesKeySizes.Contains(length);
        }

        public int Length
        {
            get
            {
                return _aesKey.Length;
            }
        }

        private static ICollection<int> ValidAesKeySizes()
        {
            List<int> validAesKeySizes = new List<int>();
            using (AesManaged aes = new AesManaged())
            {
                foreach (KeySizes keySizes in aes.LegalKeySizes)
                {
                    for (int validKeySizeInBits = keySizes.MinSize; validKeySizeInBits <= keySizes.MaxSize; validKeySizeInBits += keySizes.SkipSize)
                    {
                        validAesKeySizes.Add(validKeySizeInBits / 8);
                    }
                }
            }
            return validAesKeySizes;
        }

        #region IEquatable<AesKey> Members

        /// <summary>
        /// Check if one instance is equivalent to another.
        /// </summary>
        /// <param name="other">The instance to compare to</param>
        /// <returns>true if the keys are equivalent</returns>
        public bool Equals(AesKey other)
        {
            if (other == null)
            {
                return false;
            }
            return _aesKey.IsEquivalentTo(other._aesKey);
        }

        #endregion IEquatable<AesKey> Members
    }
}