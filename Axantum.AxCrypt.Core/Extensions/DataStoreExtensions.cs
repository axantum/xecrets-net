#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class DataStoreExtensions
    {
        public static FileInfoTypes Type(this IDataItem fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            if (!fileInfo.IsAvailable)
            {
                return FileInfoTypes.NonExisting;
            }
            if (fileInfo.IsFolder)
            {
                return FileInfoTypes.Folder;
            }
            if (fileInfo.IsEncrypted())
            {
                return FileInfoTypes.EncryptedFile;
            }
            if (New<FileFilter>().IsEncryptable(fileInfo))
            {
                return FileInfoTypes.EncryptableFile;
            }
            return FileInfoTypes.OtherFile;
        }

        public static bool IsEncrypted(this IDataItem fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            return String.Compare(Resolve.Portable.Path().GetExtension(fullName.Name), OS.Current.AxCryptExtension, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool IsLegacyV1(this IDataStore dataStore)
        {
            return dataStore.IsEncrypted() && OpenFileProperties.Create(dataStore).IsLegacyV1;
        }

        public static IEnumerable<DecryptionParameter> DecryptionParameters(this IDataStore dataStore, Passphrase password, IEnumerable<IAsymmetricPrivateKey> privateKeys)
        {
            if (privateKeys == null)
            {
                throw new ArgumentNullException(nameof(privateKeys));
            }

            if (!dataStore.IsEncrypted())
            {
                return new DecryptionParameter[0];
            }

            if (dataStore.IsLegacyV1())
            {
                return DecryptionParameter.CreateAll(new Passphrase[] { password }, privateKeys, new Guid[] { new V1Aes128CryptoFactory().CryptoId });
            }

            return DecryptionParameter.CreateAll(new Passphrase[] { password }, privateKeys, Resolve.CryptoFactory.OrderedIds.Where(id => id != new V1Aes128CryptoFactory().CryptoId));
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable", Justification = "Encryptable is a word.")]
        public static IEnumerable<IDataStore> ListEncryptable(this IDataContainer folderPath, IEnumerable<IDataContainer> ignoreFolders, FolderOperationMode folderOperationMode)
        {
            return folderPath.ListOfFiles(ignoreFolders, folderOperationMode).Where(fileInfo => fileInfo.IsEncryptable && New<FileFilter>().IsEncryptable(fileInfo));
        }

        public static IEnumerable<IDataStore> ListEncrypted(this IDataContainer folderPath, IEnumerable<IDataContainer> ignoreFolders, FolderOperationMode folderOperationMode)
        {
            return folderPath.ListOfFiles(ignoreFolders, folderOperationMode).Where(fileInfo => fileInfo.IsEncrypted());
        }

        /// <summary>
        /// Create a file name based on an existing, but convert the file name to the pattern used by
        /// AxCrypt for encrypted files. The original must not already be in that form.
        /// </summary>
        /// <param name="fileInfo">A file name representing a file that is not encrypted</param>
        /// <returns>A corresponding file name representing the encrypted version of the original</returns>
        /// <exception cref="InternalErrorException">Can't get encrypted name for a file that already has the encrypted extension.</exception>
        public static IDataStore CreateEncryptedName(this IDataItem fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            if (fullName.IsEncrypted())
            {
                throw new InternalErrorException("Can't get encrypted name for a file that cannot be encrypted.");
            }

            string encryptedName = fullName.FullName.CreateEncryptedName();
            return New<IDataStore>(encryptedName);
        }

        /// <summary>
        /// Creates a random unique unique name in the same folder.
        /// </summary>
        /// <param name="fileInfo">The file information representing the new unique random name.</param>
        /// <returns></returns>
        public static IDataStore CreateRandomUniqueName(this IDataItem fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            while (true)
            {
                int r = Math.Abs(BitConverter.ToInt32(Resolve.RandomGenerator.Generate(sizeof(int)), 0));
                string alternatePath = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(fileInfo.FullName), r.ToString(CultureInfo.InvariantCulture) + Resolve.Portable.Path().GetExtension(fileInfo.FullName));
                IDataStore alternateFileInfo = New<IDataStore>(alternatePath);
                if (!alternateFileInfo.IsAvailable)
                {
                    return alternateFileInfo;
                }
            }
        }

        public static void RandomRename(this IDataStore dataStore)
        {
            dataStore.MoveTo(dataStore.CreateRandomUniqueName().FullName);
        }

        public static void RestoreRandomRename(this IDataStore dataStore, LogOnIdentity identity)
        {
            EncryptedProperties encryptedProperties = EncryptedProperties.Create(dataStore, identity);
            if (!encryptedProperties.IsValid)
            {
                return;
            }

            string destinationFilePath = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(dataStore.FullName), encryptedProperties.FileMetaData.FileName.CreateEncryptedName());
            if (string.Equals(dataStore.FullName, destinationFilePath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            using (FileLock lockedSave = destinationFilePath.CreateUniqueFile())
            {
                dataStore.MoveTo(lockedSave.DataStore.FullName);
            }
        }

        public static bool IsInUse(this IDataStore dataStore)
        {
            if (dataStore.IsLocked())
            {
                return true;
            }

            ActiveFile activeFile = New<FileSystemState>().FindActiveFileFromEncryptedPath(dataStore.FullName);
            if (activeFile?.Status == ActiveFileStatus.AssumedOpenAndDecrypted)
            {
                return true;
            }

            return false;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "The Out pattern is used in the .NET framework.")]
        public static LogOnIdentity TryFindPassphrase(this IDataStore fileInfo, out Guid cryptoId)
        {
            cryptoId = Guid.Empty;
            if (!fileInfo.IsEncrypted())
            {
                return null;
            }

            foreach (LogOnIdentity knownKey in Resolve.KnownIdentities.Identities)
            {
                IEnumerable<DecryptionParameter> decryptionParameters = fileInfo.DecryptionParameters(knownKey.Passphrase, knownKey.PrivateKeys);
                DecryptionParameter decryptionParameter = New<AxCryptFactory>().FindDecryptionParameter(decryptionParameters, fileInfo);
                if (decryptionParameter != null)
                {
                    cryptoId = decryptionParameter.CryptoId;
                    return knownKey;
                }
            }
            return null;
        }

        public static bool IsFileKeyKnown(this IEnumerable<IDataStore> files)
        {
            foreach (IDataStore fileInfo in files)
            {
                Guid cryptoId;
                LogOnIdentity logOnIdentity = fileInfo.TryFindPassphrase(out cryptoId);
                if (logOnIdentity != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Reads the entire data store and returns the content as a byte array.
        /// </summary>
        /// <param name="dataStore">The data store.</param>
        /// <returns>All of the content as a byte array</returns>
        public static byte[] ToArray(this IDataStore dataStore)
        {
            if (dataStore == null)
            {
                throw new ArgumentNullException(nameof(dataStore));
            }

            using (Stream source = dataStore.OpenRead())
            {
                using (MemoryStream destination = new MemoryStream())
                {
                    source.CopyTo(destination);
                    return destination.ToArray();
                }
            }
        }

        public static IEnumerable<IDataStore> ListOfFiles(this IDataContainer folderPath, IEnumerable<IDataContainer> ignoreFolders, FolderOperationMode folderOperationMode)
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException("folderPath");
            }

            IEnumerable<IDataStore> files = folderPath.Files;
            if (folderOperationMode == FolderOperationMode.SingleFolder)
            {
                return files;
            }

            IEnumerable<IDataContainer> folders = folderPath.Folders.Where(folderInfo => !ignoreFolders.Any(x => x.FullName == folderInfo.FullName));
            IEnumerable<IDataStore> subFolderFiles = folders.SelectMany(folder => folder.ListOfFiles(ignoreFolders, folderOperationMode));

            return files.Concat(subFolderFiles);
        }
    }
}