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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Header;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables a single point of interaction for an AxCrypt encrypted stream with all but the data available
    /// in-memory.
    /// </summary>
    public class V1AxCryptDocument : IDisposable
    {
        public V1AxCryptDocument(ICrypto keyEncryptingCrypto)
        {
            DocumentHeaders = new V1DocumentHeaders(keyEncryptingCrypto);
        }

        public V1DocumentHeaders DocumentHeaders { get; private set; }

        private AxCryptReader _reader;

        public bool PassphraseIsValid { get; set; }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="stream">The stream to read from. Will be disposed when this instance is disposed.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(Stream stream)
        {
            _reader = AxCryptReader.Create(stream);
            PassphraseIsValid = DocumentHeaders.Load(_reader);

            return PassphraseIsValid;
        }

        /// <summary>
        /// Encrypt a stream with a given set of headers and write to an output stream. The caller is responsible for consistency and completeness
        /// of the headers. Headers that are not known until encryption and compression are added here.
        /// </summary>
        /// <param name="outputDocumentHeaders"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        public void EncryptTo(Stream inputStream, Stream outputStream, AxCryptOptions options, IProgressContext progress)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            if (!outputStream.CanSeek)
            {
                throw new ArgumentException("The output stream must support seek in order to back-track and write the HMAC.");
            }
            if (options.HasMask(AxCryptOptions.EncryptWithCompression) && options.HasMask(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, cannot specify both with and without compression.");
            }
            if (!options.HasMask(AxCryptOptions.EncryptWithCompression) && !options.HasMask(AxCryptOptions.EncryptWithoutCompression))
            {
                throw new ArgumentException("Invalid options, must specify either with or without compression.");
            }
            bool isCompressed = options.HasMask(AxCryptOptions.EncryptWithCompression);
            DocumentHeaders.IsCompressed = isCompressed;
            DocumentHeaders.WriteWithoutHmac(outputStream);
            using (ICryptoTransform encryptor = DataCrypto.CreateEncryptingTransform())
            {
                long outputStartPosition = outputStream.Position;
                using (CryptoStream encryptingStream = new CryptoStream(new NonClosingStream(outputStream), encryptor, CryptoStreamMode.Write))
                {
                    if (isCompressed)
                    {
                        EncryptWithCompressionInternal(DocumentHeaders, inputStream, encryptingStream, progress);
                    }
                    else
                    {
                        DocumentHeaders.PlaintextLength = CopyToWithCount(inputStream, encryptingStream, progress);
                    }
                }
                outputStream.Flush();
                DocumentHeaders.CipherTextLength = outputStream.Position - outputStartPosition;
                using (HmacStream outputHmacStream = new HmacStream(DocumentHeaders.HmacSubkey.Key, outputStream))
                {
                    DocumentHeaders.WriteWithHmac(outputHmacStream);
                    outputHmacStream.ReadFrom(outputStream);
                    DocumentHeaders.Hmac = outputHmacStream.HmacResult;
                }

                // Rewind and rewrite the headers, now with the updated HMAC
                DocumentHeaders.WriteWithoutHmac(outputStream);
                outputStream.Position = outputStream.Length;
            }
        }

        private static void EncryptWithCompressionInternal(V1DocumentHeaders outputDocumentHeaders, Stream inputStream, CryptoStream encryptingStream, IProgressContext progress)
        {
            using (ZOutputStream deflatingStream = new ZOutputStream(encryptingStream, -1))
            {
                deflatingStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                CopyToWithCount(inputStream, deflatingStream, progress);
                deflatingStream.FlushMode = JZlib.Z_FINISH;
                deflatingStream.Finish();

                outputDocumentHeaders.UncompressedLength = deflatingStream.TotalIn;
                outputDocumentHeaders.PlaintextLength = deflatingStream.TotalOut;
            }
        }

        private static long CopyToWithCount(Stream inputStream, Stream outputStream, IProgressContext progress)
        {
            return inputStream.CopyToWithCount(outputStream, inputStream, progress);
        }

        /// <summary>
        /// Write a copy of the current encrypted stream. Used to change meta-data
        /// and encryption key(s) etc.
        /// </summary>
        /// <param name="outputStream"></param>
        public void CopyEncryptedTo(V1DocumentHeaders outputDocumentHeaders, Stream cipherStream, IProgressContext progress)
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
            if (!PassphraseIsValid)
            {
                throw new InternalErrorException("Passphrase is not valid.");
            }

            using (HmacStream hmacStreamOutput = new HmacStream(outputDocumentHeaders.HmacSubkey.Key, cipherStream))
            {
                outputDocumentHeaders.WriteWithHmac(hmacStreamOutput);
                using (AxCryptDataStream encryptedDataStream = _reader.CreateEncryptedDataStream(DocumentHeaders.HmacSubkey.Key, DocumentHeaders.CipherTextLength, progress))
                {
                    CopyToWithCount(encryptedDataStream, hmacStreamOutput, progress);

                    if (_reader.Hmac != DocumentHeaders.Hmac)
                    {
                        throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException("HMAC validation error in the input stream.", ErrorStatus.HmacValidationError);
                    }
                }

                outputDocumentHeaders.Hmac = hmacStreamOutput.HmacResult;

                // Rewind and rewrite the headers, now with the updated HMAC
                outputDocumentHeaders.WriteWithoutHmac(cipherStream);
                cipherStream.Position = cipherStream.Length;
            }
        }

        private ICrypto _dataCrypto;

        private ICrypto DataCrypto
        {
            get
            {
                _dataCrypto = new V1AesCrypto(DocumentHeaders.DataSubkey.Key, DocumentHeaders.IV, CipherMode.CBC, PaddingMode.PKCS7);

                return _dataCrypto;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="outputPlaintextStream">The resulting plain text stream.</param>
        public void DecryptTo(Stream outputPlaintextStream, IProgressContext progress)
        {
            if (!PassphraseIsValid)
            {
                throw new InternalErrorException("Passsphrase is not valid!");
            }

            using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
            {
                using (AxCryptDataStream encryptedDataStream = _reader.CreateEncryptedDataStream(DocumentHeaders.HmacSubkey.Key, DocumentHeaders.CipherTextLength, progress))
                {
                    DecryptEncryptedDataStream(outputPlaintextStream, decryptor, encryptedDataStream, progress);
                }
            }

            if (_reader.Hmac != DocumentHeaders.Hmac)
            {
                throw new Axantum.AxCrypt.Core.Runtime.InvalidDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }
        }

        private void DecryptEncryptedDataStream(Stream outputPlaintextStream, ICryptoTransform decryptor, AxCryptDataStream encryptedDataStream, IProgressContext progress)
        {
            Exception savedExceptionIfCloseCausesCryptographicException = null;
            try
            {
                if (DocumentHeaders.IsCompressed)
                {
                    using (CryptoStream deflatedPlaintextStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (ZInputStream inflatedPlaintextStream = new ZInputStream(deflatedPlaintextStream))
                        {
                            try
                            {
                                inflatedPlaintextStream.CopyToWithCount(outputPlaintextStream, encryptedDataStream, progress);
                            }
                            catch (Exception ex)
                            {
                                savedExceptionIfCloseCausesCryptographicException = ex;
                                throw;
                            }
                        }
                    }
                }
                else
                {
                    using (Stream plainStream = new CryptoStream(encryptedDataStream, decryptor, CryptoStreamMode.Read))
                    {
                        try
                        {
                            plainStream.CopyToWithCount(outputPlaintextStream, encryptedDataStream, progress);
                        }
                        catch (Exception ex)
                        {
                            savedExceptionIfCloseCausesCryptographicException = ex;
                            throw;
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                throw savedExceptionIfCloseCausesCryptographicException;
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