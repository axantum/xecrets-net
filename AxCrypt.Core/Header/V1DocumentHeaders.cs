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
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using System;
using System.Collections.Generic;
using System.IO;

namespace AxCrypt.Core.Header
{
    public class V1DocumentHeaders
    {
        private static readonly byte[] _version = new byte[] { 3, 2, 2, 0, 0 };

        private Headers _headers = new Headers();

        private IDerivedKey _keyEncryptingKey;

        public V1DocumentHeaders(Passphrase passphrase, long keyWrapIterations)
            : this(passphrase)
        {
            _headers.HeaderBlocks.Add(new PreambleHeaderBlock());
            _headers.HeaderBlocks.Add(new VersionHeaderBlock(_version));
            _headers.HeaderBlocks.Add(new V1KeyWrap1HeaderBlock(_keyEncryptingKey.DerivedKey, keyWrapIterations));

            ICrypto headerCrypto = Resolve.CryptoFactory.Legacy.CreateCrypto(HeadersSubkey?.Key ?? throw new Runtime.FileFormatException("Missing Master Key."), null, 0);
            _headers.HeaderBlocks.Add(new V1EncryptionInfoEncryptedHeaderBlock(headerCrypto));
            _headers.HeaderBlocks.Add(new V1CompressionEncryptedHeaderBlock(headerCrypto));
            _headers.HeaderBlocks.Add(new FileInfoEncryptedHeaderBlock(headerCrypto));
            _headers.HeaderBlocks.Add(new V1UnicodeFileNameInfoEncryptedHeaderBlock(headerCrypto));
            _headers.HeaderBlocks.Add(new V1FileNameInfoEncryptedHeaderBlock(headerCrypto));
            _headers.HeaderBlocks.Add(new DataHeaderBlock());

            SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);

            V1EncryptionInfoEncryptedHeaderBlock? encryptionInfoHeaderBlock = _headers.FindHeaderBlock<V1EncryptionInfoEncryptedHeaderBlock>() ?? throw new Runtime.FileFormatException("Missing V1EncryptionInfoEncryptedHeaderBlock.");
            encryptionInfoHeaderBlock.IV = new SymmetricIV(128);
            encryptionInfoHeaderBlock.PlaintextLength = 0;

            FileName = string.Empty;
        }

        public V1DocumentHeaders(Passphrase passphrase)
        {
            _keyEncryptingKey = Resolve.CryptoFactory.Create(new V1Aes128CryptoFactory().CryptoId).CreateDerivedKey(passphrase);
        }

        public V1DocumentHeaders(V1DocumentHeaders documentHeaders)
        {
            ArgumentNullException.ThrowIfNull(documentHeaders);

            _keyEncryptingKey = documentHeaders._keyEncryptingKey;
            foreach (HeaderBlock headerBlock in documentHeaders._headers.HeaderBlocks)
            {
                _headers.HeaderBlocks.Add((HeaderBlock)headerBlock.Clone());
            }
        }

        public bool Load(AxCryptReaderBase reader)
        {
            Headers headers = new Headers();
            headers.Load(reader);

            return Load(headers);
        }

        public bool Load(Headers headers)
        {
            ArgumentNullException.ThrowIfNull(headers);

            _headers = headers;
            _headers.EnsureFileFormatVersion(1, 3);

            if (GetMasterKey() != null)
            {
                SetMasterKeyForEncryptedHeaderBlocks(_headers.HeaderBlocks);
                return true;
            }
            return false;
        }

        public Headers Headers
        {
            get { return _headers; }
        }

        public VersionHeaderBlock VersionHeaderBlock
        {
            get
            {
                return _headers.VersionHeaderBlock ?? throw new Runtime.FileFormatException("Missing VersionHeaderBlock.");
            }
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            ICrypto headerCrypto = Resolve.CryptoFactory.Legacy.CreateCrypto(HeadersSubkey?.Key ?? throw new Runtime.FileFormatException("Missing Master Key."), null, 0);

            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                if (headerBlock is EncryptedHeaderBlock encryptedHeaderBlock)
                {
                    encryptedHeaderBlock.HeaderCrypto = headerCrypto;
                }
            }
        }

        public void WriteWithoutHmac(Stream cipherStream)
        {
            if (cipherStream == null)
            {
                throw new ArgumentNullException(nameof(cipherStream));
            }

            WriteInternal(cipherStream, cipherStream);
        }

        public void WriteWithHmac(V1HmacStream hmacStream)
        {
            if (hmacStream == null)
            {
                throw new ArgumentNullException(nameof(hmacStream));
            }

            WriteInternal(hmacStream.ChainedStream, hmacStream);
        }

        private void WriteInternal(Stream cipherStream, Stream hmacStream)
        {
            cipherStream.Position = 0;
            AxCrypt1Guid.Write(cipherStream);
            PreambleHeaderBlock preambleHeaderBlock = _headers.FindHeaderBlock<PreambleHeaderBlock>() ?? throw new Runtime.FileFormatException("Missing PreambleHeaderBlock.");
            preambleHeaderBlock.Write(cipherStream);
            foreach (HeaderBlock headerBlock in _headers.HeaderBlocks)
            {
                if (headerBlock is DataHeaderBlock)
                {
                    continue;
                }
                if (headerBlock is PreambleHeaderBlock)
                {
                    continue;
                }
                headerBlock.Write(hmacStream);
            }
            DataHeaderBlock dataHeaderBlock = _headers.FindHeaderBlock<DataHeaderBlock>() ?? throw new Runtime.FileFormatException("Missing DataHeaderBlock.");
            dataHeaderBlock.Write(hmacStream);
        }

        private SymmetricKey? GetMasterKey()
        {
            V1KeyWrap1HeaderBlock? keyHeaderBlock = _headers.FindHeaderBlock<V1KeyWrap1HeaderBlock>() ?? throw new Runtime.FileFormatException("Missing V1KeyWrap1HeaderBlock.");
            VersionHeaderBlock? versionHeaderBlock = _headers.FindHeaderBlock<VersionHeaderBlock>() ?? throw new Runtime.FileFormatException("Missing VersionHeaderBlock.");
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(_keyEncryptingKey.DerivedKey, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            return new SymmetricKey(unwrappedKeyData);
        }

        public void RewrapMasterKey(IDerivedKey keyEncryptingKey, long keyWrapIterations)
        {
            if (keyEncryptingKey == null)
            {
                throw new ArgumentNullException(nameof(keyEncryptingKey));
            }

            V1KeyWrap1HeaderBlock keyHeaderBlock = _headers.FindHeaderBlock<V1KeyWrap1HeaderBlock>() ?? throw new Runtime.FileFormatException("Missing V1KeyWrap1HeaderBlock.");
            keyHeaderBlock.RewrapMasterKey(GetMasterKey() ?? throw new Runtime.FileFormatException("Missing Master Key."), keyEncryptingKey.DerivedKey, keyWrapIterations);
            _keyEncryptingKey = keyEncryptingKey;
        }

        public Subkey? HmacSubkey
        {
            get
            {
                SymmetricKey? masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Hmac);
            }
        }

        public Subkey? DataSubkey
        {
            get
            {
                SymmetricKey? masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Data);
            }
        }

        public Subkey? HeadersSubkey
        {
            get
            {
                SymmetricKey? masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Headers);
            }
        }

        public long UncompressedLength
        {
            get
            {
                V1CompressionInfoEncryptedHeaderBlock? compressionInfo = _headers.FindHeaderBlock<V1CompressionInfoEncryptedHeaderBlock>();
                if (compressionInfo == null)
                {
                    return -1;
                }
                return compressionInfo.UncompressedLength;
            }

            set
            {
                V1CompressionInfoEncryptedHeaderBlock? compressionInfo = _headers.FindHeaderBlock<V1CompressionInfoEncryptedHeaderBlock>();
                if (compressionInfo == null)
                {
                    ICrypto headerCrypto = Resolve.CryptoFactory.Legacy.CreateCrypto(HeadersSubkey?.Key ?? throw new Runtime.FileFormatException("Missing Master Key."), null, 0);
                    compressionInfo = new V1CompressionInfoEncryptedHeaderBlock(headerCrypto);
                    _headers.HeaderBlocks.Add(compressionInfo);
                }
                compressionInfo.UncompressedLength = value;
            }
        }

        private string AnsiFileName
        {
            get
            {
                V1FileNameInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1FileNameInfoEncryptedHeaderBlock>();
                return headerBlock?.FileName ?? throw new Runtime.FileFormatException("Missing V1FileNameInfoEncryptedHeaderBlock.");
            }

            set
            {
                V1FileNameInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1FileNameInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1FileNameInfoEncryptedHeaderBlock.");
                headerBlock.FileName = value;
            }
        }

        private string? UnicodeFileName
        {
            get
            {
                V1UnicodeFileNameInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1UnicodeFileNameInfoEncryptedHeaderBlock>();
                if (headerBlock == null)
                {
                    // Unicode file name was added in 1.6.3.3 - if we can't find it signal it's absence with null.
                    return null;
                }

                return headerBlock.FileName;
            }

            set
            {
                ArgumentNullException.ThrowIfNull(value);
                V1UnicodeFileNameInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1UnicodeFileNameInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1UnicodeFileNameInfoEncryptedHeaderBlock.");
                headerBlock.FileName = value;
            }
        }

        public string FileName
        {
            get
            {
                return UnicodeFileName ?? AnsiFileName;
            }

            set
            {
                UnicodeFileName = value;
                AnsiFileName = value;
            }
        }

        public bool IsCompressed
        {
            get
            {
                V1CompressionEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1CompressionEncryptedHeaderBlock>();
                if (headerBlock == null)
                {
                    // Conditional compression was added in 1.2.2, before then it was always compressed.
                    return true;
                }
                return headerBlock.IsCompressed;
            }
            set
            {
                V1CompressionEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1CompressionEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1CompressionEncryptedHeaderBlock.");
                headerBlock.IsCompressed = value;
                if (value)
                {
                    // When compressed, ensure we reserve room in headers for the CompressionInfo block
                    UncompressedLength = 0;
                }
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<FileInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing FileInfoEncryptedHeaderBlock.");
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// The Initial Vector used for CBC encryption of the data
        /// </summary>
        /// <returns>The Initial Vector</returns>
        public SymmetricIV IV
        {
            get
            {
                V1EncryptionInfoEncryptedHeaderBlock? headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1EncryptionInfoEncryptedHeaderBlock.");
                return headerBlock.IV;
            }
        }

        /// <summary>
        /// The length in bytes of the plain text. This may still require decompression (inflate).
        /// </summary>
        public long PlaintextLength
        {
            get
            {
                V1EncryptionInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1EncryptionInfoEncryptedHeaderBlock.");
                return headerBlock.PlaintextLength;
            }

            set
            {
                V1EncryptionInfoEncryptedHeaderBlock headerBlock = _headers.FindHeaderBlock<V1EncryptionInfoEncryptedHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing V1EncryptionInfoEncryptedHeaderBlock.");
                headerBlock.PlaintextLength = value;
            }
        }

        public long CipherTextLength
        {
            get
            {
                DataHeaderBlock? headerBlock = _headers.FindHeaderBlock<DataHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing DataHeaderBlock.");
                return headerBlock.CipherTextLength;
            }
            set
            {
                DataHeaderBlock headerBlock = _headers.FindHeaderBlock<DataHeaderBlock>()
                    ?? throw new Runtime.FileFormatException("Missing DataHeaderBlock.");
                headerBlock.CipherTextLength = value;
            }
        }
    }
}
