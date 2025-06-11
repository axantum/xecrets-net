﻿#region Coypright and License

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
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Header
{
    public class V2AsymmetricKeyWrapHeaderBlock : HeaderBlock, IKeyStreamCryptoFactory
    {
        private const int DATABLOCK_LENGTH = 4096 / 8;

        private ICryptoFactory? _cryptoFactory;

        public V2AsymmetricKeyWrapHeaderBlock(byte[] dataBlock)
            : base(HeaderBlockType.V2AsymmetricKeyWrap, dataBlock)
        {
        }

        public V2AsymmetricKeyWrapHeaderBlock(UserPublicKey publicKey, SymmetricKey masterKey, SymmetricIV masterIV)
            : this(publicKey.PublicKey, masterKey, masterIV)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException("publicKey");
            }
        }

        public V2AsymmetricKeyWrapHeaderBlock(IAsymmetricPublicKey publicKey, SymmetricKey masterKey, SymmetricIV masterIV)
            : this(Resolve.RandomGenerator.Generate(DATABLOCK_LENGTH))
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            byte[] encrypted = publicKey.Transform(masterKey + masterIV);
            _ = GetDataBlockBytesReference().SetFrom(encrypted);
        }

        public override object Clone()
        {
            V2AsymmetricKeyWrapHeaderBlock block = new V2AsymmetricKeyWrapHeaderBlock((byte[])GetDataBlockBytesReference().Clone());
            return block;
        }

        private IAsymmetricPrivateKey? _privateKey;

        /// <summary>
        /// Sets the private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        public void SetPrivateKey(ICryptoFactory cryptoFactory, IAsymmetricPrivateKey privateKey)
        {
            _cryptoFactory = cryptoFactory;
            _privateKey = privateKey;
            _decryptedDataBlock = null;
        }

        private byte[]? _decryptedDataBlock = null;

        private byte[] DecryptedDataBlock
        {
            get
            {
                if (_decryptedDataBlock == null)
                {
                    _decryptedDataBlock = _privateKey!.Transform(GetDataBlockBytesReference()) ?? Array.Empty<byte>();
                }
                return _decryptedDataBlock;
            }
        }

        /// <summary>
        /// Create an ICrypto instance from the decrypted asymmetric key wrap if possible.
        /// </summary>
        /// <param name="keyStreamOffset"></param>
        /// <returns>An ICrypto instance, initialized with key and iv, or null if no valid key is set.</returns>
        public ICrypto? Crypto(long keyStreamOffset)
        {
            byte[] iv = new byte[_cryptoFactory!.BlockSize / 8];
            byte[] masterKey = new byte[_cryptoFactory.KeySize / 8];

            if (DecryptedDataBlock.Length == 0)
            {
                return null;
            }
            if (DecryptedDataBlock.Length != masterKey.Length + iv.Length)
            {
                return null;
            }

            Array.Copy(DecryptedDataBlock, 0, masterKey, 0, masterKey.Length);
            Array.Copy(DecryptedDataBlock, masterKey.Length, iv, 0, iv.Length);

            ICrypto crypto = _cryptoFactory.CreateCrypto(new SymmetricKey(masterKey), new SymmetricIV(iv), keyStreamOffset);
            return crypto;
        }
    }
}
