using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class RuntimeFileInfoExtensions
    {
        public static FileInfoType Type(this IRuntimeFileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return FileInfoType.NonExisting;
            }
            if (fileInfo.IsFolder)
            {
                return FileInfoType.Folder;
            }
            if (fileInfo.IsEncryptable())
            {
                return FileInfoType.EncryptableFile;
            }
            if (fileInfo.IsEncrypted())
            {
                return FileInfoType.EncryptedFile;
            }
            return FileInfoType.OtherFile;
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
                return OS.Current.FileInfo(normalizedFolder + Path.DirectorySeparatorChar);
            }
            return OS.Current.FileInfo(normalizedFolder.Substring(0, normalizedFolder.Length - (directorySeparatorChars - 1)));
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

            return OS.Current.FileInfo(encryptedName);
        }
    }
}