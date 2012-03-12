#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.Reader;
using Org.BouncyCastle.Utilities.Zlib;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables an single point of interaction for a an AxCrypt encrypted stream with all but the data available
    /// in-memory.
    /// </summary>
    public class AxCryptDocument : IDisposable
    {
        public AxCryptDocument()
        {
        }

        private AxCryptReader _axCryptReader;
        private byte[] _masterKey;
        private byte[] _calculatedHmac;

        private IList<HeaderBlock> HeaderBlocks { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="axCryptReader">The reader.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(AxCryptReader axCryptReader)
        {
            _axCryptReader = axCryptReader;
            LoadHeaders();

            return GetMasterKey() != null;
        }

        private void LoadHeaders()
        {
            _axCryptReader.Read();
            if (_axCryptReader.CurrentItemType != AxCryptItemType.MagicGuid)
            {
                throw new FileFormatException("No magic Guid was found.", ErrorStatus.MagicGuidMissing);
            }
            HeaderBlocks = new List<HeaderBlock>();
            while (_axCryptReader.Read())
            {
                switch (_axCryptReader.CurrentItemType)
                {
                    case AxCryptItemType.HeaderBlock:
                        HeaderBlocks.Add(_axCryptReader.CurrentHeaderBlock);
                        break;
                    case AxCryptItemType.Data:
                        EnsureFileFormatVersion();
                        return;
                    default:
                        throw new InternalErrorException("The reader returned an AxCryptItemType it should not be possible for it to return.");
                }
            }
            throw new FileFormatException("Premature end of stream.", ErrorStatus.EndOfStream);
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

        public byte[] GetMasterKey()
        {
            if (_masterKey == null)
            {
                KeyWrap1HeaderBlock keyHeaderBlock = FindHeaderBlock<KeyWrap1HeaderBlock>();
                byte[] wrappedKeyData = keyHeaderBlock.GetKeyData();
                byte[] salt = keyHeaderBlock.GetSalt();
                byte[] keyEncryptingKey = _axCryptReader.Settings.GetDerivedPassphrase();
                VersionHeaderBlock versionHeaderBlock = FindHeaderBlock<VersionHeaderBlock>();
                if (versionHeaderBlock.FileVersionMajor <= 1)
                {
                    byte[] badKey = new byte[keyEncryptingKey.Length];
                    Array.Copy(keyEncryptingKey, 0, badKey, 0, 4);
                    keyEncryptingKey = badKey;

                    byte[] badSalt = new byte[salt.Length];
                    Array.Copy(salt, 0, badSalt, 0, 4);
                    salt = badSalt;
                }

                long iterations = keyHeaderBlock.Iterations();
                byte[] unwrappedKeyData = null;
                using (KeyWrap keyWrap = new KeyWrap(keyEncryptingKey, salt, iterations, KeyWrapMode.AxCrypt))
                {
                    unwrappedKeyData = keyWrap.Unwrap(wrappedKeyData);
                    if (!KeyWrap.IsKeyUnwrapValid(unwrappedKeyData))
                    {
                        return null;
                    }
                }
                _masterKey = KeyWrap.GetKeyBytes(unwrappedKeyData);
            }
            return _masterKey;
        }

        public byte[] GetHmac()
        {
            PreambleHeaderBlock headerBlock = FindHeaderBlock<PreambleHeaderBlock>();

            return headerBlock.GetHmac();
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

        private AesCrypto _dataCrypto;

        private AesCrypto DataCrypto
        {
            get
            {
                if (_dataCrypto == null)
                {
                    Subkey dataSubkey = new Subkey(GetMasterKey(), HeaderSubkey.Data);
                    _dataCrypto = new AesCrypto(dataSubkey.Get(), GetIV(), CipherMode.CBC, PaddingMode.PKCS7);
                }
                return _dataCrypto;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="plainTextStream">The plain text stream.</param>
        public void DecryptTo(Stream plaintextStream)
        {
            if (_axCryptReader == null)
            {
                throw new InvalidOperationException("Load() must have been called.");
            }

            if (_axCryptReader.CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("Load() has been called, but appears to have failed.");
            }

            using (HmacStream hmacStream = new HmacStream(new Subkey(GetMasterKey(), HeaderSubkey.Hmac).Get()))
            {
                using (Stream encryptedDataStream = _axCryptReader.CreateEncryptedDataStream(hmacStream))
                {
                    using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
                    {
                        if (IsCompressed)
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (Stream deflatedCryptoStream = new ZInputStream(cryptoStream))
                                {
                                    deflatedCryptoStream.CopyTo(plaintextStream);
                                }
                            }
                        }
                        else
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                            {
                                cryptoStream.CopyTo(plaintextStream);
                            }
                        }
                    }
                }
                _calculatedHmac = hmacStream.GetHmacResult();
            }
            if (!_calculatedHmac.IsEquivalentTo(GetHmac()))
            {
                throw new InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }

            if (_axCryptReader.CurrentItemType != AxCryptItemType.EndOfStream)
            {
                throw new FileFormatException("The stream should end here.", ErrorStatus.FileFormatError);
            }
        }

        public byte[] GetCalculatedHmac()
        {
            return (byte[])_calculatedHmac.Clone();
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
                if (_dataCrypto != null)
                {
                    _dataCrypto.Dispose();
                    _dataCrypto = null;
                }
            }

            _disposed = true;
        }
    }
}