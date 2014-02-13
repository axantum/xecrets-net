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
    /// Implements V2 AES Cryptography, briefly AES-256 in CTR-Mode.
    /// </summary>
    public class V2AesCrypto : CryptoBase
    {
        private AesIV _iv;

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

        /// <summary>
        /// Instantiate a transformation
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="iv">Initial Vector, will be XOR:ed with the counter value</param>
        /// <param name="blockCounter">The block counter.</param>
        /// <param name="blockOffset">The block offset.</param>
        /// <exception cref="System.ArgumentNullException">key
        /// or
        /// iv</exception>
        public V2AesCrypto(AesKey key, AesIV iv, long blockCounter, int blockOffset)
            : this(key, iv)
        {
            _blockCounter = blockCounter;
            _blockOffset = blockOffset;
        }

        public V2AesCrypto(AesKey key, AesIV iv, long keyStreamOffset)
            : this(key, iv, keyStreamOffset / iv.Length, (int)(keyStreamOffset % iv.Length))
        {
        }

        public V2AesCrypto(AesKey key, AesIV iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!IsValidKeyLength(key.Length))
            {
                throw new ArgumentException("Key length is invalid.");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            if (iv.Length != BlockLength)
            {
                throw new ArgumentException("The IV length must be the same as the algorithm block length.");
            }

            Key = key;
            _iv = iv;
        }

        /// <summary>
        /// Gets the unique name of the algorithm implementation.
        /// </summary>
        /// <value>
        /// The name. This must be a short, language independent name usable both as an internal identifier, and as a display name.
        /// Typical values are "AES-128", "AES-256". The UI may use these as indexes for localized or clearer names, but if unknown
        /// the UI must be able to fallback and actually display this identifier as a selector for example in the UI. This is to
        /// support plug-in algorithm implementations in the future.
        /// </value>
        public override string Name
        {
            get { return "AES-256"; }
        }

        /// <summary>
        /// Create an instance of the underlying symmetric algorithm.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An instance of the algorithm.
        /// </value>
        public override SymmetricAlgorithm CreateAlgorithm()
        {
            SymmetricAlgorithm algorithm = CreateRawAlgorithm();
            algorithm.Key = Key.GetBytes();
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
            using (SymmetricAlgorithm algorithm = CreateAlgorithm())
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
            using (SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return new CounterModeCryptoTransform(algorithm, _blockCounter, _blockOffset);
            }
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>
        /// A new encrypting transformation instance
        /// </returns>
        public override ICryptoTransform CreateEncryptingTransform()
        {
            using (SymmetricAlgorithm algorithm = CreateAlgorithm())
            {
                return new CounterModeCryptoTransform(algorithm, _blockCounter, _blockOffset);
            }
        }
    }
}