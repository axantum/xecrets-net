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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2KeyWrapHeaderBlock : HeaderBlock
    {
        public V2KeyWrapHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.V2KeyWrap, dataBlock)
        {
        }

        public V2KeyWrapHeaderBlock(AesKey keyEncryptingKey)
            : this(new byte[248])
        {
            Initialize(keyEncryptingKey);
        }

        public override object Clone()
        {
            V1KeyWrap1HeaderBlock block = new V1KeyWrap1HeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public byte[] GetKeyData()
        {
            byte[] keyData = new byte[16 + 8];
            Array.Copy(GetDataBlockBytesReference(), 0, keyData, 0, keyData.Length);

            return keyData;
        }

        private void Initialize(AesKey keyEncryptingKey)
        {
            byte[] keyMaterial = OS.Current.GetRandomBytes(128);
            long iterations = Instance.UserSettings.KeyWrapIterations;
            KeyWrapSalt salt = new KeyWrapSalt(keyEncryptingKey.Length);
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.Specification))
            {
                byte[] wrappedKeyData = keyWrap.Wrap(keyMaterial);
                Set(wrappedKeyData, salt, iterations);
            }
        }

        private void Set(byte[] wrapped, KeyWrapSalt salt, long iterations)
        {
            Array.Copy(wrapped, 0, GetDataBlockBytesReference(), 0, wrapped.Length);
            Array.Copy(salt.GetBytes(), 0, GetDataBlockBytesReference(), wrapped.Length, salt.Length);
            byte[] iterationsBytes = iterations.GetLittleEndianBytes();
            Array.Copy(iterationsBytes, 0, GetDataBlockBytesReference(), wrapped.Length + salt.Length, sizeof(uint));
        }
    }
}