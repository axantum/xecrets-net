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

using System;
using System.Linq;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Implements V2 AES Cryptography
    /// </summary>
    public class V2AesCrypto : CryptoBase
    {
        private SymmetricIV _iv;

        private long _blockCounter;

        private int _blockOffset;

        static V2AesCrypto()
        {
            using (SymmetricAlgorithm algorithm = CreateRawAlgorithm())
            {
                SetValidKeyLengths(algorithm.LegalKeySizes);
                SetBlockLength(algorithm.BlockSize / 8);
            }
        }

        public V2AesCrypto(IPassphrase key, SymmetricIV iv, long keyStreamOffset)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            if (!IsValidKeyLength(key.DerivedKey.Size / 8))
            {
                throw new ArgumentException("Key length is invalid.");
            }
            if (iv.Length != BlockLength)
            {
                throw new ArgumentException("The IV length must be the same as the algorithm block length.");
            }

            Key = key;
            _iv = iv;
            _blockCounter = keyStreamOffset / iv.Length;
            _blockOffset = (int)(keyStreamOffset % iv.Length);
        }

        /// <summary>
        /// Create an instance of a transform suitable for NIST Key Wrap.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An instance of the transform.
        /// </value>
        public override IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection)
        {
            return new BlockAlgorithmKeyWrapTransform(CreateAlgorithmInternal(), salt, keyWrapDirection);
        }

        private SymmetricAlgorithm CreateAlgorithmInternal()
        {
            SymmetricAlgorithm algorithm = CreateRawAlgorithm();
            algorithm.Key = Key.DerivedKey.GetBytes();
            algorithm.IV = _iv.GetBytes();
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.None;

            return algorithm;
        }

        private static SymmetricAlgorithm CreateRawAlgorithm()
        {
            return new AesManaged();
        }

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText">The complete cipher text</param>
        /// <returns>
        /// The decrypted result minus any padding
        /// </returns>
        public override byte[] Decrypt(byte[] cipherText)
        {
            return Transform(cipherText);
        }

        /// <summary>
        /// Encrypt in one operation
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>
        /// The cipher text, complete with any padding
        /// </returns>
        public override byte[] Encrypt(byte[] plaintext)
        {
            return Transform(plaintext);
        }

        private byte[] Transform(byte[] plaintext)
        {
            using (SymmetricAlgorithm algorithm = CreateAlgorithmInternal())
            {
                using (ICryptoTransform transform = new CounterModeCryptoTransform(algorithm, _blockCounter, _blockOffset))
                {
                    return transform.TransformFinalBlock(plaintext, 0, plaintext.Length);
                }
            }
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>
        /// A new decrypting transformation instance
        /// </returns>
        public override ICryptoTransform CreateDecryptingTransform()
        {
            return new CounterModeCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public override ICryptoTransform CreateEncryptingTransform()
        {
            return new CounterModeCryptoTransform(CreateAlgorithmInternal(), _blockCounter, _blockOffset);
        }
    }
}