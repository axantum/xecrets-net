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
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class RuntimeFileInfoExtensions
    {
        public static FileInfoTypes Type(this IRuntimeFileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return FileInfoTypes.NonExisting;
            }
            if (fileInfo.IsFolder)
            {
                return FileInfoTypes.Folder;
            }
            if (fileInfo.IsEncryptable())
            {
                return FileInfoTypes.EncryptableFile;
            }
            if (fileInfo.IsEncrypted())
            {
                return FileInfoTypes.EncryptedFile;
            }
            return FileInfoTypes.OtherFile;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable is a word.")]
        public static bool IsEncryptable(this IRuntimeFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            foreach (Regex filter in OS.PathFilters)
            {
                if (filter.IsMatch(fileInfo.FullName))
                {
                    return false;
                }
            }
            return !fileInfo.IsEncrypted();
        }

        public static IRuntimeFileInfo NormalizeFolder(this IRuntimeFileInfo folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }

            if (folder.FullName.Length == 0)
            {
                throw new ArgumentException("The path must be a non-empty string.", "folder");
            }

            string normalizedFolder = folder.FullName.Replace(Path.DirectorySeparatorChar == '/' ? '\\' : '/', Path.DirectorySeparatorChar);
            int directorySeparatorChars = 0;
            while (normalizedFolder[normalizedFolder.Length - (directorySeparatorChars + 1)] == Path.DirectorySeparatorChar)
            {
                ++directorySeparatorChars;
            }

            if (directorySeparatorChars == 0)
            {
                return Factory.New<IRuntimeFileInfo>(normalizedFolder + Path.DirectorySeparatorChar);
            }
            return Factory.New<IRuntimeFileInfo>(normalizedFolder.Substring(0, normalizedFolder.Length - (directorySeparatorChars - 1)));
        }

        public static bool IsEncrypted(this IRuntimeFileInfo fullName)
        {
            return String.Compare(Path.GetExtension(fullName.FullName), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable is a word.")]
        public static IEnumerable<IRuntimeFileInfo> ListEncryptable(this IRuntimeFileInfo folderPath)
        {
            return folderPath.Files.Where((IRuntimeFileInfo fileInfo) => { return fileInfo.IsEncryptable(); });
        }

        public static IEnumerable<IRuntimeFileInfo> ListEncrypted(this IRuntimeFileInfo folderPath)
        {
            return folderPath.Files.Where((IRuntimeFileInfo fileInfo) => { return fileInfo.IsEncrypted(); });
        }

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        /// <exception cref="InternalErrorException">Can't get encrypted name for a file that already has the encrypted extension.</exception>
        public static IRuntimeFileInfo CreateEncryptedName(this IRuntimeFileInfo fullName)
        {
            if (fullName.IsEncrypted())
            {
                throw new InternalErrorException("Can't get encrypted name for a file that cannot be encrypted.");
            }

            string extension = Path.GetExtension(fullName.FullName);
            string encryptedName = fullName.FullName;
            encryptedName = encryptedName.Substring(0, encryptedName.Length - extension.Length);
            encryptedName += extension.Replace('.', '-');
            encryptedName += OS.Current.AxCryptExtension;

            return Factory.New<IRuntimeFileInfo>(encryptedName);
        }

        public static bool TryFindDecryptionKey(this IRuntimeFileInfo fileInfo, out SymmetricKey key)
        {
            foreach (SymmetricKey knownKey in Instance.KnownKeys.Keys)
            {
                using (V1AxCryptDocument document = Factory.New<AxCryptFile>().Document(fileInfo, knownKey, new ProgressContext()))
                {
                    if (document.PassphraseIsValid)
                    {
                        key = knownKey;
                        return true;
                    }
                }
            }
            key = null;
            return false;
        }
    }
}