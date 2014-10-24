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
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class RuntimeFileInfoExtensions
    {
        public static FileInfoTypes Type(this IDataItem fileInfo)
        {
            if (!fileInfo.IsAvailable)
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
        public static bool IsEncryptable(this IDataItem fileInfo)
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

        public static bool IsEncrypted(this IDataItem fullName)
        {
            return String.Compare(Resolve.Portable.Path().GetExtension(fullName.Name), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0;
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable is a word.")]
        public static IEnumerable<IDataStore> ListEncryptable(this IDataContainer folderPath)
        {
            return folderPath.Files.Where((IDataStore fileInfo) => { return fileInfo.IsEncryptable(); });
        }

        public static IEnumerable<IDataStore> ListEncrypted(this IDataContainer folderPath)
        {
            return folderPath.Files.Where((IDataStore fileInfo) => { return fileInfo.IsEncrypted(); });
        }

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        /// <exception cref="InternalErrorException">Can't get encrypted name for a file that already has the encrypted extension.</exception>
        public static IDataStore CreateEncryptedName(this IDataStore fullName)
        {
            if (fullName.IsEncrypted())
            {
                throw new InternalErrorException("Can't get encrypted name for a file that cannot be encrypted.");
            }

            string encryptedName = fullName.FullName.CreateEncryptedName();
            return TypeMap.Resolve.New<IDataStore>(encryptedName);
        }

        /// <summary>
        /// Creates a random unique unique name in the same folder.
        /// </summary>
        /// <param name="fileInfo">The file information representing the new unique random name.</param>
        /// <returns></returns>
        public static IDataStore CreateRandomUniqueName(this IDataStore fileInfo)
        {
            while (true)
            {
                int r = Math.Abs(BitConverter.ToInt32(Resolve.RandomGenerator.Generate(sizeof(int)), 0));
                string alternatePath = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(fileInfo.FullName), r.ToString(CultureInfo.InvariantCulture) + Resolve.Portable.Path().GetExtension(fileInfo.FullName));
                IDataStore alternateFileInfo = TypeMap.Resolve.New<IDataStore>(alternatePath);
                if (!alternateFileInfo.IsAvailable)
                {
                    return alternateFileInfo;
                }
            }
        }

        public static Passphrase TryFindPassphrase(this IDataStore fileInfo, out Guid cryptoId)
        {
            cryptoId = Guid.Empty;
            if (!fileInfo.IsEncrypted())
            {
                return null;
            }

            foreach (Passphrase knownKey in Resolve.KnownKeys.Keys)
            {
                cryptoId = TypeMap.Resolve.New<AxCryptFactory>().TryFindCryptoId(knownKey, fileInfo, Resolve.CryptoFactory.OrderedIds);
                if (cryptoId != Guid.Empty)
                {
                    return knownKey;
                }
            }
            return null;
        }
    }
}