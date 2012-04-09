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
    /// Enables a single point of interaction for an AxCrypt encrypted stream with all but the data available
    /// in-memory.
    /// </summary>
    public class AxCryptDocument : IDisposable
    {
        public AxCryptDocument()
        {
        }

        private AxCryptReader _axCryptReader;

        private byte[] _calculatedHmac;

        public DocumentHeaders DocumentHeaders { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="axCryptReader">The reader.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(AxCryptReader axCryptReader)
        {
            _axCryptReader = axCryptReader;
            DocumentHeaders documentHeaders = new DocumentHeaders();
            bool loadedOk = documentHeaders.Load(_axCryptReader, _axCryptReader.Settings.GetDerivedPassphrase());
            if (!loadedOk)
            {
                return false;
            }
            DocumentHeaders = documentHeaders;
            return true;
        }

        /// <summary>
        /// Write a copy of the current encrypted stream. Used to change meta-data
        /// and encryption key(s) etc.
        /// </summary>
        /// <param name="outputStream"></param>
        public void CopyEncryptedTo(DocumentHeaders outputDocumentHeaders, Stream cipherStream)
        {
            if (cipherStream == null)
            {
                throw new ArgumentNullException("cipherStream");
            }

            if (!cipherStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }

            EnsureLoaded();

            using (HmacStream hmacStreamInput = new HmacStream(DocumentHeaders.HmacSubkey.Get()))
            {
                using (HmacStream hmacStreamOutput = new HmacStream(outputDocumentHeaders.HmacSubkey.Get(), cipherStream))
                {
                    outputDocumentHeaders.Write(cipherStream, hmacStreamOutput);
                    using (Stream encryptedDataStream = _axCryptReader.CreateEncryptedDataStream(hmacStreamInput))
                    {
                        encryptedDataStream.CopyTo(hmacStreamOutput);

                        if (!hmacStreamInput.GetHmacResult().IsEquivalentTo(DocumentHeaders.GetHmac()))
                        {
                            throw new InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
                        }
                    }

                    outputDocumentHeaders.SetHmac(hmacStreamOutput.GetHmacResult());

                    // Rewind and rewrite the headers, now with the updated HMAC
                    outputDocumentHeaders.Write(cipherStream, null);
                    cipherStream.Position = cipherStream.Length;
                }
            }
        }

        private AesCrypto _dataCrypto;

        private AesCrypto DataCrypto
        {
            get
            {
                if (_dataCrypto == null)
                {
                    Subkey dataSubkey = new Subkey(DocumentHeaders.GetMasterKey(), HeaderSubkey.Data);
                    _dataCrypto = new AesCrypto(dataSubkey.Get(), DocumentHeaders.GetIV(), CipherMode.CBC, PaddingMode.PKCS7);
                }
                return _dataCrypto;
            }
        }

        private void EnsureLoaded()
        {
            if (_axCryptReader == null)
            {
                throw new InvalidOperationException("Load() must have been called.");
            }

            if (_axCryptReader.CurrentItemType == AxCryptItemType.EndOfStream)
            {
                throw new InvalidOperationException("This method can only be called once.");
            }

            if (_axCryptReader.CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("Load() has been called, but appears to have failed.");
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="plainTextStream">The plain text stream.</param>
        public void DecryptTo(Stream plaintextStream)
        {
            EnsureLoaded();

            using (HmacStream hmacStream = new HmacStream(new Subkey(DocumentHeaders.GetMasterKey(), HeaderSubkey.Hmac).Get()))
            {
                using (Stream encryptedDataStream = _axCryptReader.CreateEncryptedDataStream(hmacStream))
                {
                    using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
                    {
                        if (DocumentHeaders.IsCompressed)
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
            if (!_calculatedHmac.IsEquivalentTo(DocumentHeaders.GetHmac()))
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
                if (_dataCrypto != null)
                {
                    _dataCrypto.Dispose();
                    _dataCrypto = null;
                }

                if (DocumentHeaders != null)
                {
                    DocumentHeaders.Dispose();
                    DocumentHeaders = null;
                }
            }

            _disposed = true;
        }
    }
}