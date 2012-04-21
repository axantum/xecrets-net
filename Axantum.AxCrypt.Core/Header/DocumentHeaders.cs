using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core.Reader
{
    public class DocumentHeaders
    {
        private IList<HeaderBlock> _headerBlocks;

        private AesKey _keyEncryptingKey;

        public DocumentHeaders(AesKey keyEncryptingKey)
        {
            _keyEncryptingKey = keyEncryptingKey;

            _headerBlocks = new List<HeaderBlock>();
            _headerBlocks.Add(new PreambleHeaderBlock());
            _headerBlocks.Add(new VersionHeaderBlock());
            _headerBlocks.Add(new KeyWrap1HeaderBlock(keyEncryptingKey));
            _headerBlocks.Add(new EncryptionInfoHeaderBlock());
            _headerBlocks.Add(new CompressionHeaderBlock());
            _headerBlocks.Add(new FileInfoHeaderBlock());
            _headerBlocks.Add(new UnicodeFileNameInfoHeaderBlock());
            _headerBlocks.Add(new FileNameInfoHeaderBlock());
            _headerBlocks.Add(new DataHeaderBlock());

            SetMasterKeyForEncryptedHeaderBlocks(_headerBlocks);
        }

        public DocumentHeaders(DocumentHeaders documentHeaders)
        {
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            foreach (HeaderBlock headerBlock in documentHeaders._headerBlocks)
            {
                headerBlocks.Add((HeaderBlock)headerBlock.Clone());
            }
            _headerBlocks = headerBlocks;

            _keyEncryptingKey = documentHeaders._keyEncryptingKey;
        }

        public bool Load(AxCryptReader axCryptReader)
        {
            axCryptReader.Read();
            if (axCryptReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            while (axCryptReader.Read())
            {
                switch (axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.HeaderBlock:
                        headerBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        headerBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        _headerBlocks = headerBlocks;
                        EnsureFileFormatVersion();
                        if (GetMasterKey() != null)
                        {
                            SetMasterKeyForEncryptedHeaderBlocks(headerBlocks);
                            return true;
                        }
                        return false;
                    default:
                        throw new InternalErrorException("The reader returned an AxCryptItemType it should not be possible for it to return.");
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
        }

        private void SetMasterKeyForEncryptedHeaderBlocks(IList<HeaderBlock> headerBlocks)
        {
            AesCrypto headerCrypto = new AesCrypto(HeadersSubkey.Key);

            foreach (HeaderBlock headerBlock in headerBlocks)
            {
                EncryptedHeaderBlock encryptedHeaderBlock = headerBlock as EncryptedHeaderBlock;
                if (encryptedHeaderBlock != null)
                {
                    encryptedHeaderBlock.HeaderCrypto = headerCrypto;
                }
            }
        }

        public void WriteWithoutHmac(Stream cipherStream)
        {
            if (cipherStream == null)
            {
                throw new ArgumentNullException("cipherStream");
            }

            WriteInternal(cipherStream, cipherStream);
        }

        public void WriteWithHmac(HmacStream hmacStream)
        {
            if (hmacStream == null)
            {
                throw new ArgumentNullException("hmacStream");
            }

            WriteInternal(hmacStream.ChainedStream, hmacStream);
        }

        private void WriteInternal(Stream cipherStream, Stream hmacStream)
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            versionHeaderBlock.SetCurrentVersion();

            cipherStream.Position = 0;
            AxCrypt1Guid.Write(cipherStream);
            PreambleHeaderBlock preambleHaderBlock = FindHeaderBlock<PreambleHeaderBlock>();
            preambleHaderBlock.Write(cipherStream);
            foreach (HeaderBlock headerBlock in _headerBlocks)
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
            DataHeaderBlock dataHeaderBlock = FindHeaderBlock<DataHeaderBlock>();
            dataHeaderBlock.Write(hmacStream);
        }

        private AesKey GetMasterKey()
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(_keyEncryptingKey, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return null;
            }
            return new AesKey(unwrappedKeyData);
        }

        public void RewrapMasterKey(AesKey keyEncryptingKey)
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            keyHeaderBlock.RewrapMasterKey(GetMasterKey(), keyEncryptingKey);
            _keyEncryptingKey = keyEncryptingKey;
        }

        public Subkey HmacSubkey
        {
            get
            {
                AesKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Hmac);
            }
        }

        public Subkey DataSubkey
        {
            get
            {
                AesKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Data);
            }
        }

        public Subkey HeadersSubkey
        {
            get
            {
                AesKey masterKey = GetMasterKey();
                if (masterKey == null)
                {
                    return null;
                }
                return new Subkey(masterKey, HeaderSubkey.Headers);
            }
        }

        public DataHmac Hmac
        {
            get
            {
                PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();

                return headerBlock.Hmac;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();
                headerBlock.Hmac = value;
            }
        }

        public long UncompressedLength
        {
            get
            {
                CompressionInfoHeaderBlock compressionInfo = FindHeaderBlock<CompressionInfoHeaderBlock>();
                if (compressionInfo == null)
                {
                    return -1;
                }
                return compressionInfo.UncompressedLength;
            }

            set
            {
                CompressionInfoHeaderBlock compressionInfo = FindHeaderBlock<CompressionInfoHeaderBlock>();
                if (compressionInfo == null)
                {
                    compressionInfo = new CompressionInfoHeaderBlock();
                    compressionInfo.HeaderCrypto = new AesCrypto(HeadersSubkey.Key);
                    _headerBlocks.Add(compressionInfo);
                }
                compressionInfo.UncompressedLength = value;
            }
        }

        private string AnsiFileName
        {
            get
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();
                return headerBlock.FileName;
            }

            set
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();
                headerBlock.FileName = value;
            }
        }

        private string UnicodeFileName
        {
            get
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
                if (headerBlock == null)
                {
                    // Unicode file name was added in 1.6.3.3 - if we can't find it signal it's absence with null.
                    return null;
                }

                return headerBlock.FileName;
            }

            set
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
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
                CompressionHeaderBlock headerBlock = FindHeaderBlock<CompressionHeaderBlock>();
                if (headerBlock == null)
                {
                    // Conditional compression was added in 1.2.2, before then it was always compressed.
                    return true;
                }
                return headerBlock.IsCompressed;
            }
            set
            {
                CompressionHeaderBlock headerBlock = FindHeaderBlock<CompressionHeaderBlock>();
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
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.CreationTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.CreationTimeUtc = value;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastAccessTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastAccessTimeUtc = value;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                return headerBlock.LastWriteTimeUtc;
            }
            set
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();
                headerBlock.LastWriteTimeUtc = value;
            }
        }

        /// <summary>
        /// The Initial Vector used for CBC encryption of the data
        /// </summary>
        /// <returns>The Initial Vector</returns>
        public AesIV IV
        {
            get
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
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
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
                return headerBlock.PlaintextLength;
            }

            set
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();
                headerBlock.PlaintextLength = value;
            }
        }

        public long CipherTextLength
        {
            get
            {
                DataHeaderBlock headerBlock = FindHeaderBlock<DataHeaderBlock>();
                return headerBlock.CipherTextLength;
            }
            set
            {
                DataHeaderBlock headerBlock = FindHeaderBlock<DataHeaderBlock>();
                headerBlock.CipherTextLength = value;
            }
        }

        private void EnsureFileFormatVersion()
        {
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            if (versionHeaderBlock.FileVersionMajor > 3)
            {
                throw new FileFormatException("Too new file format.", ErrorStatus.TooNewFileFormatVersion);
            }
        }

        private T FindHeaderBlock<T>() where T : HeaderBlock
        {
            foreach (HeaderBlock headerBlock in _headerBlocks)
            {
                T typedHeaderHeaderBlock = headerBlock as T;
                if (typedHeaderHeaderBlock != null)
                {
                    return typedHeaderHeaderBlock;
                }
            }
            return null;
        }
    }
}