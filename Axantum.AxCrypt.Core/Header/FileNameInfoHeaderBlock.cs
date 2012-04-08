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
using System.Globalization;
using System.Text;

namespace Axantum.AxCrypt.Core.Header
{
    public class FileNameInfoHeaderBlock : EncryptedHeaderBlock
    {
        public FileNameInfoHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.FileNameInfo, dataBlock)
        {
        }

        public override object Clone()
        {
            FileNameInfoHeaderBlock block = new FileNameInfoHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public string GetFileName(AesCrypto aesCrypto)
        {
            byte[] rawFileName = aesCrypto.Decrypt(GetDataBlockBytesReference());

            int end = Array.IndexOf<byte>(rawFileName, 0);
            if (end == -1)
            {
                throw new InvalidOperationException("Could not find terminating nul byte in file name");
            }

            string fileName = Encoding.ASCII.GetString(rawFileName, 0, end);

            return fileName;
        }
    }
}