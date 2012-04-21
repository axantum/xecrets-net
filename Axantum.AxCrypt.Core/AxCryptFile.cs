﻿#region Coypright and License

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
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;

namespace Axantum.AxCrypt.Core
{
    public static class AxCryptFile
    {
        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="file">The file to encrypt</param>
        /// <param name="destination">The destination file</param>
        /// <remarks>It is the callers responsibility to ensure that the source file exists, that the destination file
        /// does not exist and can be created etc.</remarks>
        public static void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options)
        {
            using (Stream sourceStream = sourceFile.OpenRead())
            {
                using (Stream destinationStream = destinationFile.OpenWrite())
                {
                    using (AxCryptDocument document = new AxCryptDocument())
                    {
                        DocumentHeaders headers = new DocumentHeaders(passphrase.DerivedPassphrase);
                        headers.FileName = sourceFile.Name;
                        headers.CreationTimeUtc = sourceFile.CreationTimeUtc;
                        headers.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
                        headers.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
                        document.DocumentHeaders = headers;
                        document.EncryptTo(headers, sourceStream, destinationStream, options);
                    }
                }
                if (options.HasFlag(AxCryptOptions.SetFileTimes))
                {
                    destinationFile.SetFileTimes(sourceFile.CreationTimeUtc, sourceFile.LastAccessTimeUtc, sourceFile.LastWriteTimeUtc);
                }
            }
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>true if the passphrase was correct</returns>
        public static bool Decrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options)
        {
            using (AxCryptDocument document = Document(sourceFile, passphrase))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                Decrypt(document, destinationFile, options);
            }
            return true;
        }

        /// <summary>
        /// Decrypt from loaded AxCryptDocument to a destination file
        /// </summary>
        /// <param name="document">The loaded AxCryptDocument</param>
        /// <param name="destinationFile">The destination file</param>
        public static void Decrypt(AxCryptDocument document, IRuntimeFileInfo destinationFile, AxCryptOptions options)
        {
            using (Stream destinationStream = destinationFile.OpenWrite())
            {
                document.DecryptTo(destinationStream);
            }
            if (options.HasFlag(AxCryptOptions.SetFileTimes))
            {
                DocumentHeaders headers = document.DocumentHeaders;
                destinationFile.SetFileTimes(headers.CreationTimeUtc, headers.LastAccessTimeUtc, headers.LastWriteTimeUtc);
            }
        }

        /// <summary>
        /// Load an AxCryptDocument from a source file with a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>An instance of AxCryptDocument. Use IsPassphraseValid property to determine validity.</returns>
        public static AxCryptDocument Document(IRuntimeFileInfo sourceFile, Passphrase passphrase)
        {
            AxCryptDocument document = new AxCryptDocument();
            document.Load(sourceFile.OpenRead(), passphrase);
            return document;
        }
    }
}