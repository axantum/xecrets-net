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
using Axantum.AxCrypt.Core.Crypto;

namespace Axantum.AxCrypt.Core.Reader
{
    public class CompressionHeaderBlock : EncryptedHeaderBlock
    {
        public CompressionHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.Compression, dataBlock)
        {
        }

        public CompressionHeaderBlock()
            : this(new byte[8])
        {
        }

        public override object Clone()
        {
            CompressionHeaderBlock block = new CompressionHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
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
                byte[] encryptedBlock = HeaderCrypto.Encrypt(isCompressedBytes);
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}