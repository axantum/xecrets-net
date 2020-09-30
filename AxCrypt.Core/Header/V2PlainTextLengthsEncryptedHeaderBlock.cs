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
using System.Linq;

namespace AxCrypt.Core.Header
{
    public class V2PlaintextLengthsEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V2PlaintextLengthsEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.PlaintextLengths, dataBlock)
        {
        }

        public V2PlaintextLengthsEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[sizeof(long) + sizeof(long)])
        {
            HeaderCrypto = headerCrypto;
            PlaintextLength = 0;
            CompressedPlaintextLength = 0;
        }

        public override object Clone()
        {
            V2PlaintextLengthsEncryptedHeaderBlock clone = new V2PlaintextLengthsEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(clone);
        }

        /// <summary>
        /// The uncompressed size of the data in bytes
        /// </summary>
        public long PlaintextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long plaintextLength = rawBlock.GetLittleEndianValue(0, sizeof(long));
                return plaintextLength;
            }

            set
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                byte[] plaintextLength = value.GetLittleEndianBytes();
                Array.Copy(plaintextLength, 0, rawBlock, 0, plaintextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(rawBlock);
                SetDataBlockBytesReference(encryptedBlock);
            }
        }

        /// <summary>
        /// The Compressed size of the data in bytes
        /// </summary>
        public long CompressedPlaintextLength
        {
            get
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                long compressedPlaintextLength = rawBlock.GetLittleEndianValue(sizeof(long), sizeof(long));
                return compressedPlaintextLength;
            }

            set
            {
                byte[] rawBlock = HeaderCrypto.Decrypt(GetDataBlockBytesReference());
                byte[] compressedPlaintextLength = value.GetLittleEndianBytes();
                Array.Copy(compressedPlaintextLength, 0, rawBlock, sizeof(long), compressedPlaintextLength.Length);
                byte[] encryptedBlock = HeaderCrypto.Encrypt(rawBlock);
                SetDataBlockBytesReference(encryptedBlock);
            }
        }
    }
}