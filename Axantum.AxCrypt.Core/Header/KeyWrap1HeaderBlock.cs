﻿#region Coypright and License

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

        public void Set(byte[] keyData, byte[] salt, long iterations)
        {
        }

        public byte[] GetSalt()
        {
            byte[] salt = new byte[16];
            Array.Copy(GetDataBlockBytesReference(), 16 + 8, salt, 0, salt.Length);

            return salt;
        }

        public long Iterations()
        {
            uint iterations = BitConverter.ToUInt32(GetDataBlockBytesReference(), 16 + 8 + 16);

            return iterations;
        }
    }
}