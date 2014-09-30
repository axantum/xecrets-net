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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using System;

namespace Axantum.AxCrypt.Core.Header
{
    public class V1CompressionInfoEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V1CompressionInfoEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.CompressionInfo, dataBlock)
        {
        }

        public V1CompressionInfoEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(Resolve.RandomGenerator.Generate(16))
        {
            HeaderCrypto = headerCrypto;
            UncompressedLength = 0;
        }

        public override object Clone()
        {
            V1CompressionInfoEncryptedHeaderBlock block = new V1CompressionInfoEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        /// <summary>
        /// The uncompressed size of the data
        /// </summary>
        public long UncompressedLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long normalSize = rawBlock.GetLittleEndianValue(0, sizeof(long));
                return normalSize;
            }

            set
            {
                byte[] normalSizeBytes = value.GetLittleEndianBytes();
                Array.Copy(normalSizeBytes, 0, GetDataBlockBytesReference(), 0, normalSizeBytes.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(GetDataBlockBytesReference());
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}