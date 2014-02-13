﻿#region Coypright and License

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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2DocumentHeaders
    {
        private static readonly byte[] _version = new byte[] { 4, 0, 0, 0, 0 };

        private DocumentHeadersCommon _headers;

        private ICrypto _keyEncryptingCrypto;

        public V2DocumentHeaders(ICrypto keyEncryptingCrypto, long iterations)
        {
            _keyEncryptingCrypto = keyEncryptingCrypto;
            _headers = new DocumentHeadersCommon(_version);

            _headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(keyEncryptingCrypto, iterations));
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);

            V1EncryptionInfoHeaderBlock encryptionInfoHeaderBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
            encryptionInfoHeaderBlock.IV = new AesIV();
            encryptionInfoHeaderBlock.PlaintextLength = 0;
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                EncryptedHeaderBlock encryptedHeaderBlock = headerBlock as EncryptedHeaderBlock;
                if (encryptedHeaderBlock == null)
                {
                    continue;
                }
                switch (encryptedHeaderBlock.HeaderBlockType)
                {
                    case HeaderBlockType.FileInfo:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(MasterKey, MasterIV, 256);
                        break;

                    case HeaderBlockType.Compression:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(MasterKey, MasterIV, 512);
                        break;

                    case HeaderBlockType.UnicodeFileNameInfo:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(MasterKey, MasterIV, 768);
                        break;
                }
            }
        }

        public AesKey MasterKey
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterKey(_keyEncryptingCrypto);
            }
        }

        public AesIV MasterIV
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterIV(_keyEncryptingCrypto);
            }
        }
    }
}