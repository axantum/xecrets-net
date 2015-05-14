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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Header
{
    public class V2DocumentHeaders : IDisposable
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

        private IKeyStreamCryptoFactory _keyStreamFactory;

        public V2DocumentHeaders(EncryptionParameters encryptionParameters, long keyWrapIterations)
        {
            _headers = new Headers();

            _headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            _headers.HeaderBlocks.Add(new VersionHeaderBlock(_version));

            ICryptoFactory cryptoFactory = Resolve.CryptoFactory.Create(encryptionParameters.CryptoId);
            IDerivedKey keyEncryptingKey = cryptoFactory.CreateDerivedKey(encryptionParameters.Passphrase);
            V2KeyWrapHeaderBlock keyWrap = new V2KeyWrapHeaderBlock(cryptoFactory, keyEncryptingKey, keyWrapIterations);
            _headers.HeaderBlocks.Add(keyWrap);
            _keyStreamFactory = keyWrap;

            foreach (IAsymmetricPublicKey publicKey in encryptionParameters.PublicKeys)
            {
                _headers.HeaderBlocks.Add(new V2AsymmetricKeyWrapHeaderBlock(publicKey, keyWrap.MasterKey, keyWrap.MasterIV));
            }
            _headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(GetHeaderCrypto(HeaderBlockType.FileInfo)));
            _headers.HeaderBlocks.Add(new V2CompressionEncryptedHeaderBlock(GetHeaderCrypto(HeaderBlockType.Compression)));
            _headers.HeaderBlocks.Add(new V2UnicodeFileNameInfoEncryptedHeaderBlock(GetHeaderCrypto(HeaderBlockType.UnicodeFileNameInfo)));
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetDataEncryptingCryptoForEncryptedHeaderBlocks(_headers.HeaderBlocks);
        }

        public V2DocumentHeaders(IKeyStreamCryptoFactory keyStreamCryptoFactory)
        {
            _keyStreamFactory = keyStreamCryptoFactory;
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

        public bool Load(Headers headers)
        {
            headers.EnsureFileFormatVersion(4, 4);

            if (!IsMasterKeyKnown(headers))
            {
                return false;
            }

            _hmacStream = new V2HmacStream(GetHmacKey());
            AxCrypt1Guid.Write(_hmacStream);
            foreach (HeaderBlock header in headers.HeaderBlocks)
            {
                header.Write(_hmacStream);
            }

            SetDataEncryptingCryptoForEncryptedHeaderBlocks(headers.HeaderBlocks);

            _headers = headers;
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
                encryptedHeaderBlock.HeaderCrypto = GetHeaderCrypto(encryptedHeaderBlock.HeaderBlockType);
            }
        }

        private ICrypto GetHeaderCrypto(HeaderBlockType headerBlockType)
        {
            switch (headerBlockType)
            {
                case HeaderBlockType.FileInfo:
                    return CreateKeyStreamCrypto(FILEINFO_KEYSTREAM_INDEX);

                case HeaderBlockType.Compression:
                    return CreateKeyStreamCrypto(COMPRESSIONINFO_KEYSTREAM_INDEX);

                case HeaderBlockType.UnicodeFileNameInfo:
                    return CreateKeyStreamCrypto(FILENAMEINFO_KEYSTREAM_INDEX);
            }
            throw new InternalErrorException("Unexpected header block type. Can't determine Header Cryptop.");
        }

        private ICrypto CreateKeyStreamCrypto(long keyStreamOffset)
        {
            return _keyStreamFactory.Crypto(keyStreamOffset);
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

        public void WriteEndWithHmac(V2HmacStream hmacStream, long plaintextLength, long compressedPlaintextLength)
        {
            WriteGeneralHeaders(hmacStream);

            V2PlaintextLengthsEncryptedHeaderBlock lengths = new V2PlaintextLengthsEncryptedHeaderBlock(CreateKeyStreamCrypto(LENGTHSINFO_KEYSTREAM_INDEX));
            lengths.PlaintextLength = plaintextLength;
            lengths.CompressedPlaintextLength = compressedPlaintextLength;
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
                    case HeaderBlockType.PlaintextLengths:
                        continue;
                }
                headerBlock.Write(hmacStream);
            }
        }

        private bool IsMasterKeyKnown(Headers headers)
        {
            V2KeyWrapHeaderBlock keyHeaderBlock = headers.FindHeaderBlock<V2KeyWrapHeaderBlock>();
            if (keyHeaderBlock.MasterKey != null)
            {
                return true;
            }

            if (_keyStreamFactory != null && _keyStreamFactory.Crypto(0) != null)
            {
                return true;
            }

            return false;
        }

        public ICrypto CreateDataCrypto()
        {
            return CreateKeyStreamCrypto(DATA_KEYSTREAM_INDEX);
        }

        public byte[] GetHmacKey()
        {
            ICrypto hmacKeyCrypto = CreateKeyStreamCrypto(HMACKEY_KEYSTREAM_INDEX);
            byte[] key = new byte[V2Hmac.RequiredLength];
            key = hmacKeyCrypto.Encrypt(key);

            return key;
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>();
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        public bool IsCompressed
        {
            get
            {
                V2CompressionEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V2CompressionEncryptedHeaderBlock>();
                return headerBlock.IsCompressed;
            }
            set
            {
                V2CompressionEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V2CompressionEncryptedHeaderBlock>();
                headerBlock.IsCompressed = value;
            }
        }

        public string FileName
        {
            get
            {
                V2UnicodeFileNameInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V2UnicodeFileNameInfoEncryptedHeaderBlock>();
                return headerBlock.FileName;
            }

            set
            {
                V2UnicodeFileNameInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V2UnicodeFileNameInfoEncryptedHeaderBlock>();
                headerBlock.FileName = value;
            }
        }

        public Hmac Hmac
        {
            get
            {
                V2HmacHeaderBlock hmacHeaderBlock = _headers.FindTrailerBlock<V2HmacHeaderBlock>();
                return hmacHeaderBlock.Hmac;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_hmacStream == null)
            {
                return;
            }
            _hmacStream.Dispose();
            _hmacStream = null;
        }
    }
}