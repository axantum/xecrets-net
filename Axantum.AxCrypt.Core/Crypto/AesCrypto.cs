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
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class AesCrypto : IDisposable
    {
        private AesManaged _aes = null;

        public static readonly int KeyBits = 128;
        public static readonly int KeyBytes = KeyBits / 8;

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

        public AesCrypto(AesKey key)
            : this(key, AesIV.Zero, CipherMode.CBC, PaddingMode.None)
        {
        }

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

        public ICryptoTransform CreateDecryptingTransform()
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            return _aes.CreateDecryptor();
        }

        public ICryptoTransform CreateEncryptingTransform()
        {
            if (_aes == null)
            {
                throw new ObjectDisposedException("_aes");
            }
            return _aes.CreateEncryptor();
        }

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