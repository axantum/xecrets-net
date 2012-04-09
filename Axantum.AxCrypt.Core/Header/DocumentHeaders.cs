using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core.Header
{
    public class DocumentHeaders : IDisposable
    {
        private IList<HeaderBlock> HeaderBlocks { get; set; }

        private byte[] _masterKey;

        private RandomNumberGenerator _rng;

        public DocumentHeaders(byte[] keyEncryptingKey)
        {
            HeaderBlocks = new List<HeaderBlock>();
            HeaderBlocks.Add(new PreambleHeaderBlock());
        }

        public DocumentHeaders(DocumentHeaders documentHeaders)
        {
            List<HeaderBlock> headerBlocks = new List<HeaderBlock>();
            foreach (HeaderBlock headerBlock in documentHeaders.HeaderBlocks)
            {
                headerBlocks.Add((HeaderBlock)headerBlock.Clone());
            }
            HeaderBlocks = headerBlocks;
            _masterKey = (byte[])documentHeaders._masterKey.Clone();
        }

        private byte[] GetRandomBytes(int n)
        {
            if (_rng == null)
            {
                _rng = RandomNumberGenerator.Create();
            }

            byte[] data = new byte[n];
            _rng.GetBytes(data);
            return data;
        }

        public bool Load(AxCryptReader axCryptReader, byte[] keyEncryptingKey)
        {
            axCryptReader.Read();
            if (axCryptReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }
            HeaderBlocks = new List<HeaderBlock>();
            while (axCryptReader.Read())
            {
                switch (axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.HeaderBlock:
                        HeaderBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        HeaderBlocks.Add(axCryptReader.CurrentHeaderBlock);
                        EnsureFileFormatVersion();
                        UnwrapMasterKey(keyEncryptingKey);
                        return GetMasterKey() != null;
                    default:
                        throw new InternalErrorException("The reader returned an AxCryptItemType it should not be possible for it to return.");
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
        }

        public void Write(Stream cipherStream, Stream hmacStream)
        {
            cipherStream.Position = 0;
            AxCrypt1Guid.Write(cipherStream);
            bool preambleSeen = false;
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                if (preambleSeen && hmacStream != null)
                {
                    headerBlock.Write(hmacStream);
                }
                else
                {
                    headerBlock.Write(cipherStream);
                }
                if (headerBlock is PreambleHeaderBlock)
                {
                    preambleSeen = true;
                }
            }
        }

        private void UnwrapMasterKey(byte[] keyEncryptingKey)
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
            VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
            byte[] unwrappedKeyData = keyHeaderBlock.UnwrapMasterKey(keyEncryptingKey, versionHeaderBlock.FileVersionMajor);
            if (unwrappedKeyData.Length == 0)
            {
                return;
            }
            _masterKey = unwrappedKeyData;
        }

        public byte[] GetMasterKey()
        {
            if (_masterKey == null)
            {
                return null;
            }
            return (byte[])_masterKey.Clone();
        }

        public void RewrapMasterKey(byte[] keyEncryptingKey)
        {
            KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();

            long iterations = keyHeaderBlock.Iterations();
            byte[] masterKey = GetMasterKey();
            byte[] salt = GetRandomBytes(16);
            using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.AxCrypt))
            {
                byte[] wrappedKeyData = keyWrap.Wrap(masterKey);
                keyHeaderBlock.Set(wrappedKeyData, salt, iterations);
            }
        }

        Subkey _hmacSubkey;

        public Subkey HmacSubkey
        {
            get
            {
                if (_hmacSubkey == null)
                {
                    if (_masterKey == null)
                    {
                        return null;
                    }
                    _hmacSubkey = new Subkey(_masterKey, HeaderSubkey.Hmac);
                }
                return _hmacSubkey;
            }
        }

        Subkey _dataSubkey;

        public Subkey DataSubkey
        {
            get
            {
                if (_dataSubkey == null)
                {
                    if (_masterKey == null)
                    {
                        return null;
                    }
                    _dataSubkey = new Subkey(_masterKey, HeaderSubkey.Data);
                }
                return _dataSubkey;
            }
        }

        public byte[] GetHmac()
        {
            PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();

            return headerBlock.GetHmac();
        }

        public void SetHmac(byte[] hmac)
        {
            if (hmac == null)
            {
                throw new ArgumentNullException("hmac");
            }
            PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();
            headerBlock.SetHmac(hmac);
        }

        public string AnsiFileName
        {
            get
            {
                FileNameInfoHeaderBlock headerBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();

                string fileName = headerBlock.GetFileName(HeaderCrypto);
                return fileName;
            }
        }

        public string UnicodeFileName
        {
            get
            {
                UnicodeFileNameInfoHeaderBlock headerBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
                if (headerBlock == null)
                {
                    // Unicode file name was added in 1.6.3.3 - if we can't find it signal it's absence with an empty string.
                    return String.Empty;
                }

                string fileName = headerBlock.GetFileName(HeaderCrypto);
                return fileName;
            }
        }

        public string FileName
        {
            get
            {
                UnicodeFileNameInfoHeaderBlock unicodeHeaderBlock = FindHeaderBlock<UnicodeFileNameInfoHeaderBlock>();
                if (unicodeHeaderBlock != null)
                {
                    return unicodeHeaderBlock.GetFileName(HeaderCrypto);
                }
                FileNameInfoHeaderBlock ansiHeaderBlock = FindHeaderBlock<FileNameInfoHeaderBlock>();
                return ansiHeaderBlock.GetFileName(HeaderCrypto);
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

                return headerBlock.IsCompressed(HeaderCrypto);
            }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();

                return headerBlock.GetCreationTimeUtc(HeaderCrypto);
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();

                return headerBlock.GetLastAccessTimeUtc(HeaderCrypto);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                FileInfoHeaderBlock headerBlock = FindHeaderBlock<FileInfoHeaderBlock>();

                return headerBlock.GetLastWriteTimeUtc(HeaderCrypto);
            }
        }

        /// <summary>
        /// The Initial Vector used for CBC encryption of the data
        /// </summary>
        /// <returns>The Initial Vector</returns>
        public byte[] GetIV()
        {
            EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();

            byte[] iv = headerBlock.GetIV(HeaderCrypto);
            return iv;
        }

        /// <summary>
        /// The length in bytes of the plain text. This may still require decompression (inflate).
        /// </summary>
        public long PlaintextLength
        {
            get
            {
                EncryptionInfoHeaderBlock headerBlock = FindHeaderBlock<EncryptionInfoHeaderBlock>();

                return headerBlock.GetPlaintextLength(HeaderCrypto);
            }
        }

        private AesCrypto _headerCrypto;

        private AesCrypto HeaderCrypto
        {
            get
            {
                if (_headerCrypto == null)
                {
                    Subkey headersSubkey = new Subkey(GetMasterKey(), HeaderSubkey.Headers);
                    _headerCrypto = new AesCrypto(headersSubkey.Get());
                }
                return _headerCrypto;
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

        private T FindHeaderBlock<T>() where T : class
        {
            foreach (HeaderBlock headerBlock in HeaderBlocks)
            {
                T typedHeaderHeaderBlock = headerBlock as T;
                if (typedHeaderHeaderBlock != null)
                {
                    return typedHeaderHeaderBlock;
                }
            }
            return null;
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_headerCrypto != null)
                {
                    _headerCrypto.Dispose();
                    _headerCrypto = null;
                }
            }

            _disposed = true;
        }
    }
}