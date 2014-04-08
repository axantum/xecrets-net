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
using System.Linq;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;

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
            _crypto = keyEncryptingCrypto;

            Initialize(keyWrapIterations);
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
                long keyWrapIterations = GetDataBlockBytesReference().GetLittleEndianValue(WRAP_ITERATIONS_OFFSET, sizeof(uint));

                return keyWrapIterations;
            }
        }

        public int DerivationIterations
        {
            get
            {
                int derivationIterations = (int)GetDataBlockBytesReference().GetLittleEndianValue(PASSPHRASE_DERIVATION_ITERATIONS_OFFSET, PASSPHRASE_DERIVATION_ITERATIONS_LENGTH);

                return derivationIterations;
            }
            set
            {
                byte[] derivationIterationBytes = value.GetLittleEndianBytes();

                Array.Copy(derivationIterationBytes, 0, GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_ITERATIONS_OFFSET, PASSPHRASE_DERIVATION_ITERATIONS_LENGTH);
            }
        }

        private byte[] GetKeyData(int blockSize, int keyLength)
        {
            byte[] keyData = new byte[keyLength + blockSize + blockSize / 2];
            Array.Copy(GetDataBlockBytesReference(), WRAP_OFFSET, keyData, 0, keyData.Length);

            return keyData;
        }

        public Salt DerivationSalt
        {
            get
            {
                byte[] derivationSalt = new byte[PASSPHRASE_DERIVATION_SALT_MAX_LENGTH];
                Array.Copy(GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_SALT_OFFSET, derivationSalt, 0, derivationSalt.Length);

                return new Salt(derivationSalt);
            }
            set
            {
                Array.Copy(value.GetBytes(), 0, GetDataBlockBytesReference(), PASSPHRASE_DERIVATION_SALT_OFFSET, value.Length);
            }
        }

        private void Initialize(long keyWrapIterations)
        {
            DerivationSalt = _crypto.Key.DerivationSalt;
            DerivationIterations = _crypto.Key.DerivationIterations;

            Salt salt = new Salt(_crypto.Key.DerivedKey.Size);
            KeyWrap keyWrap = new KeyWrap(salt, keyWrapIterations, KeyWrapMode.Specification);
            _unwrappedKeyData = Instance.RandomGenerator.Generate(_crypto.Key.DerivedKey.Size / 8 + _crypto.BlockLength);
            byte[] wrappedKeyData = keyWrap.Wrap(_crypto, _unwrappedKeyData);
            Set(wrappedKeyData, salt, keyWrapIterations);
        }

        private void Set(byte[] wrapped, Salt salt, long keyWrapIterations)
        {
            Array.Copy(wrapped, 0, GetDataBlockBytesReference(), 0, wrapped.Length);
            Array.Copy(salt.GetBytes(), 0, GetDataBlockBytesReference(), WRAP_SALT_OFFSET, salt.Length);
            byte[] keyWrapIterationsBytes = keyWrapIterations.GetLittleEndianBytes();
            Array.Copy(keyWrapIterationsBytes, 0, GetDataBlockBytesReference(), WRAP_ITERATIONS_OFFSET, WRAP_ITERATIONS_LENGTH);
        }

        private byte[] _unwrappedKeyData;

        private byte[] UnwrappedMasterKeyData
        {
            get
            {
                if (_unwrappedKeyData == null)
                {
                    _unwrappedKeyData = UnwrapMasterKeyData();
                }
                return _unwrappedKeyData;
            }
        }

        private byte[] UnwrapMasterKeyData()
        {
            ICryptoFactory cryptoFactory = Instance.CryptoFactory.Create(_crypto.Key.CryptoId);
            IPassphrase key = cryptoFactory.CreatePassphrase(_crypto.Key.Passphrase, DerivationSalt, DerivationIterations);
            ICrypto keyEncryptingCrypto = cryptoFactory.CreateCrypto(key);
            byte[] saltBytes = new byte[keyEncryptingCrypto.Key.DerivedKey.Size / 8];
            Array.Copy(GetDataBlockBytesReference(), WRAP_SALT_OFFSET, saltBytes, 0, saltBytes.Length);
            Salt salt = new Salt(saltBytes);

            KeyWrap keyWrap = new KeyWrap(salt, KeyWrapIterations, KeyWrapMode.Specification);
            byte[] wrappedKeyData = GetKeyData(keyEncryptingCrypto.BlockLength, keyEncryptingCrypto.Key.DerivedKey.Size / 8);
            return keyWrap.Unwrap(keyEncryptingCrypto, wrappedKeyData);
        }

        private ICrypto _crypto;

        public ICrypto Crypto
        {
            set
            {
                _crypto = value;
                _unwrappedKeyData = null;
            }
        }

        public SymmetricKey MasterKey
        {
            get
            {
                if (UnwrappedMasterKeyData.Length == 0)
                {
                    return null;
                }
                byte[] masterKeyBytes = new byte[_crypto.Key.DerivedKey.Size / 8];
                Array.Copy(UnwrappedMasterKeyData, 0, masterKeyBytes, 0, masterKeyBytes.Length);
                return new SymmetricKey(masterKeyBytes);
            }
        }

        public SymmetricIV MasterIV
        {
            get
            {
                if (UnwrappedMasterKeyData.Length == 0)
                {
                    return null;
                }
                byte[] masterIVBytes = new byte[_crypto.BlockLength];
                Array.Copy(UnwrappedMasterKeyData, _crypto.Key.DerivedKey.Size / 8, masterIVBytes, 0, masterIVBytes.Length);
                return new SymmetricIV(masterIVBytes);
            }
        }
    }
}