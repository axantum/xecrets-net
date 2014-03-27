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

        public V2KeyWrapHeaderBlock(ICrypto keyEncryptingCrypto, long keyWrapIterations)
            : this(Instance.RandomGenerator.Generate(DATABLOCK_LENGTH))
        {
            Initialize(keyEncryptingCrypto, keyWrapIterations);
        }

        public override object Clone()
        {
            V2KeyWrapHeaderBlock block = new V2KeyWrapHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        public long KeyWrapIterations
        {
            get
            {
                long iterations = GetDataBlockBytesReference().GetLittleEndianValue(WRAP_ITERATIONS_OFFSET, sizeof(uint));

                return iterations;
            }
        }

        public int DerivationIterations
        {
            get
            {
                long iterations = GetDataBlockBytesReference().GetLittleEndianValue(PASSPHRASE_DERIVATION_ITERATIONS_OFFSET, PASSPHRASE_DERIVATION_ITERATIONS_LENGTH);

                return (int)iterations;
            }
            set
            {
                byte[] derivationIterationBytes = value.GetLittleEndianBytes();

                Array.Copy(derivationIterationBytes, 0, GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_ITERATIONS_OFFSET, PASSPHRASE_DERIVATION_ITERATIONS_LENGTH);
            }
        }

        public byte[] GetKeyData(int blockSize, int keyLength)
        {
            byte[] keyData = new byte[keyLength + blockSize + blockSize / 2];
            Array.Copy(GetDataBlockBytesReference(), WRAP_OFFSET, keyData, 0, keyData.Length);

            return keyData;
        }

        public byte[] GetDerivationSalt()
        {
            byte[] derivationSalt = new byte[PASSPHRASE_DERIVATION_SALT_MAX_LENGTH];
            Array.Copy(GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_SALT_OFFSET, derivationSalt, 0, derivationSalt.Length);

            return derivationSalt;
        }

        private void SetDeriviationSalt(byte[] salt)
        {
            Array.Copy(salt, 0, GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_SALT_OFFSET, salt.Length);
        }

        private void Initialize(ICrypto keyEncryptingCrypto, long keyWrapIterations)
        {
            SetDeriviationSalt(keyEncryptingCrypto.Key.GetDerivationSalt());
            DerivationIterations = (int)keyEncryptingCrypto.Key.DerivationIterations;

            KeyWrapSalt salt = new KeyWrapSalt(keyEncryptingCrypto.Key.DerivedKey.Length);
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.Specification);
            byte[] keyMaterial = Instance.RandomGenerator.Generate(keyEncryptingCrypto.Key.DerivedKey.Length + keyEncryptingCrypto.BlockLength);
            byte[] wrappedKeyData = keyWrap.Wrap(keyEncryptingCrypto, keyMaterial);
            Set(wrappedKeyData, salt, keyWrapIterations);
        }

        public byte[] UnwrapMasterKey(ICrypto keyEncryptingCrypto)
        {
            keyEncryptingCrypto = Instance.CryptoFactory.Default.CreateCrypto(new V2Passphrase(keyEncryptingCrypto.Key.Passphrase, GetDerivationSalt(), DerivationIterations, keyEncryptingCrypto.Key.DerivedKey.Length * 8), SymmetricIV.Zero128, 0);
            byte[] saltBytes = new byte[keyEncryptingCrypto.Key.DerivedKey.Length];
            Array.Copy(GetDataBlockBytesReference(), WRAP_SALT_OFFSET, saltBytes, 0, saltBytes.Length);
            KeyWrapSalt salt = new KeyWrapSalt(saltBytes);

            byte[] unwrappedKeyData;
            KeyWrap keyWrap = new KeyWrap(salt, KeyWrapIterations, KeyWrapMode.Specification);
            byte[] wrappedKeyData = GetKeyData(keyEncryptingCrypto.BlockLength, keyEncryptingCrypto.Key.DerivedKey.Length);
            unwrappedKeyData = keyWrap.Unwrap(keyEncryptingCrypto, wrappedKeyData);
            return unwrappedKeyData;
        }

        public SymmetricKey MasterKey(ICrypto keyEncryptingCrypto)
        {
            byte[] unwrappedKeyData = UnwrapMasterKey(keyEncryptingCrypto);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            byte[] masterKeyBytes = new byte[keyEncryptingCrypto.Key.DerivedKey.Length];
            Array.Copy(unwrappedKeyData, 0, masterKeyBytes, 0, masterKeyBytes.Length);
            return new SymmetricKey(masterKeyBytes);
        }

        public SymmetricIV MasterIV(ICrypto keyEncryptingCrypto)
        {
            byte[] unwrappedKeyData = UnwrapMasterKey(keyEncryptingCrypto);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            byte[] masterIVBytes = new byte[keyEncryptingCrypto.BlockLength];
            Array.Copy(unwrappedKeyData, keyEncryptingCrypto.Key.DerivedKey.Length, masterIVBytes, 0, masterIVBytes.Length);
            return new SymmetricIV(masterIVBytes);
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