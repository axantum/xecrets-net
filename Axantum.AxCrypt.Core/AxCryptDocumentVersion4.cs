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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.UI;
using Org.BouncyCastle.Utilities.Zlib;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// Enables a single point of interaction for an AxCrypt File Format Version 4 encrypted stream with all but the data available
    /// in-memory. File Format Version 4 is only supported by AxCrypt 2.x or higher. It builds on, and is similar to, File Format
    /// Version 3. See the specification titled "AxCrypt Version 2 Algorithms and File Format" for details.
    /// </summary>
    public class AxCryptDocumentVersion4
    {
        public AxCryptDocumentVersion4()
        {
        }

        public DocumentHeaders DocumentHeaders { get; private set; }

        //private AxCryptReader _reader;

        public bool PassphraseIsValid { get; set; }

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
            using (ICryptoTransform encryptor = CreateEncryptingTransform())
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
                        DocumentHeaders.PlaintextLength = inputStream.CopyToWithCount(encryptingStream, inputStream, progress);
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

        private static void EncryptWithCompressionInternal(DocumentHeaders outputDocumentHeaders, Stream inputStream, CryptoStream encryptingStream, IProgressContext progress)
        {
            using (ZOutputStream deflatingStream = new ZOutputStream(encryptingStream, -1))
            {
                deflatingStream.FlushMode = JZlib.Z_SYNC_FLUSH;
                inputStream.CopyToWithCount(deflatingStream, inputStream, progress);
                deflatingStream.FlushMode = JZlib.Z_FINISH;
                deflatingStream.Finish();

                outputDocumentHeaders.UncompressedLength = deflatingStream.TotalIn;
                outputDocumentHeaders.PlaintextLength = deflatingStream.TotalOut;
            }
        }

        private ICryptoTransform CreateEncryptingTransform()
        {
            return null;
        }
    }
}