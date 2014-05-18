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
    public class V1AxCryptDocument : IAxCryptDocument
    {
        private AxCryptReader _reader;

        private V1HmacStream _hmacStream;

        private long _expectedTotalHmacLength = 0;

        public ICryptoFactory CryptoFactory { get; private set; }

        public V1AxCryptDocument()
        {
            CryptoFactory = new V1Aes128CryptoFactory();
        }

        public V1AxCryptDocument(Passphrase passphrase, long keyWrapIterations)
            : this()
        {
            DocumentHeaders = new V1DocumentHeaders(passphrase, keyWrapIterations);
        }

        public V1DocumentHeaders DocumentHeaders { get; private set; }

        public bool PassphraseIsValid { get; set; }

        public bool Load(Passphrase key, Guid cryptoId, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.Load(inputStream);

            return Load(key, reader, headers);
        }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="inputStream">The stream to read from. Will be disposed when this instance is disposed.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        public bool Load(Passphrase key, AxCryptReader reader, Headers headers)
        {
            _reader = reader;
            DocumentHeaders = new V1DocumentHeaders(key);
            PassphraseIsValid = DocumentHeaders.Load(headers);
            if (!PassphraseIsValid)
            {
                return false;
            }

            _hmacStream = new V1HmacStream(DocumentHeaders.HmacSubkey.Key.DerivedKey);
            foreach (HeaderBlock header in DocumentHeaders.Headers.HeaderBlocks)
            {
                if (header.HeaderBlockType != HeaderBlockType.Preamble)
                {
                    header.Write(_hmacStream);
                }
            }
            return true;
        }

        /// <summary>
        /// Encrypt a stream with a given set of headers and write to an output stream. The caller is responsible for consistency and completeness
        /// of the headers. Headers that are not known until encryption and compression are added here.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public void EncryptTo(Stream inputStream, Stream outputStream, AxCryptOptions options)
        {
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
                        EncryptWithCompressionInternal(DocumentHeaders, inputStream, encryptingStream);
                    }
                    else
                    {
                        DocumentHeaders.PlaintextLength = StreamExtensions.CopyTo(inputStream, encryptingStream);
                    }
                }
                outputStream.Flush();
                DocumentHeaders.CipherTextLength = outputStream.Position - outputStartPosition;
                using (V1HmacStream outputHmacStream = new V1HmacStream(DocumentHeaders.HmacSubkey.Key.DerivedKey, outputStream))
                {
                    DocumentHeaders.WriteWithHmac(outputHmacStream);
                    outputHmacStream.ReadFrom(outputStream);
                    DocumentHeaders.Headers.Hmac = outputHmacStream.HmacResult;
                }

                // Rewind and rewrite the headers, now with the updated HMAC
                DocumentHeaders.WriteWithoutHmac(outputStream);
                outputStream.Position = outputStream.Length;
            }
        }

        private static void EncryptWithCompressionInternal(V1DocumentHeaders outputDocumentHeaders, Stream inputStream, CryptoStream encryptingStream)
        {
            using (ZOutputStream deflatingStream = new ZOutputStream(encryptingStream, -1))
            {
                deflatingStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                inputStream.CopyTo(deflatingStream);
                deflatingStream.FlushMode = JZlib.Z_FINISH;
                deflatingStream.Finish();

                outputDocumentHeaders.UncompressedLength = deflatingStream.TotalIn;
                outputDocumentHeaders.PlaintextLength = deflatingStream.TotalOut;
            }
        }

        /// <summary>
        /// Write a copy of the current encrypted stream. Used to change meta-data
        /// and encryption key(s) etc.
        /// </summary>
        /// <param name="outputStream"></param>
        public void CopyEncryptedTo(V1DocumentHeaders outputDocumentHeaders, Stream cipherStream)
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

            using (V1HmacStream hmacStreamOutput = new V1HmacStream(outputDocumentHeaders.HmacSubkey.Key.DerivedKey, cipherStream))
            {
                outputDocumentHeaders.WriteWithHmac(hmacStreamOutput);
                using (V1AxCryptDataStream encryptedDataStream = CreateEncryptedDataStream(_reader.InputStream, DocumentHeaders.CipherTextLength))
                {
                    encryptedDataStream.CopyTo(hmacStreamOutput);

                    if (Hmac != DocumentHeaders.Headers.Hmac)
                    {
                        throw new Axantum.AxCrypt.Core.Runtime.IncorrectDataException("HMAC validation error in the input stream.", ErrorStatus.HmacValidationError);
                    }
                }

                outputDocumentHeaders.Headers.Hmac = hmacStreamOutput.HmacResult;

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
                _dataCrypto = Instance.CryptoFactory.Legacy.CreateCrypto(DocumentHeaders.DataSubkey.Key.DerivedKey, DocumentHeaders.IV, 0);

                return _dataCrypto;
            }
        }

        /// <summary>
        /// Decrypts the encrypted data to the given stream
        /// </summary>
        /// <param name="outputPlaintextStream">The resulting plain text stream.</param>
        public void DecryptTo(Stream outputPlaintextStream)
        {
            if (!PassphraseIsValid)
            {
                throw new InternalErrorException("Passsphrase is not valid!");
            }

            using (ICryptoTransform decryptor = DataCrypto.CreateDecryptingTransform())
            {
                using (V1AxCryptDataStream encryptedDataStream = CreateEncryptedDataStream(_reader.InputStream, DocumentHeaders.CipherTextLength))
                {
                    encryptedDataStream.DecryptTo(outputPlaintextStream, decryptor, DocumentHeaders.IsCompressed);
                }
            }

            if (Hmac != DocumentHeaders.Headers.Hmac)
            {
                throw new Axantum.AxCrypt.Core.Runtime.IncorrectDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }
        }

        private V1AxCryptDataStream CreateEncryptedDataStream(Stream inputStream, long cipherTextLength)
        {
            if (_reader.CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("GetEncryptedDataStream() was called when the reader is not positioned at the data.");
            }

            _reader.SetEndOfStream();

            _expectedTotalHmacLength = _hmacStream.Position + cipherTextLength;

            V1AxCryptDataStream encryptedDataStream = new V1AxCryptDataStream(inputStream, _hmacStream, cipherTextLength);
            return encryptedDataStream;
        }

        private Hmac Hmac
        {
            get
            {
                if (_hmacStream.Length != _expectedTotalHmacLength)
                {
                    throw new InvalidOperationException("There is no valid HMAC until the encrypted data stream is read to end.");
                }
                return _hmacStream.HmacResult;
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
                if (_hmacStream != null)
                {
                    _hmacStream.Dispose();
                    _hmacStream = null;
                }
            }

            _disposed = true;
        }

        public string FileName
        {
            get { return DocumentHeaders.FileName; }
            set { DocumentHeaders.FileName = value; }
        }

        public DateTime CreationTimeUtc
        {
            get { return DocumentHeaders.CreationTimeUtc; }
            set { DocumentHeaders.CreationTimeUtc = value; }
        }

        public DateTime LastAccessTimeUtc
        {
            get { return DocumentHeaders.LastAccessTimeUtc; }
            set { DocumentHeaders.LastAccessTimeUtc = value; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return DocumentHeaders.LastWriteTimeUtc; }
            set { DocumentHeaders.LastWriteTimeUtc = value; }
        }
    }
}