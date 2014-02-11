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

namespace Axantum.AxCrypt.Core.Header
{
    public class V2DocumentHeaders
    {
        private static readonly byte[] _version = new byte[] { 3, 2, 2, 0, 0 };

        private DocumentHeadersCommon _headers;

        private AesKey _keyEncryptingKey;

        public V2DocumentHeaders(AesKey keyEncryptingKey)
        {
            _keyEncryptingKey = keyEncryptingKey;
            _headers = new DocumentHeadersCommon(_version);

            _headers.HeaderBlocks.Add(new V1KeyWrap1HeaderBlock(keyEncryptingKey));
            _headers.HeaderBlocks.Add(new V1EncryptionInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new CompressionHeaderBlock());
            _headers.HeaderBlocks.Add(new FileInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new UnicodeFileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V1FileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);

            V1EncryptionInfoHeaderBlock encryptionInfoHeaderBlock = _headers.FindHeaderBlock<V1EncryptionInfoHeaderBlock>();
            encryptionInfoHeaderBlock.IV = new AesIV();
            encryptionInfoHeaderBlock.PlaintextLength = 0;
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            ICrypto headerCrypto = new V2AesCrypto(_keyEncryptingKey, new AesIV(), 0);

            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                EncryptedHeaderBlock encryptedHeaderBlock = headerBlock as EncryptedHeaderBlock;
                if (encryptedHeaderBlock != null)
                {
                    encryptedHeaderBlock.HeaderCrypto = headerCrypto;
                }
            }
        }

        private AesKey GetMasterKey()
        {
            V1KeyWrap1HeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V1KeyWrap1HeaderBlock>();
            VersionHeaderBlock versionHeaderBlock = _headers.FindHeaderBlock<VersionHeaderBlock>();
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(_keyEncryptingKey, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            return new AesKey(unwrappedKeyData);
        }
    }
}