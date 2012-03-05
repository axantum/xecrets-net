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

namespace Axantum.AxCrypt.Core.Header
{
    public class EncryptionInfoHeaderBlock : EncryptedHeaderBlock
    {
        public EncryptionInfoHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.EncryptionInfo, dataBlock)
        {
        }

        public long PlaintextLength(AesCrypto aesCrypto)
        {
            byte[] rawData = aesCrypto.Decrypt(GetDataBlockBytesReference());

            long plaintextLength = BitConverter.ToInt64(rawData, 0);
            return plaintextLength;
        }

        public byte[] GetIV(AesCrypto aesCrypto)
        {
            byte[] rawData = aesCrypto.Decrypt(GetDataBlockBytesReference());

            byte[] iv = new byte[16];
            Array.Copy(rawData, 8, iv, 0, iv.Length);
            return iv;
        }
    }
}