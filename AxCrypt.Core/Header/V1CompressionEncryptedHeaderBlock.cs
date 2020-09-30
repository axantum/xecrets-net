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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using System;

namespace AxCrypt.Core.Header
{
    public class V1CompressionEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V1CompressionEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Compression, dataBlock)
        {
        }

        public V1CompressionEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[16])
        {
            HeaderCrypto = headerCrypto;
            IsCompressed = false;
        }

        public override object Clone()
        {
            V1CompressionEncryptedHeaderBlock block = new V1CompressionEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public bool IsCompressed
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                Int32 isCompressed = (Int32)rawBlock.GetLittleEndianValue(0, sizeof(Int32));
                return isCompressed != 0;
            }

            set
            {
                int isCompressed = value ? 1 : 0;
                byte[] isCompressedBytes = isCompressed.GetLittleEndianBytes();
                Array.Copy(isCompressedBytes, 0, GetDataBlockBytesReference(), 0, isCompressedBytes.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(GetDataBlockBytesReference());
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}