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

namespace Axantum.AxCrypt.Core.Header
{
    public class AesCrypto : IDisposable
    {
        private static readonly byte[] _zeroIv = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        private AesManaged _aes = null;

        public static readonly int KeyBits = 128;
        public static readonly int KeyBytes = KeyBits / 8;

        public AesCrypto(byte[] key, byte[] iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.Length != KeyBytes)
            {
                throw new ArgumentOutOfRangeException("key");
            }
            if (iv == null)
            {
                throw new ArgumentNullException("iv");
            }
            _aes = new AesManaged();
            if (iv.Length != _aes.BlockSize / 8)
            {
                throw new ArgumentOutOfRangeException("iv");
            }
            _aes.Key = key;
            _aes.Mode = cipherMode;
            _aes.IV = iv;
            _aes.Padding = paddingMode;
        }

        public AesCrypto(byte[] key)
            : this(key, _zeroIv, CipherMode.CBC, PaddingMode.None)
        {
        }

        public byte[] Decrypt(byte[] cipherText)
        {
            using (ICryptoTransform decryptor = _aes.CreateDecryptor())
            {
                byte[] plainText = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
                return plainText;
            }
        }

        public byte[] Encrypt(byte[] plaintext)
        {
            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            {
                byte[] cipherText = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                return cipherText;
            }
        }

        public ICryptoTransform CreateDecryptingTransform()
        {
            return _aes.CreateDecryptor();
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