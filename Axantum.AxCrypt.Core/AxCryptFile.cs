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
        /// <remarks>It is the callers responsibility to ensure that the source file exists, that the the destination file
        /// does not exist and can be created etc.</remarks>
        public static void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase)
        {
            using (Stream sourceStream = sourceFile.OpenRead())
            {
                using (Stream destinationStream = destinationFile.OpenWrite())
                {
                    using (AxCryptDocument document = new AxCryptDocument())
                    {
                        DocumentHeaders headers = new DocumentHeaders(passphrase.DerivedPassphrase);
                        headers.UnicodeFileName = sourceFile.Name;
                        headers.CreationTimeUtc = sourceFile.CreationTimeUtc;
                        headers.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
                        headers.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
                        document.DocumentHeaders = headers;
                        document.EncryptTo(headers, sourceStream, destinationStream);
                    }
                }
            }
        }
    }
}