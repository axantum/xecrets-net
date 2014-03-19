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
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Crypto
{
    /// <summary>
    /// Wrap an AES implementation with key and parameters.
    /// </summary>
    public class V1AesCrypto : CryptoBase
    {
        internal const string InternalName = "AES-128-V1";

        private SymmetricIV _iv;

        static V1AesCrypto()
        {
            using (SymmetricAlgorithm algorithm = CreateRawAlgorithm())
            {
                SetBlockLength(algorithm.BlockSize / 8);
            }
        }

        /// <summary>
        /// Instantiate a transformation
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="iv">Initial Vector</param>
        public V1AesCrypto(IPassphrase key, SymmetricIV iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }

            Key = key;
            _iv = iv;
        }

        /// <summary>
        /// Instantiate an AES transform with zero IV.
        /// </summary>
        /// <param name="key">The key</param>
        public V1AesCrypto(IPassphrase key)
            : this(key, SymmetricIV.Zero128)
        {
        }

        public V1AesCrypto()
            : this(new GenericPassphrase(SymmetricKey.Zero128))
        {
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public override string Name
        {
            get { return InternalName; }
        }

        /// <summary>
        /// Create an instance of tranform suitable for NIST Key Wrap
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An instance of the algorithm.
        /// </value>
        public override IKeyWrapTransform CreateKeyWrapTransform(KeyWrapSalt salt, KeyWrapDirection keyWrapDirection)
        {
            return new BlockAlgorithmKeyWrapTransform(CreateAlgorithmInternal(), salt, keyWrapDirection);
        }

        private SymmetricAlgorithm CreateAlgorithmInternal()
        {
            SymmetricAlgorithm algorithm = CreateRawAlgorithm();
            algorithm.Key = Key.DerivedKey.GetBytes();
            algorithm.IV = _iv.GetBytes();

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
        /// <returns>The decrypted result minus any padding</returns>
        public override byte[] Decrypt(byte[] cipherText)
        {
            using (SymmetricAlgorithm aes = CreateAlgorithmInternal())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] plaintext = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                    return plaintext;
                }
            }
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>The cipher text, complete with any padding</returns>
        public override byte[] Encrypt(byte[] plaintext)
        {
            using (SymmetricAlgorithm aes = CreateAlgorithmInternal())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.None;
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cipherText = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                    return cipherText;
                }
            }
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>A new decrypting transformation instance</returns>
        public override ICryptoTransform CreateDecryptingTransform()
        {
            using (SymmetricAlgorithm aes = CreateAlgorithmInternal())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                return aes.CreateDecryptor();
            }
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>A new encrypting transformation instance</returns>
        public override ICryptoTransform CreateEncryptingTransform()
        {
            using (SymmetricAlgorithm aes = CreateAlgorithmInternal())
            {
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                return aes.CreateEncryptor();
            }
        }
    }
}