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
 * The source is maintained at http://AxCrypt.codeplex.com/ please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.IO;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core.Header
{
    public abstract class EncryptedHeaderBlock : HeaderBlock
    {
        public EncryptedHeaderBlock(HeaderBlockType headerBlockType, byte[] dataBlock)
            : base(headerBlockType, dataBlock)
        {
        }

        private static readonly byte[] _iv = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };

        private const int KEY_BITS = 128;

        private byte[] _keyBytes = null;

        public void SetKey(byte[] keyBytes)
        {
            _keyBytes = (byte[])keyBytes.Clone();
        }

        private AesManaged _aes = null;

        private AesManaged Aes
        {
            get
            {
                if (_aes == null)
                {
                    _aes = new AesManaged();
                    _aes.Mode = CipherMode.CBC;
                    _aes.IV = _iv;
                    _aes.KeySize = KEY_BITS;
                    _aes.Padding = PaddingMode.None;
                }
                return _aes;
            }
        }

        private byte[] _dataBlockBytes = null;

        protected override byte[] GetDataBlockBytesReference()
        {
            if (_keyBytes == null)
            {
                throw new InvalidOperationException("Can't get encrypted block without a key being set.");
            }
            if (_dataBlockBytes != null)
            {
                return _dataBlockBytes;
            }
            byte[] encryptedDataBlockBytesReference = base.GetDataBlockBytesReference();
            using (ICryptoTransform decryptor = _aes.CreateDecryptor())
            {
                _dataBlockBytes = decryptor.TransformFinalBlock(encryptedDataBlockBytesReference, 0, encryptedDataBlockBytesReference.Length);
            }
            return _dataBlockBytes;
        }

        public override void Write(Stream stream)
        {
            if (_keyBytes == null)
            {
                throw new InvalidOperationException("Can't write encrypted block without a key being set.");
            }
            WritePrefix(stream);
            byte[] encryptedDataBlock;
            byte[] dataBlockBytesReference = GetDataBlockBytesReference();
            using (ICryptoTransform encryptor = _aes.CreateEncryptor())
            {
                encryptedDataBlock = encryptor.TransformFinalBlock(dataBlockBytesReference, 0, dataBlockBytesReference.Length);
            }
            stream.Write(encryptedDataBlock, 0, encryptedDataBlock.Length);
        }
    }
}