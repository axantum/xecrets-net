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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Reader;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    public class AxCryptFile
    {
        /// <summary>
        /// Encrypt a file
        /// </summary>
        /// <param name="file">The file to encrypt</param>
        /// <param name="destination">The destination file</param>
        /// <remarks>It is the callers responsibility to ensure that the source file exists, that the destination file
        /// does not exist and can be created etc.</remarks>
        public virtual void Encrypt(IRuntimeFileInfo sourceFile, IRuntimeFileInfo destinationFile, Passphrase passphrase, AxCryptOptions options, ProgressContext progress)
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

        public static void EncryptFileWithBackupAndWipe(string sourceFile, string destinationFile, AesKey key, ProgressContext progress)
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
            IRuntimeFileInfo sourceFileInfo = OS.Current.FileInfo(sourceFile);
            IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(destinationFile);
            EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, key, progress);
        }

        public virtual void EncryptFilesUniqueWithBackupAndWipe(IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress)
        {
            IEnumerable<IRuntimeFileInfo> files = fileInfo.FullName.ListEncryptable();
            foreach (IRuntimeFileInfo file in files)
            {
                EncryptFileUniqueWithBackupAndWipe(file, encryptionKey, progress);
            }
        }

        public virtual void EncryptFileUniqueWithBackupAndWipe(IRuntimeFileInfo fileInfo, AesKey encryptionKey, ProgressContext progress)
        {
            IRuntimeFileInfo destinationFileInfo = fileInfo.CreateEncryptedName();
            destinationFileInfo = OS.Current.FileInfo(destinationFileInfo.FullName.CreateUniqueFile());
            AxCryptFile.EncryptFileWithBackupAndWipe(fileInfo, destinationFileInfo, encryptionKey, progress);
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
            progress.NotifyLevelStart();
            using (Stream activeFileStream = sourceFileInfo.OpenRead())
            {
                WriteToFileWithBackup(destinationFileInfo, (Stream destination) =>
                {
                    Encrypt(sourceFileInfo, destination, key, AxCryptOptions.EncryptWithCompression, progress);
                }, progress);
            }
            Wipe(sourceFileInfo, progress);
            progress.NotifyLevelFinished();
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
            using (AxCryptDocument document = Document(sourceFile, key, new ProgressContext()))
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
            using (AxCryptDocument document = Document(sourceFile, key, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return destinationFileName;
                }
                destinationFileName = document.DocumentHeaders.FileName;
                IRuntimeFileInfo destinationFullPath = OS.Current.FileInfo(Path.Combine(destinationDirectory, destinationFileName));
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
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Decrypting to '{0}'.".InvariantFormat(destinationFile.Name));
                }

                using (Stream destinationStream = destinationFile.OpenWrite())
                {
                    document.DecryptTo(destinationStream, progress);
                }

                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Decrypted to '{0}'.".InvariantFormat(destinationFile.Name));
                }
            }
            catch (OperationCanceledException)
            {
                if (destinationFile.Exists)
                {
                    AxCryptFile.Wipe(destinationFile, progress);
                }
                throw;
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
            {
                DocumentHeaders headers = document.DocumentHeaders;
                destinationFile.SetFileTimes(headers.CreationTimeUtc, headers.LastAccessTimeUtc, headers.LastWriteTimeUtc);
            }
        }

        public virtual void DecryptFilesUniqueWithWipeOfOriginal(IRuntimeFileInfo fileInfo, AesKey decryptionKey, ProgressContext progress)
        {
            IEnumerable<IRuntimeFileInfo> files = fileInfo.FullName.ListEncrypted();
            foreach (IRuntimeFileInfo file in files)
            {
                DecryptFileUniqueWithWipeOfOriginal(file, decryptionKey, progress);
            }
        }

        public static void DecryptFileUniqueWithWipeOfOriginal(IRuntimeFileInfo fileInfo, AesKey decryptionKey, ProgressContext progress)
        {
            progress.NotifyLevelStart();
            using (AxCryptDocument document = AxCryptFile.Document(fileInfo, decryptionKey, progress))
            {
                if (!document.PassphraseIsValid)
                {
                    return;
                }

                IRuntimeFileInfo destinationFileInfo = OS.Current.FileInfo(Path.Combine(Path.GetDirectoryName(fileInfo.FullName), document.DocumentHeaders.FileName));
                destinationFileInfo = OS.Current.FileInfo(destinationFileInfo.FullName.CreateUniqueFile());
                AxCryptFile.DecryptFile(document, destinationFileInfo.FullName, progress);
            }
            AxCryptFile.Wipe(fileInfo, progress);
            progress.NotifyLevelFinished();
        }

        public static void DecryptFile(AxCryptDocument document, string decryptedFileFullName, ProgressContext progress)
        {
            IRuntimeFileInfo decryptedFileInfo = OS.Current.FileInfo(decryptedFileFullName);
            AxCryptFile.Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, progress);
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
            Stream stream = new ProgressStream(sourceFile.OpenRead(), progress);
            progress.AddTotal(stream.Length);
            document.Load(stream, key);
            return document;
        }

        public static void WriteToFileWithBackup(IRuntimeFileInfo destinationFileInfo, Action<Stream> writeFileStreamTo, ProgressContext progress)
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
            IRuntimeFileInfo temporaryFileInfo = OS.Current.FileInfo(temporaryFilePath);

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
                    AxCryptFile.Wipe(temporaryFileInfo, progress);
                }
                throw;
            }

            if (destinationFileInfo.Exists)
            {
                string backupFilePath = MakeAlternatePath(destinationFileInfo, ".bak");
                IRuntimeFileInfo backupFileInfo = OS.Current.FileInfo(destinationFileInfo.FullName);

                backupFileInfo.MoveTo(backupFilePath);
                temporaryFileInfo.MoveTo(destinationFileInfo.FullName);
                AxCryptFile.Wipe(backupFileInfo, progress);
            }
            else
            {
                temporaryFileInfo.MoveTo(destinationFileInfo.FullName);
            }
        }

        private static string MakeAlternatePath(IRuntimeFileInfo fileInfo, string extension)
        {
            string alternatePath = Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.Name) + extension);
            return alternatePath.CreateUniqueFile();
        }

        public static string MakeAxCryptFileName(IRuntimeFileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            string axCryptExtension = OS.Current.AxCryptExtension;
            string originalExtension = Path.GetExtension(fileInfo.Name);
            string modifiedExtension = originalExtension.Length == 0 ? String.Empty : "-" + originalExtension.Substring(1);
            string axCryptFileName = Path.Combine(Path.GetDirectoryName(fileInfo.FullName), Path.GetFileNameWithoutExtension(fileInfo.Name) + modifiedExtension + axCryptExtension);

            return axCryptFileName;
        }

        public static void Wipe(string fullName, ProgressContext progress)
        {
            Wipe(OS.Current.FileInfo(fullName), progress);
        }

        public static void Wipe(IRuntimeFileInfo fileInfo, ProgressContext progress)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            if (!fileInfo.Exists)
            {
                return;
            }
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Wiping '{0}'.".InvariantFormat(fileInfo.Name));
            }
            bool cancelPending = false;
            progress.NotifyLevelStart();
            using (Stream stream = fileInfo.OpenWrite())
            {
                long length = stream.Length + OS.Current.StreamBufferSize - stream.Length % OS.Current.StreamBufferSize;
                progress.AddTotal(length);
                for (long position = 0; position < length; position += OS.Current.StreamBufferSize)
                {
                    byte[] random = OS.Current.GetRandomBytes(OS.Current.StreamBufferSize);
                    stream.Write(random, 0, random.Length);
                    stream.Flush();
                    try
                    {
                        progress.AddCount(random.Length);
                    }
                    catch (OperationCanceledException)
                    {
                        cancelPending = true;
                        progress.Cancel = false;
                        progress.AddCount(random.Length);
                    }
                }
            }
            string randomName;
            do
            {
                randomName = GenerateRandomFileName(fileInfo.FullName);
            } while (OS.Current.FileInfo(randomName).Exists);

            IRuntimeFileInfo moveToFileInfo = OS.Current.FileInfo(fileInfo.FullName);
            moveToFileInfo.MoveTo(randomName);
            moveToFileInfo.Delete();
            progress.NotifyLevelFinished();
            if (cancelPending)
            {
                progress.Cancel = true;
                throw new OperationCanceledException("Delayed cancel during wipe.");
            }
        }

        private static string GenerateRandomFileName(string originalFullName)
        {
            const string validFileNameChars = "abcdefghijklmnopqrstuvwxyz";

            string directory = Path.GetDirectoryName(originalFullName);
            string fileName = Path.GetFileNameWithoutExtension(originalFullName);

            int randomLength = fileName.Length < 8 ? 8 : fileName.Length;
            StringBuilder randomName = new StringBuilder(randomLength + 4);
            byte[] random = OS.Current.GetRandomBytes(randomLength);
            for (int i = 0; i < randomLength; ++i)
            {
                randomName.Append(validFileNameChars[random[i] % validFileNameChars.Length]);
            }
            randomName.Append(".tmp");

            return Path.Combine(directory, randomName.ToString());
        }
    }
}