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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
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

        public DocumentHeaders DocumentHeaders { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="axCryptReader">The reader.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(AxCryptReader axCryptReader, AxCryptReaderSettings settings)
        {
            DocumentHeaders documentHeaders = new DocumentHeaders(settings.DerivedPassphrase);
            bool loadedOk = documentHeaders.Load(axCryptReader);
            if (!loadedOk)
            {
                return false;
            }
            DocumentHeaders = documentHeaders;
            return true;
        }

        /// <summary>
        /// Encrypt a stream with a given set of headers and write to an output stream. The caller is responsible for consistency and completeness
        /// of the headers. Headers that are not known until encryption and compression are added here.
        /// </summary>
        /// <param name="outputDocumentHeaders"></param>
        /// <param name="inputPlainStream"></param>
        /// <param name="outputCipherStream"></param>
        public void EncryptTo(DocumentHeaders outputDocumentHeaders, Stream inputPlainStream, Stream outputCipherStream)
        {
            if (outputDocumentHeaders == null)
            {
                throw new ArgumentNullException("outputDocumentHeaders");
            }
            if (inputPlainStream == null)
            {
                throw new ArgumentNullException("inputPlainStream");
            }
            if (outputCipherStream == null)
            {
                throw new ArgumentNullException("outputCipherStream");
            }
            if (!outputCipherStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }
            outputDocumentHeaders.Write(outputCipherStream, null);
            using (ICryptoTransform encryptor = DataCrypto.CreateEncryptingTransform())
            {
                long cipherStartPosition = outputCipherStream.Position;
                using (CryptoStream deflatedCipherStream = new CryptoStream(new NonClosingStream(outputCipherStream), encryptor, CryptoStreamMode.Write))
                {
                    using (ZOutputStream deflatedPlainStream = new ZOutputStream(deflatedCipherStream, -1))
                    {
                        byte[] buffer = new byte[4096];
                        int count;
                        while ((count = inputPlainStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            deflatedPlainStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                            deflatedPlainStream.Write(buffer, 0, count);
                        }
                        deflatedPlainStream.FlushMode = JZlib.Z_FINISH;
                        deflatedPlainStream.Finish();

                        outputDocumentHeaders.NormalSize = deflatedPlainStream.TotalIn;
                        outputDocumentHeaders.PlaintextLength = deflatedPlainStream.TotalOut;

                        deflatedCipherStream.FlushFinalBlock();
                        outputCipherStream.Flush();
                        outputDocumentHeaders.DataLength = outputCipherStream.Position - cipherStartPosition;
                    }
                }
                using (HmacStream outputHmacStream = new HmacStream(outputDocumentHeaders.HmacSubkey.Key, outputCipherStream))
                {
                    outputDocumentHeaders.Write(outputCipherStream, outputHmacStream);
                    outputHmacStream.ReadFrom(outputCipherStream);
                    outputDocumentHeaders.SetHmac(outputHmacStream.GetHmacResult());
                }

                // Rewind and rewrite the headers, now with the updated HMAC
                outputDocumentHeaders.Write(outputCipherStream, null);
                outputCipherStream.Position = outputCipherStream.Length;
            }
        }

        /// <summary>
        /// Write a copy of the current encrypted stream. Used to change meta-data
        /// and encryption key(s) etc.
        /// </summary>
        /// <param name="outputStream"></param>
        public void CopyEncryptedTo(AxCryptReader axCryptReader, DocumentHeaders outputDocumentHeaders, Stream cipherStream)
        {
            if (cipherStream == null)
            {
                throw new ArgumentNullException("cipherStream");
            }
            if (!cipherStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }

            using (HmacStream hmacStreamInput = new HmacStream(DocumentHeaders.HmacSubkey.Key))
            {
                using (HmacStream hmacStreamOutput = new HmacStream(outputDocumentHeaders.HmacSubkey.Key, cipherStream))
                {
                    outputDocumentHeaders.Write(cipherStream, hmacStreamOutput);
                    axCryptReader.HmacStream = hmacStreamInput;
                    using (Stream encryptedDataStream = axCryptReader.EncryptedDataStream)
                    {
                        encryptedDataStream.CopyTo(hmacStreamOutput);

                        if (!hmacStreamInput.GetHmacResult().GetBytes().IsEquivalentTo(DocumentHeaders.GetHmac().GetBytes()))
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
                    _dataCrypto = new AesCrypto(DocumentHeaders.DataSubkey.Key, DocumentHeaders.IV, CipherMode.CBC, PaddingMode.PKCS7);
                }
                return _dataCrypto;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="outputPlaintextStream">The resulting plain text stream.</param>
        public void DecryptTo(AxCryptReader axCryptReader, Stream outputPlaintextStream)
        {
            if (DocumentHeaders == null)
            {
                throw new InternalErrorException("Document headers are not loaded");
            }
            DataHmac calculatedHmac;
            using (HmacStream hmacStream = new HmacStream(DocumentHeaders.HmacSubkey.Key))
            {
                axCryptReader.HmacStream = hmacStream;
                using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
                {
                    if (DocumentHeaders.IsCompressed)
                    {
                        using (Stream deflatedPlaintextStream = new CryptoStream(axCryptReader.EncryptedDataStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (Stream inflatedPlaintextStream = new ZInputStream(deflatedPlaintextStream))
                            {
                                inflatedPlaintextStream.CopyTo(outputPlaintextStream);
                            }
                        }
                    }
                    else
                    {
                        using (Stream plainStream = new CryptoStream(axCryptReader.EncryptedDataStream, decryptor, CryptoStreamMode.Read))
                        {
                            plainStream.CopyTo(outputPlaintextStream);
                        }
                    }
                }
                calculatedHmac = hmacStream.GetHmacResult();
            }
            if (!calculatedHmac.GetBytes().IsEquivalentTo(DocumentHeaders.GetHmac().GetBytes()))
            {
                throw new InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }

            if (axCryptReader.CurrentItemType != AxCryptItemType.EndOfStream)
            {
                throw new FileFormatException("The stream should end here.", ErrorStatus.FileFormatError);
            }
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