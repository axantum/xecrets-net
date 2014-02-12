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
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2KeyWrapHeaderBlock : HeaderBlock
    {
        private const int WRAP_MAX_LENGTH = 128 + 16;
        private const int WRAP_SALT_MAX_LENGTH = 64;
        private const int WRAP_ITERATIONS_LENGTH = sizeof(uint);
        private const int PASSPHRASE_DERIVATION_SALT_MAX_LENGTH = 32;
        private const int PASSPHRASE_DERIVATION_ITERATIONS_LENGTH = sizeof(uint);

        private const int WRAP_OFFSET = 0;
        private const int WRAP_SALT_OFFSET = WRAP_OFFSET + WRAP_MAX_LENGTH;
        private const int WRAP_ITERATIONS_OFFSET = WRAP_SALT_OFFSET + WRAP_SALT_MAX_LENGTH;
        private const int PASSPHRASE_DERIVATION_SALT_OFFSET = WRAP_ITERATIONS_OFFSET + WRAP_ITERATIONS_LENGTH;
        private const int PASSPHRASE_DERIVATION_ITERATIONS_OFFSET = PASSPHRASE_DERIVATION_SALT_OFFSET + PASSPHRASE_DERIVATION_SALT_MAX_LENGTH;

        private const int DATABLOCK_LENGTH = PASSPHRASE_DERIVATION_ITERATIONS_OFFSET + PASSPHRASE_DERIVATION_ITERATIONS_LENGTH;

        public V2KeyWrapHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.V2KeyWrap, dataBlock)
        {
            if (dataBlock == null)
            {
                throw new ArgumentNullException("dataBlock");
            }
            if (dataBlock.Length != DATABLOCK_LENGTH)
            {
                throw new ArgumentException("Incorrect length for dataBlock");
            }
        }

        public V2KeyWrapHeaderBlock(AesKey keyEncryptingKey, long iterations)
            : this(Instance.RandomGenerator.Generate(DATABLOCK_LENGTH))
        {
            Initialize(keyEncryptingKey, iterations);
        }

        public override object Clone()
        {
            V2KeyWrapHeaderBlock block = new V2KeyWrapHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public long Iterations
        {
            get
            {
                long iterations = GetDataBlockBytesReference().GetLittleEndianValue(WRAP_ITERATIONS_OFFSET, sizeof(uint));

                return iterations;
            }
        }

        public byte[] GetKeyData(int blockSize, int keyLength)
        {
            byte[] keyData = new byte[keyLength + blockSize + blockSize / 2];
            Array.Copy(GetDataBlockBytesReference(), WRAP_OFFSET, keyData, 0, keyData.Length);

            return keyData;
        }

        private void Initialize(AesKey keyEncryptingKey, long iterations)
        {
            KeyWrapSalt salt = new KeyWrapSalt(keyEncryptingKey.Length);
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.Specification))
            {
                byte[] keyMaterial = Instance.RandomGenerator.Generate(keyEncryptingKey.Length + keyWrap.BlockSize);
                byte[] wrappedKeyData = keyWrap.Wrap(keyMaterial);
                Set(wrappedKeyData, salt, iterations);
            }
        }

        public byte[] UnwrapMasterKey(AesKey keyEncryptingKey)
        {
            byte[] saltBytes = new byte[keyEncryptingKey.Length];
            Array.Copy(GetDataBlockBytesReference(), WRAP_SALT_OFFSET, saltBytes, 0, saltBytes.Length);
            KeyWrapSalt salt = new KeyWrapSalt(saltBytes);

            byte[] unwrappedKeyData;
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, Iterations, KeyWrapMode.Specification))
            {
                byte[] wrappedKeyData = GetKeyData(keyWrap.BlockSize, keyEncryptingKey.Length);
                unwrappedKeyData = keyWrap.Unwrap(wrappedKeyData);
            }
            return unwrappedKeyData;
        }

        private void Set(byte[] wrapped, KeyWrapSalt salt, long iterations)
        {
            Array.Copy(wrapped, 0, GetDataBlockBytesReference(), 0, wrapped.Length);
            Array.Copy(salt.GetBytes(), 0, GetDataBlockBytesReference(), WRAP_SALT_OFFSET, salt.Length);
            byte[] iterationsBytes = iterations.GetLittleEndianBytes();
            Array.Copy(iterationsBytes, 0, GetDataBlockBytesReference(), WRAP_ITERATIONS_OFFSET, WRAP_ITERATIONS_LENGTH);
        }
    }
}