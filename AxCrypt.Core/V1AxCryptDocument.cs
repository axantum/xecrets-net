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

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.Session;
using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core
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
            AsymmetricRecipients = new UserPublicKey[0];
        }

        public V1AxCryptDocument(AxCryptReader reader)
            : this()
        {
            _reader = reader;
        }

        public V1AxCryptDocument(Passphrase passphrase, long keyWrapIterations)
            : this()
        {
            DocumentHeaders = new V1DocumentHeaders(passphrase, keyWrapIterations);
        }

        public V1DocumentHeaders DocumentHeaders { get; private set; }

        public bool PassphraseIsValid { get; set; }

        public IEnumerable<UserPublicKey> AsymmetricRecipients { get; private set; }

        public IAsymmetricPublicKey AsymmetricMasterKey { get; private set; }

        public IEnumerable<IAsymmetricPublicKey> AsymmetricMasterKeys { get; private set; }

        public DecryptionParameter DecryptionParameter { get; set; }

        public EncryptedProperties Properties { get; private set; }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "cryptoId", Justification = "Part of contract, and is used for other implementations.")]
        public bool Load(Passphrase passphrase, Guid cryptoId, Stream inputStream)
        {
            Headers headers = new Headers();
            AxCryptReader reader = headers.CreateReader(new LookAheadStream(inputStream));

            return Load(passphrase, reader, headers);
        }

        public bool Load(Passphrase passphrase, Guid cryptoId, Headers headers)
        {
            if (cryptoId != new V1Aes128CryptoFactory().CryptoId)
            {
                ResetState(passphrase);
                return false;
            }
            return Load(passphrase, _reader, headers);
        }

        public bool Load(IAsymmetricPrivateKey privateKey, Guid cryptoId, Headers headers)
        {
            ResetState(Passphrase.Empty);
            return false;
        }

        private void ResetState(Passphrase passphrase)
        {
            DocumentHeaders = new V1DocumentHeaders(passphrase);
            PassphraseIsValid = false;
            Properties = EncryptedProperties.Create(this);
            if (_hmacStream != null)
            {
                _hmacStream.Dispose();
                _hmacStream = null;
            }
        }

        /// <summary>
        /// Loads an AxCrypt file from the specified reader. After this, the reader is positioned to
        /// read encrypted data.
        /// </summary>
        /// <param name="inputStream">The stream to read from. Will be disposed when this instance is disposed.</param>
        /// <returns>True if the key was valid, false if it was wrong.</returns>
        private bool Load(Passphrase passphrase, AxCryptReader reader, Headers headers)
        {
            _reader = reader;
            ResetState(passphrase);
            PassphraseIsValid = DocumentHeaders.Load(headers);
            if (!PassphraseIsValid)
            {
                return false;
            }

            _hmacStream = new V1HmacStream(DocumentHeaders.HmacSubkey.Key);
            foreach (HeaderBlock header in DocumentHeaders.Headers.HeaderBlocks)
            {
                if (header.HeaderBlockType != HeaderBlockType.Preamble)
                {
                    header.Write(_hmacStream);
                }
            }

            Properties = EncryptedProperties.Create(this);
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
            using (ICryptoTransform encryptor = DataCrypto.EncryptingTransform())
            {
                long outputStartPosition = outputStream.Position;
                using (Stream encryptingStream = New<CryptoStreamBase>().Initialize(new NonClosingStream(outputStream), encryptor, CryptoStreamMode.Write))
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
                using (V1HmacStream outputHmacStream = new V1HmacStream(DocumentHeaders.HmacSubkey.Key, outputStream))
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

        private static void EncryptWithCompressionInternal(V1DocumentHeaders outputDocumentHeaders, Stream inputStream, Stream encryptingStream)
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

            using (V1HmacStream hmacStreamOutput = new V1HmacStream(outputDocumentHeaders.HmacSubkey.Key, cipherStream))
            {
                outputDocumentHeaders.WriteWithHmac(hmacStreamOutput);
                using (V1AxCryptDataStream encryptedDataStream = CreateEncryptedDataStream(_reader.InputStream, DocumentHeaders.CipherTextLength))
                {
                    encryptedDataStream.CopyTo(hmacStreamOutput);

                    if (Hmac != DocumentHeaders.Headers.Hmac)
                    {
                        throw new AxCrypt.Core.Runtime.IncorrectDataException("HMAC validation error in the input stream.", ErrorStatus.HmacValidationError);
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
                _dataCrypto = Resolve.CryptoFactory.Legacy.CreateCrypto(DocumentHeaders.DataSubkey.Key, DocumentHeaders.IV, 0);

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
                throw new InternalErrorException("Passphrase is not valid!");
            }

            using (ICryptoTransform decryptor = DataCrypto.DecryptingTransform())
            {
                using (V1AxCryptDataStream encryptedDataStream = CreateEncryptedDataStream(_reader.InputStream, DocumentHeaders.CipherTextLength))
                {
                    encryptedDataStream.DecryptTo(outputPlaintextStream, decryptor, DocumentHeaders.IsCompressed);
                }
            }

            if (Hmac != DocumentHeaders.Headers.Hmac)
            {
                throw new AxCrypt.Core.Runtime.IncorrectDataException("HMAC validation error.", ErrorStatus.HmacValidationError);
            }
        }

        public bool VerifyHmac()
        {
            using (V1AxCryptDataStream encryptedDataStream = CreateEncryptedDataStream(_reader.InputStream, DocumentHeaders.CipherTextLength))
            {
                encryptedDataStream.CopyTo(Stream.Null);
            }

            return Hmac == DocumentHeaders.Headers.Hmac;
        }

        private V1AxCryptDataStream CreateEncryptedDataStream(Stream inputStream, long cipherTextLength)
        {
            if (_reader.CurrentItemType != AxCryptItemType.Data)
            {
                throw new InvalidOperationException("An attempt to create an encrypted data stream was made when the reader is not positioned at the data.");
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