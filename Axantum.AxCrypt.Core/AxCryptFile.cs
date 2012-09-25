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
using System.Globalization;
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;

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
        public static void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options, ProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

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
                        document.EncryptTo(headers, sourceStream, destinationStream, options, progress);
                    }
                }
                if (options.HasMask(AxCryptOptions.SetFileTimes))
                {
                    destinationFile.SetFileTimes(sourceFile.CreationTimeUtc, sourceFile.LastAccessTimeUtc, sourceFile.LastWriteTimeUtc);
                }
            }
        }

        public static void Encrypt(IRuntimeFileInfo sourceFile, Stream destinationStream, AesKey key, AxCryptOptions options, ProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream sourceStream = sourceFile.OpenRead())
            {
                using (AxCryptDocument document = new AxCryptDocument())
                {
                    DocumentHeaders headers = new DocumentHeaders(key);
                    headers.FileName = sourceFile.Name;
                    headers.CreationTimeUtc = sourceFile.CreationTimeUtc;
                    headers.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
                    headers.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;
                    document.DocumentHeaders = headers;
                    document.EncryptTo(headers, sourceStream, destinationStream, options, progress);
                }
            }
        }

        public static void EncryptFileWithBackupAndWipe(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFileInfo, AesKey key, ProgressContext progress)
        {
            if (sourceFileInfo == null)
            {
                throw new ArgumentNullException("sourceFileInfo");
            }
            if (destinationFileInfo == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            using (Stream activeFileStream = sourceFileInfo.OpenRead())
            {
                WriteToFileWithBackup(destinationFileInfo, (Stream destination) =>
                {
                    Encrypt(sourceFileInfo, destination, key, AxCryptOptions.EncryptWithCompression, progress);
                });
            }
            Wipe(sourceFileInfo);
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>true if the passphrase was correct</returns>
        public static bool Decrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, AesKey key, AxCryptOptions options, ProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            using (AxCryptDocument document = Document(sourceFile, key, progress))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                Decrypt(document, destinationFile, options, progress);
            }
            return true;
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>true if the passphrase was correct</returns>
        public static string Decrypt(IRuntimeFileInfo sourceFile, string destinationDirectory, AesKey key, AxCryptOptions options, ProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationDirectory == null)
            {
                throw new ArgumentNullException("destinationDirectory");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            string destinationFileName = null;
            using (AxCryptDocument document = Document(sourceFile, key, progress))
            {
                if (!document.PassphraseIsValid)
                {
                    return destinationFileName;
                }
                destinationFileName = document.DocumentHeaders.FileName;
                IRuntimeFileInfo destinationFullPath = AxCryptEnvironment.Current.FileInfo(Path.Combine(destinationDirectory, destinationFileName));
                Decrypt(document, destinationFullPath, options, progress);
            }
            return destinationFileName;
        }

        /// <summary>
        /// Decrypt from loaded AxCryptDocument to a destination file
        /// </summary>
        /// <param name="document">The loaded AxCryptDocument</param>
        /// <param name="destinationFile">The destination file</param>
        public static void Decrypt(AxCryptDocument document, IRuntimeFileInfo destinationFile, AxCryptOptions options, ProgressContext progress)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Decrypting to '{0}'.".InvariantFormat(destinationFile.Name));
                }

                using (Stream destinationStream = destinationFile.OpenWrite())
                {
                    document.DecryptTo(destinationStream, progress);
                }

                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Decrypted to '{0}'.".InvariantFormat(destinationFile.Name));
                }
            }
            catch (OperationCanceledException)
            {
                if (destinationFile.Exists)
                {
                    destinationFile.Delete();
                }
                throw;
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
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
        public static AxCryptDocument Document(IRuntimeFileInfo sourceFile, AesKey key, ProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            AxCryptDocument document = new AxCryptDocument();
            document.Load(new ProgressStream(sourceFile.OpenRead(), progress), key);
            return document;
        }

        public static void WriteToFileWithBackup(IRuntimeFileInfo destinationFileInfo, Action<Stream> writeFileStreamTo)
        {
            if (destinationFileInfo == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (writeFileStreamTo == null)
            {
                throw new ArgumentNullException("writeFileStreamTo");
            }

            string temporaryFilePath = MakeAlternatePath(destinationFileInfo, ".tmp");
            IRuntimeFileInfo temporaryFileInfo = AxCryptEnvironment.Current.FileInfo(temporaryFilePath);

            try
            {
                using (Stream temporaryStream = temporaryFileInfo.OpenWrite())
                {
                    writeFileStreamTo(temporaryStream);
                }
            }
            catch (OperationCanceledException)
            {
                if (temporaryFileInfo.Exists)
                {
                    temporaryFileInfo.Delete();
                }
                throw;
            }

            if (destinationFileInfo.Exists)
            {
                string backupFilePath = MakeAlternatePath(destinationFileInfo, ".bak");
                IRuntimeFileInfo backupFileInfo = AxCryptEnvironment.Current.FileInfo(destinationFileInfo.FullName);

                backupFileInfo.MoveTo(backupFilePath);
                temporaryFileInfo.MoveTo(destinationFileInfo.FullName);
                backupFileInfo.Delete();
            }
            else
            {
                temporaryFileInfo.MoveTo(destinationFileInfo.FullName);
            }
        }

        private static string MakeAlternatePath(IRuntimeFileInfo fileInfo, string extension)
        {
            string alternatePath;
            IRuntimeFileInfo alternateFileInfo;
            int version = 0;
            do
            {
                string alternateExtension = (version > 0 ? "." + version.ToString(CultureInfo.InvariantCulture) : String.Empty) + extension;
                alternatePath = Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.Name) + alternateExtension);
                alternateFileInfo = AxCryptEnvironment.Current.FileInfo(alternatePath);
                ++version;
            } while (alternateFileInfo.Exists);

            return alternatePath;
        }

        public static string MakeAxCryptFileName(IRuntimeFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            string axCryptExtension = AxCryptEnvironment.Current.AxCryptExtension;
            string originalExtension = Path.GetExtension(fileInfo.Name);
            string modifiedExtension = originalExtension.Length == 0 ? String.Empty : "-" + originalExtension.Substring(1);
            string axCryptFileName = Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.Name) + modifiedExtension + axCryptExtension);

            return axCryptFileName;
        }

        public static void Wipe(IRuntimeFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            if (fileInfo.Exists)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Wiping '{0}'.".InvariantFormat(fileInfo.Name));
                }
                fileInfo.Delete();
            }
        }
    }
}