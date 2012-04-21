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

        private AxCryptReader _reader;

        public bool PassphraseIsValid { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="stream">The stream to read from. Will be disposed when this instance is disposed.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(Stream stream, Passphrase passphrase)
        {
            _reader = AxCryptReader.Create(stream);
            DocumentHeaders documentHeaders = new DocumentHeaders(passphrase.DerivedPassphrase);
            PassphraseIsValid = documentHeaders.Load(_reader);
            if (PassphraseIsValid)
            {
                DocumentHeaders = documentHeaders;
            }
            return PassphraseIsValid;
        }

        /// <summary>
        /// Encrypt a stream with a given set of headers and write to an output stream. The caller is responsible for consistency and completeness
        /// of the headers. Headers that are not known until encryption and compression are added here.
        /// </summary>
        /// <param name="outputDocumentHeaders"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public void EncryptTo(DocumentHeaders outputDocumentHeaders, Stream inputStream, Stream outputStream, AxCryptOptions options)
        {
            if (outputDocumentHeaders == null)
            {
                throw new ArgumentNullException("outputDocumentHeaders");
            }
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            if (!outputStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }
            if (options.HasFlag(AxCryptOptions.EncryptWithCompression) && options.HasFlag(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, cannot specify both with and without compression.");
            }
            if (!options.HasFlag(AxCryptOptions.EncryptWithCompression) && !options.HasFlag(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, must specify either with or without compression.");
            }
            bool isCompressed = options.HasFlag(AxCryptOptions.EncryptWithCompression);
            outputDocumentHeaders.IsCompressed = isCompressed;
            outputDocumentHeaders.WriteWithoutHmac(outputStream);
            using (ICryptoTransform encryptor = DataCrypto.CreateEncryptingTransform())
            {
                long outputStartPosition = outputStream.Position;
                using (CryptoStream encryptingStream = new CryptoStream(new NonClosingStream(outputStream), encryptor, CryptoStreamMode.Write))
                {
                    if (isCompressed)
                    {
                        EncryptWithCompressionInternal(outputDocumentHeaders, inputStream, encryptingStream);
                    }
                    else
                    {
                        outputDocumentHeaders.PlaintextLength = CopyToWithCount(inputStream, encryptingStream);
                    }
                }
                outputStream.Flush();
                outputDocumentHeaders.CipherTextLength = outputStream.Position - outputStartPosition;
                using (HmacStream outputHmacStream = new HmacStream(outputDocumentHeaders.HmacSubkey.Key, outputStream))
                {
                    outputDocumentHeaders.WriteWithHmac(outputHmacStream);
                    outputHmacStream.ReadFrom(outputStream);
                    outputDocumentHeaders.Hmac = outputHmacStream.HmacResult;
                }

                // Rewind and rewrite the headers, now with the updated HMAC
                outputDocumentHeaders.WriteWithoutHmac(outputStream);
                outputStream.Position = outputStream.Length;
            }
        }

        private static void EncryptWithCompressionInternal(DocumentHeaders outputDocumentHeaders, Stream inputStream, CryptoStream encryptingStream)
        {
            using (ZOutputStream deflatingStream = new ZOutputStream(encryptingStream, -1))
            {
                deflatingStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                CopyToWithCount(inputStream, deflatingStream);
                deflatingStream.FlushMode = JZlib.Z_FINISH;
                deflatingStream.Finish();

                outputDocumentHeaders.UncompressedLength = deflatingStream.TotalIn;
                outputDocumentHeaders.PlaintextLength = deflatingStream.TotalOut;
            }
        }

        private static long CopyToWithCount(Stream inputStream, Stream outputStream)
        {
            byte[] buffer = new byte[4096];
            int count;
            long totalCount = 0;
            while ((count = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                outputStream.Write(buffer, 0, count);
                totalCount += count;
            }
            return totalCount;
        }

        /// <summary>
        /// Write a copy of the current encrypted stream. Used to change meta-data
        /// and encryption key(s) etc.
        /// </summary>
        /// <param name="outputStream"></param>
        public void CopyEncryptedTo(DocumentHeaders outputDocumentHeaders, Stream cipherStream)
        {
            if (outputDocumentHeaders == null)
            {
                throw new ArgumentNullException("outputDocumentHeaders");
            }
            if (cipherStream == null)
            {
                throw new ArgumentNullException("cipherStream");
            }
            if (!cipherStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }
            if (DocumentHeaders == null)
            {
                throw new InternalErrorException("Document headers are not loaded");
            }

            using (HmacStream hmacStreamOutput = new HmacStream(outputDocumentHeaders.HmacSubkey.Key, cipherStream))
            {
                outputDocumentHeaders.WriteWithHmac(hmacStreamOutput);
                using (Stream encryptedDataStream = _reader.CreateEncryptedDataStream(DocumentHeaders.HmacSubkey.Key, DocumentHeaders.CipherTextLength))
                {
                    encryptedDataStream.CopyTo(hmacStreamOutput);

                    if (_reader.Hmac != DocumentHeaders.Hmac)
                    {
                        throw new InvalidDataException("HMAC validation error in the input stream.", ErrorStatus.HmacValidationError);
                    }
                }

                outputDocumentHeaders.Hmac = hmacStreamOutput.HmacResult;

                // Rewind and rewrite the headers, now with the updated HMAC
                outputDocumentHeaders.WriteWithoutHmac(cipherStream);
                cipherStream.Position = cipherStream.Length;
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
        public void DecryptTo(Stream outputPlaintextStream)
        {
            if (DocumentHeaders == null)
            {
                throw new InternalErrorException("Document headers are not loaded");
            }
            using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
            {
                using (Stream encryptedDataStream = _reader.CreateEncryptedDataStream(DocumentHeaders.HmacSubkey.Key, DocumentHeaders.CipherTextLength))
                {
                    if (DocumentHeaders.IsCompressed)
                    {
                        using (Stream deflatedPlaintextStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (Stream inflatedPlaintextStream = new ZInputStream(deflatedPlaintextStream))
                            {
                                inflatedPlaintextStream.CopyTo(outputPlaintextStream);
                            }
                        }
                    }
                    else
                    {
                        using (Stream plainStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                        {
                            plainStream.CopyTo(outputPlaintextStream);
                        }
                    }
                }
            }
            if (_reader.Hmac != DocumentHeaders.Hmac)
            {
                throw new InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
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
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
            }

            _disposed = true;
        }
    }
}