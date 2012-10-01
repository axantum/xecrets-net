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
    /// Wrap an AES implementation with key and parameters. Instances of this class are immutable.
    /// </summary>
    public class AesCrypto : IDisposable
    {
        private Aes _aes = null;

        /// <summary>
        /// The default key size, in bits
        /// </summary>
        public static readonly int KeyBits = 128;

        /// <summary>
        /// The default key size in bytes
        /// </summary>
        public static readonly int KeyBytes = KeyBits / 8;

        /// <summary>
        /// Instantiate a transformation
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="iv">Initial Vector</param>
        /// <param name="cipherMode">Mode of operation, typically CBC</param>
        /// <param name="paddingMode">Padding mode, typically PCS7</param>
        public AesCrypto(AesKey key, AesIV iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            _aes = new AesManaged();
            _aes.Key = key.GetBytes();
            _aes.Mode = cipherMode;
            _aes.IV = iv.GetBytes();
            _aes.Padding = paddingMode;
        }

        /// <summary>
        /// Instantiate an AES transform with zero IV, CBC and no padding.
        /// </summary>
        /// <param name="key">The key</param>
        public AesCrypto(AesKey key)
            : this(key, AesIV.Zero, CipherMode.CBC, PaddingMode.None)
        {
        }

        /// <summary>
        /// Decrypt in one operation.
        /// </summary>
        /// <param name="cipherText">The complete cipher text</param>
        /// <returns>The decrypted result minus any padding</returns>
        public byte[] Decrypt(byte[] cipherText)
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            using (ICryptoTransform decryptor = _aes.CreateDecryptor())
            {
                byte[] plainText = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                return plainText;
            }
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="plaintext">The complete plaintext bytes</param>
        /// <returns>The cipher text, complete with any padding</returns>
        public byte[] Encrypt(byte[] plaintext)
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            {
                byte[] cipherText = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                return cipherText;
            }
        }

        /// <summary>
        /// Using this instances parameters, create a decryptor
        /// </summary>
        /// <returns>A new decrypting transformation instance</returns>
        public ICryptoTransform CreateDecryptingTransform()
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            return _aes.CreateDecryptor();
        }

        /// <summary>
        /// Using this instances parameters, create an encryptor
        /// </summary>
        /// <returns>A new encrypting transformation instance</returns>
        public ICryptoTransform CreateEncryptingTransform()
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            return _aes.CreateEncryptor();
        }

        /// <summary>
        /// Dispose this instances resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_aes == null)
            {
                return;
            }

            if (disposing)
            {
                _aes.Clear();
                _aes = null;
            }
        }
    }
}