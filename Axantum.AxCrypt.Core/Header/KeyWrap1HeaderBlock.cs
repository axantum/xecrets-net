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

namespace Axantum.AxCrypt.Core.Header
{
    public class KeyWrap1HeaderBlock : HeaderBlock
    {
        public KeyWrap1HeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.KeyWrap1, dataBlock)
        {
        }

        public byte[] GetKeyData()
        {
            byte[] keyData = new byte[16 + 8];
            Array.Copy(GetDataBlockBytesReference(), 0, keyData, 0, keyData.Length);

            return keyData;
        }

        public void Set(byte[] wrapped, byte[] salt, long iterations)
        {
            if (wrapped == null)
            {
                throw new ArgumentNullException("wrapped");
            }
            if (wrapped.Length != 16 + 8)
            {
                throw new ArgumentException("wrapped must be 128 bits + 8 bytes.");
            }
            if (salt == null)
            {
                throw new ArgumentNullException("salt");
            }
            if (salt.Length != 16)
            {
                throw new ArgumentException("salt must have same length as the wrapped key, i.e. 128 bits.");
            }
            Array.Copy(wrapped, 0, GetDataBlockBytesReference(), 0, wrapped.Length);
            Array.Copy(salt, 0, GetDataBlockBytesReference(), 16 + 8, salt.Length);
            byte[] iterationsBytes = iterations.GetLittleEndianBytes();
            Array.Copy(iterationsBytes, 0, GetDataBlockBytesReference(), 16 + 8 + 16, sizeof(uint));
        }

        public byte[] GetSalt()
        {
            byte[] salt = new byte[16];
            Array.Copy(GetDataBlockBytesReference(), 16 + 8, salt, 0, salt.Length);

            return salt;
        }

        public long Iterations()
        {
            long iterations = GetDataBlockBytesReference().GetLittleEndianValue(16 + 8 + 16, sizeof(uint));

            return iterations;
        }
    }
}