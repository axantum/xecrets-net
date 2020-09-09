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
using System.Text;

namespace AxCrypt.Core.Header
{
    public class V1UnicodeFileNameInfoEncryptedHeaderBlock : EncryptedHeaderBlock
    {
        public V1UnicodeFileNameInfoEncryptedHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.UnicodeFileNameInfo, dataBlock)
        {
        }

        public V1UnicodeFileNameInfoEncryptedHeaderBlock(ICrypto headerCrypto)
            : this(new byte[0])
        {
            HeaderCrypto = headerCrypto;
        }

        public override object Clone()
        {
            V1UnicodeFileNameInfoEncryptedHeaderBlock block = new V1UnicodeFileNameInfoEncryptedHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return CopyTo(block);
        }

        public string FileName
        {
            get
            {
                byte[] rawFileName = HeaderCrypto.Decrypt(GetDataBlockBytesReference());

                int end = rawFileName.Locate(new byte[] { 0, 0, }, 0, rawFileName.Length, 2);
                if (end == -1)
                {
                    throw new InvalidOperationException("Could not find terminating double null byte in file name");
                }

                string fileName = Encoding.Unicode.GetString(rawFileName, 0, end);

                return fileName;
            }

            set
            {
                byte[] rawFileName = Encoding.Unicode.GetBytes(value);
                byte[] dataBlock = new byte[rawFileName.Length + 2 + 15 - (rawFileName.Length + 2 + 15) % 16];
                rawFileName.CopyTo(dataBlock, 0);
                SetDataBlockBytesReference(HeaderCrypto.Encrypt(dataBlock));
            }
        }
    }
}