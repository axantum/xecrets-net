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
            _headers.HeaderBlocks.Add(new FileInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V2CompressionHeaderBlock());
            _headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetDataEncryptingCryptoForEncryptedHeaderBlocks(_headers.HeaderBlocks);
        }

        private void SetDataEncryptingCryptoForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
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
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, 256);
                        break;

                    case HeaderBlockType.Compression:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, 512);
                        break;

                    case HeaderBlockType.UnicodeFileNameInfo:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, 768);
                        break;
                }
            }
        }

        public AesKey DataEncryptingKey
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterKey(_keyEncryptingCrypto);
            }
        }

        public AesIV DataEncryptingIV
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterIV(_keyEncryptingCrypto);
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        public bool IsCompressed
        {
            get
            {
                V2CompressionHeaderBlock headerBlock = _headers.FindHeaderBlock<V2CompressionHeaderBlock>();
                return headerBlock.IsCompressed;
            }
            set
            {
                V2CompressionHeaderBlock headerBlock = _headers.FindHeaderBlock<V2CompressionHeaderBlock>();
                headerBlock.IsCompressed = value;
            }
        }

        public string FileName
        {
            get
            {
                V2UnicodeFileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V2UnicodeFileNameInfoHeaderBlock>();
                return headerBlock.FileName;
            }

            set
            {
                V2UnicodeFileNameInfoHeaderBlock headerBlock = _headers.FindHeaderBlock<V2UnicodeFileNameInfoHeaderBlock>();
                headerBlock.FileName = value;
            }
        }
    }
}