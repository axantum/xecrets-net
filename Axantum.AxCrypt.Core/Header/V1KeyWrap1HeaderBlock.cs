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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;

namespace Axantum.AxCrypt.Core.Header
{
    public class V1KeyWrap1HeaderBlock : HeaderBlock
    {
        public V1KeyWrap1HeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.KeyWrap1, dataBlock)
        {
        }

        public V1KeyWrap1HeaderBlock(ICrypto keyEncryptingCrypto, long keyWrapIterations)
            : this(new byte[44])
        {
            Initialize(keyEncryptingCrypto, keyWrapIterations);
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

        protected void Set(byte[] wrapped, Salt salt, long keyWrapIterations)
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
            Array.Copy(salt.GetBytes(), 0, GetDataBlockBytesReference(), 16 + 8, salt.Length);
            byte[] iterationsBytes = keyWrapIterations.GetLittleEndianBytes();
            Array.Copy(iterationsBytes, 0, GetDataBlockBytesReference(), 16 + 8 + 16, sizeof(uint));
        }

        public Salt Salt
        {
            get
            {
                byte[] salt = new byte[16];
                Array.Copy(GetDataBlockBytesReference(), 16 + 8, salt, 0, salt.Length);

                return new Salt(salt);
            }
        }

        public long Iterations
        {
            get
            {
                long iterations = GetDataBlockBytesReference().GetLittleEndianValue(16 + 8 + 16, sizeof(uint));

                return iterations;
            }
        }

        public byte[] UnwrapMasterKey(ICrypto keyEncryptingCrypto, byte fileVersionMajor)
        {
            byte[] wrappedKeyData = GetKeyData();
            Salt salt = Salt;
            IPassphrase keyEncryptingKey = keyEncryptingCrypto.Key;
            if (fileVersionMajor <= 1)
            {
                // Due to a bug in 1.1 and earlier we only used a truncated part of the key and salt :-(
                // Compensate for this here. Users should be warned if FileVersionMajor <= 1 .
                byte[] badKey = new byte[keyEncryptingKey.DerivedKey.Length];
                Array.Copy(keyEncryptingCrypto.Key.DerivedKey.GetBytes(), 0, badKey, 0, 4);
                keyEncryptingKey = new GenericPassphrase(new SymmetricKey(badKey));

                byte[] badSalt = new byte[salt.Length];
                Array.Copy(salt.GetBytes(), 0, badSalt, 0, 4);
                salt = new Salt(badSalt);
            }

            byte[] unwrappedKeyData;
            KeyWrap keyWrap = new KeyWrap(salt, Iterations, KeyWrapMode.AxCrypt);
            unwrappedKeyData = keyWrap.Unwrap(Instance.CryptoFactory.Legacy.CreateCrypto(keyEncryptingKey), wrappedKeyData);
            return unwrappedKeyData;
        }

        private void Initialize(ICrypto keyEncryptingCrypto, long keyWrapIterations)
        {
            SymmetricKey masterKey = new SymmetricKey(keyEncryptingCrypto.Key.DerivedKey.Length * 8);
            Salt salt = new Salt(masterKey.Length * 8);
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.AxCrypt);
            byte[] wrappedKeyData = keyWrap.Wrap(keyEncryptingCrypto, masterKey);
            Set(wrappedKeyData, salt, keyWrapIterations);
        }

        public void RewrapMasterKey(SymmetricKey masterKey, SymmetricKey keyEncryptingKey)
        {
            Salt salt = new Salt(keyEncryptingKey.Length * 8);
            KeyWrap keyWrap = new KeyWrap(salt, Iterations, KeyWrapMode.AxCrypt);
            byte[] wrappedKeyData = keyWrap.Wrap(Instance.CryptoFactory.Legacy.CreateCrypto(new GenericPassphrase(keyEncryptingKey)), masterKey);
            Set(wrappedKeyData, salt, Iterations);
        }
    }
}