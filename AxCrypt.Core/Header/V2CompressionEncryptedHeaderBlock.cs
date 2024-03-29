﻿#region Coypright and License

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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using System;

namespace AxCrypt.Core.Header
{
    public class V2CompressionEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V2CompressionEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Compression, dataBlock)
        {
        }

        public V2CompressionEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[1])
        {
            HeaderCrypto = headerCrypto;
            IsCompressed = false;
        }

        public override object Clone()
        {
            V2CompressionEncryptedHeaderBlock block = new V2CompressionEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public bool IsCompressed
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                byte isCompressed = (byte)rawBlock.GetLittleEndianValue(0, sizeof(byte));
                return isCompressed != 0;
            }

            set
            {
                byte[] isCompressedBytes = value ? new byte[] { 1 } : new byte[] { 0 };
                Array.Copy(isCompressedBytes, 0, GetDataBlockBytesReference(), 0, isCompressedBytes.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(GetDataBlockBytesReference());
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}