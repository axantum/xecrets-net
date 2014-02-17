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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2DocumentHeaders
    {
        private const int HMACKEY_KEYSTREAM_INDEX = 0;
        private const int FILEINFO_KEYSTREAM_INDEX = 256;
        private const int COMPRESSIONINFO_KEYSTREAM_INDEX = 512;
        private const int FILENAMEINFO_KEYSTREAM_INDEX = 768;
        private const int LENGTHSINFO_KEYSTREAM_INDEX = 2048;
        private const int DATA_KEYSTREAM_INDEX = 1048576;

        private static readonly byte[] _version = new byte[] { 4, 0, 2, 0, 0 };

        private Headers _headers;
        private V2HmacStream _hmacStream;

        private ICrypto _keyEncryptingCrypto;

        public V2DocumentHeaders(ICrypto keyEncryptingCrypto, long iterations)
        {
            _keyEncryptingCrypto = keyEncryptingCrypto;
            _headers = new Headers();

            _headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            _headers.HeaderBlocks.Add(new VersionHeaderBlock(_version));
            _headers.HeaderBlocks.Add(new V2KeyWrapHeaderBlock(keyEncryptingCrypto, iterations));
            _headers.HeaderBlocks.Add(new FileInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new V2CompressionHeaderBlock());
            _headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoHeaderBlock());
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetDataEncryptingCryptoForEncryptedHeaderBlocks(_headers.HeaderBlocks);
        }

        public V2DocumentHeaders(ICrypto keyEncryptingCrypto)
        {
            _keyEncryptingCrypto = keyEncryptingCrypto;
            _headers = new Headers();
        }

        public Headers Headers
        {
            get { return _headers; }
        }

        public V2HmacStream HmacStream
        {
            get { return _hmacStream; }
        }

        public bool Load(AxCryptReader axCryptReader)
        {
            _headers.Load(axCryptReader);

            _headers.EnsureFileFormatVersion(4, 4);
            if (DataEncryptingKey == null)
            {
                return false;
            }

            _hmacStream = new V2HmacStream(GetHmacKey());
            AxCrypt1Guid.Write(_hmacStream);
            foreach (HeaderBlock header in _headers.HeaderBlocks)
            {
                header.Write(_hmacStream);
            }

            SetDataEncryptingCryptoForEncryptedHeaderBlocks(_headers.HeaderBlocks);
            return true;
        }

        public void Trailers(AxCryptReader axCryptReader)
        {
            _headers.Trailers(axCryptReader);
            foreach (HeaderBlock header in _headers.TrailerBlocks)
            {
                if (header.HeaderBlockType == HeaderBlockType.V2Hmac)
                {
                    continue;
                }
                header.Write(_hmacStream);
            }
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
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, FILEINFO_KEYSTREAM_INDEX);
                        break;

                    case HeaderBlockType.Compression:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, COMPRESSIONINFO_KEYSTREAM_INDEX);
                        break;

                    case HeaderBlockType.UnicodeFileNameInfo:
                        encryptedHeaderBlock.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, FILENAMEINFO_KEYSTREAM_INDEX);
                        break;
                }
            }
        }

        public void SetCurrentVersion()
        {
            _headers.SetCurrentVersion(_version);
        }

        public void WriteStartWithHmac(V2HmacStream hmacStream)
        {
            if (hmacStream == null)
            {
                throw new ArgumentNullException("hmacStream");
            }

            AxCrypt1Guid.Write(hmacStream);

            PreambleHeaderBlock preambleHeaderBlock = _headers.FindHeaderBlock<PreambleHeaderBlock>();
            preambleHeaderBlock.Write(hmacStream);

            WriteGeneralHeaders(hmacStream);

            DataHeaderBlock dataHeaderBlock = _headers.FindHeaderBlock<DataHeaderBlock>();
            dataHeaderBlock.Write(hmacStream);
        }

        public void WriteEndWithHmac(V2HmacStream hmacStream, long plainTextLength, long compressedPlainTextLength)
        {
            WriteGeneralHeaders(hmacStream);

            V2PlainTextLengthsHeaderBlock lengths = new V2PlainTextLengthsHeaderBlock();
            lengths.HeaderCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, LENGTHSINFO_KEYSTREAM_INDEX);
            lengths.PlainTextLength = plainTextLength;
            lengths.CompressedPlainTextLength = compressedPlainTextLength;
            lengths.Write(hmacStream);

            V2HmacHeaderBlock hmac = new V2HmacHeaderBlock();
            hmac.Hmac = hmacStream.Hmac;
            hmac.Write(hmacStream);
        }

        private void WriteGeneralHeaders(V2HmacStream hmacStream)
        {
            foreach (HeaderBlock headerBlock in _headers.HeaderBlocks)
            {
                switch (headerBlock.HeaderBlockType)
                {
                    case HeaderBlockType.Data:
                    case HeaderBlockType.Preamble:
                    case HeaderBlockType.PlainTextLengths:
                        continue;
                }
                headerBlock.Write(hmacStream);
            }
        }

        public SymmetricKey DataEncryptingKey
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterKey(_keyEncryptingCrypto);
            }
        }

        public SymmetricIV DataEncryptingIV
        {
            get
            {
                V2KeyWrapHeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
                return keyHeaderBlock.MasterIV(_keyEncryptingCrypto);
            }
        }

        public ICrypto GetDataCrypto()
        {
            return new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, DATA_KEYSTREAM_INDEX);
        }

        public byte[] GetHmacKey()
        {
            ICrypto hmacKeyCrypto = new V2AesCrypto(DataEncryptingKey, DataEncryptingIV, HMACKEY_KEYSTREAM_INDEX);
            byte[] key = new byte[V2Hmac.RequiredLength];
            key = hmacKeyCrypto.Encrypt(key);

            return key;
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

        public Hmac Hmac
        {
            get
            {
                V2HmacHeaderBlock hmacHeaderBlock = _headers.FindHeaderBlock<V2HmacHeaderBlock>(_headers.TrailerBlocks);
                return hmacHeaderBlock.Hmac;
            }
        }
    }
}