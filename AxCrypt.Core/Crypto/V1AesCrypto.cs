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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Algorithm;
using System;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// Wrap an AES implementation with key and parameters.
    /// </summary>
    public class V1AesCrypto : CryptoBase
    {
        internal const string InternalName = "AES-128-V1";

        private readonly SymmetricIV _iv;

        /// <summary>
        /// Instantiate a transformation
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="iv">Initial Vector, or null for a zero vector.</param>
        public V1AesCrypto(ICryptoFactory factory, SymmetricKey key, SymmetricIV? iv)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (key.Size != 128)
            {
                throw new ArgumentException("Key length is invalid.");
            }
            using (SymmetricAlgorithm algorithm = CreateRawAlgorithm())
            {
                iv ??= new SymmetricIV(new byte[algorithm.BlockSize / 8]);
                if (iv.Length != algorithm.BlockSize / 8)
                {
                    throw new ArgumentException("The IV length must be the same as the algorithm block length.");
                }
            }

            Key = key;
            _iv = iv;
        }

        public override int BlockLength
        {
            get { return _iv.Length; }
        }

        /// <summary>
        /// Create an instance of tranform suitable for NIST Key Wrap
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// An instance of the algorithm.
        /// </value>
        public override IKeyWrapTransform CreateKeyWrapTransform(Salt salt, KeyWrapDirection keyWrapDirection)
        {
            return new BlockAlgorithmKeyWrapTransform(CreateAlgorithmInternal(), salt, keyWrapDirection);
        }

        private SymmetricAlgorithm CreateAlgorithmInternal()
        {
            SymmetricAlgorithm algorithm = CreateRawAlgorithm();
            algorithm.SetKey(Key.GetBytes());
            algorithm.SetIV(_iv.GetBytes());

            return algorithm;
        }

        private static SymmetricAlgorithm CreateRawAlgorithm()
        {
            return New<Aes>();
        }

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText">The complete cipher text</param>
        /// <returns>The decrypted result minus any padding</returns>
        public override byte[] Decrypt(byte[] cipherText)
        {
            if (cipherText == null)
            {
                throw new ArgumentNullException(nameof(cipherText));
            }

            using SymmetricAlgorithm aes = CreateAlgorithmInternal();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            using ICryptoTransform decryptor = aes.CreateDecryptingTransform();
            byte[] plaintext = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            return plaintext;
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>The cipher text, complete with any padding</returns>
        public override byte[] Encrypt(byte[] plaintext)
        {
            if (plaintext == null)
            {
                throw new ArgumentNullException(nameof(plaintext));
            }

            using SymmetricAlgorithm aes = CreateAlgorithmInternal();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            using ICryptoTransform encryptor = aes.CreateEncryptingTransform();
            byte[] cipherText = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
            return cipherText;
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>A new decrypting transformation instance</returns>
        public override ICryptoTransform DecryptingTransform()
        {
            using SymmetricAlgorithm aes = CreateAlgorithmInternal();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes.CreateDecryptingTransform();
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>A new encrypting transformation instance</returns>
        public override ICryptoTransform EncryptingTransform()
        {
            using SymmetricAlgorithm aes = CreateAlgorithmInternal();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            return aes.CreateEncryptingTransform();
        }
    }
}
