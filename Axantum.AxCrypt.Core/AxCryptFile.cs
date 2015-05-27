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
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// File-centric methods for encryption and decryption.
    /// </summary>
    public class AxCryptFile
    {
        public static void Encrypt(Stream sourceStream, string sourceFileName, IDataStore destinationFileInfo, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException("sourceStream");
            }
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileInfo == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream destinationStream = destinationFileInfo.OpenWrite())
            {
                using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFactory>().CreateDocument(encryptionParameters))
                {
                    document.FileName = sourceFileName;
                    document.CreationTimeUtc = OS.Current.UtcNow;
                    document.LastAccessTimeUtc = document.CreationTimeUtc;
                    document.LastWriteTimeUtc = document.CreationTimeUtc;

                    document.EncryptTo(sourceStream, destinationStream, options);
                }
            }
        }

        public static void Encrypt(IDataStore sourceFile, IDataStore destinationFile, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream destinationStream = destinationFile.OpenWrite())
            {
                Encrypt(sourceFile, destinationStream, encryptionParameters, options, progress);
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
            {
                destinationFile.SetFileTimes(sourceFile.CreationTimeUtc, sourceFile.LastAccessTimeUtc, sourceFile.LastWriteTimeUtc);
            }
        }

        public static void Encrypt(IDataStore sourceFile, Stream destinationStream, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream sourceStream = new ProgressStream(sourceFile.OpenRead(), progress))
            {
                using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFactory>().CreateDocument(encryptionParameters))
                {
                    document.FileName = sourceFile.Name;
                    document.CreationTimeUtc = sourceFile.CreationTimeUtc;
                    document.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
                    document.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

                    document.EncryptTo(sourceStream, destinationStream, options);
                }
            }
        }

        public void EncryptFileWithBackupAndWipe(string sourceFile, string destinationFile, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationFile == null)
            {
                throw new ArgumentNullException("destinationFile");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            IDataStore sourceFileInfo = TypeMap.Resolve.New<IDataStore>(sourceFile);
            IDataStore destinationFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFile);
            EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileInfo, encryptionParameters, progress);
        }

        public virtual void EncryptFoldersUniqueWithBackupAndWipe(IEnumerable<IDataContainer> folders, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                IEnumerable<IDataStore> files = folders.SelectMany((folder) => folder.ListEncryptable());
                progress.AddTotal(files.Count());
                foreach (IDataStore file in files)
                {
                    EncryptFileUniqueWithBackupAndWipe(file, encryptionParameters, progress);
                    progress.AddCount(1);
                }
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        public virtual void EncryptFileUniqueWithBackupAndWipe(IDataStore fileInfo, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            IDataStore destinationFileInfo = fileInfo.CreateEncryptedName();
            using (FileLock lockedDestination = destinationFileInfo.FullName.CreateUniqueFile())
            {
                EncryptFileWithBackupAndWipe(fileInfo, lockedDestination.DataStore, encryptionParameters, progress);
            }
        }

        public virtual void EncryptFileWithBackupAndWipe(IDataStore sourceFileInfo, IDataStore destinationFileInfo, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (sourceFileInfo == null)
            {
                throw new ArgumentNullException("sourceFileInfo");
            }
            if (destinationFileInfo == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
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
                    Encrypt(sourceFileInfo, destination, encryptionParameters, AxCryptOptions.EncryptWithCompression, progress);
                }, progress);
            }
            Wipe(sourceFileInfo, progress);
            progress.NotifyLevelFinished();
        }

        public bool Decrypt(IDataStore sourceFile, Stream destinationStream, LogOnIdentity passphrase)
        {
            if (sourceFile == null)
            {
                throw new ArgumentNullException("sourceFile");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            using (IAxCryptDocument document = Document(sourceFile, passphrase, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                Decrypt(sourceFile.OpenRead(), destinationStream, passphrase, sourceFile.FullName, new ProgressContext());
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
        public bool Decrypt(IDataStore sourceFile, IDataStore destinationFile, LogOnIdentity passphrase, AxCryptOptions options, IProgressContext progress)
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

            using (IAxCryptDocument document = Document(sourceFile, passphrase, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                Decrypt(document, destinationFile, options, progress);
            }
            return true;
        }

        public static void Decrypt(Stream source, Stream destination, LogOnIdentity passphrase, string displayContext, IProgressContext progress)
        {
            using (IAxCryptDocument document = Document(source, passphrase, displayContext, progress))
            {
                document.DecryptTo(destination);
            }
        }

        /// <summary>
        /// Decrypt from loaded AxCryptDocument to a destination file
        /// </summary>
        /// <param name="document">The loaded AxCryptDocument</param>
        /// <param name="destinationFile">The destination file</param>
        public void Decrypt(IAxCryptDocument document, IDataStore destinationFile, AxCryptOptions options, IProgressContext progress)
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
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypting to '{0}'.".InvariantFormat(destinationFile.Name));
                }

                using (Stream destinationStream = destinationFile.OpenWrite())
                {
                    document.DecryptTo(destinationStream);
                }

                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypted to '{0}'.".InvariantFormat(destinationFile.Name));
                }
            }
            catch (Exception)
            {
                if (destinationFile.IsAvailable)
                {
                    Wipe(destinationFile, progress);
                }
                throw;
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
            {
                destinationFile.SetFileTimes(document.CreationTimeUtc, document.LastAccessTimeUtc, document.LastWriteTimeUtc);
            }
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="destinationFile">The destination file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>true if the passphrase was correct</returns>
        public string Decrypt(IDataStore sourceFile, string destinationDirectory, LogOnIdentity key, AxCryptOptions options, IProgressContext progress)
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
            using (IAxCryptDocument document = Document(sourceFile, key, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return destinationFileName;
                }
                destinationFileName = document.FileName;
                IDataStore destinationFullPath = TypeMap.Resolve.New<IDataStore>(Resolve.Portable.Path().Combine(destinationDirectory, destinationFileName));
                Decrypt(document, destinationFullPath, options, progress);
            }
            return destinationFileName;
        }

        public virtual void DecryptFilesInsideFolderUniqueWithWipeOfOriginal(IDataContainer folderInfo, LogOnIdentity decryptionKey, IStatusChecker statusChecker, IProgressContext progress)
        {
            IEnumerable<IDataStore> files = folderInfo.ListEncrypted();
            Resolve.ParallelFileOperation.DoFiles(files, (file, context) =>
            {
                context.LeaveSingleThread();
                return DecryptFileUniqueWithWipeOfOriginal(file, decryptionKey, context);
            },
            (status) =>
            {
                Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.PurgeActiveFiles));
                statusChecker.CheckStatusAndShowMessage(status.Status, status.FullName);
            });
        }

        public FileOperationContext DecryptFileUniqueWithWipeOfOriginal(IDataStore fileInfo, LogOnIdentity decryptionKey, IProgressContext progress)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            using (IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFile>().Document(fileInfo, decryptionKey, progress))
            {
                if (!document.PassphraseIsValid)
                {
                    return new FileOperationContext(fileInfo.FullName, FileOperationStatus.Canceled);
                }

                IDataStore destinationStore = TypeMap.Resolve.New<IDataStore>(Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(fileInfo.FullName), document.FileName));
                using (FileLock lockedDestination = destinationStore.FullName.CreateUniqueFile())
                {
                    DecryptFile(document, lockedDestination.DataStore.FullName, progress);
                }
            }
            Wipe(fileInfo, progress);
            progress.NotifyLevelFinished();
            return new FileOperationContext(String.Empty, FileOperationStatus.Success);
        }

        public virtual void DecryptFile(IAxCryptDocument document, string decryptedFileFullName, IProgressContext progress)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (decryptedFileFullName == null)
            {
                throw new ArgumentNullException("decryptedFileFullName");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            IDataStore decryptedFileInfo = TypeMap.Resolve.New<IDataStore>(decryptedFileFullName);
            Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, progress);
        }

        /// <summary>
        /// Load an AxCryptDocument from a source file with a passphrase
        /// </summary>
        /// <param name="sourceFile">The source file</param>
        /// <param name="passphrase">The passphrase</param>
        /// <returns>An instance of AxCryptDocument. Use IsPassphraseValid property to determine validity.</returns>
        public virtual IAxCryptDocument Document(IDataStore sourceFile, LogOnIdentity key, IProgressContext progress)
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

            return Document(sourceFile.OpenRead(), key, sourceFile.FullName, progress);
        }

        /// <summary>
        /// Creates an IAxCryptDocument instance from the specified source stream.
        /// </summary>
        /// <param name="source">The source stream. Ownership is passed to the IAxCryptDocument instance which disposes the stream when it is.</param>
        /// <param name="key">The passphrase.</param>
        /// <param name="displayContext">The display context.</param>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// key
        /// or
        /// progress
        /// </exception>
        public static IAxCryptDocument Document(Stream source, LogOnIdentity logOnIdentity, string displayContext, IProgressContext progress)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (logOnIdentity == null)
            {
                throw new ArgumentNullException("logOnIdentity");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            try
            {
                IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { logOnIdentity.Passphrase }, logOnIdentity.PrivateKeys, Resolve.CryptoFactory.OrderedIds);
                IAxCryptDocument document = TypeMap.Resolve.New<AxCryptFactory>().CreateDocument(decryptionParameters, new ProgressStream(source, progress));
                return document;
            }
            catch (AxCryptException ace)
            {
                ace.DisplayContext = displayContext;
                throw;
            }
            catch (Exception ex)
            {
                AxCryptException ace = new InternalErrorException("An unhandled exception occurred.", ErrorStatus.Unknown, ex);
                ace.DisplayContext = displayContext;
                throw ace;
            }
        }

        public void WriteToFileWithBackup(IDataStore destinationFileInfo, Action<Stream> writeFileStreamTo, IProgressContext progress)
        {
            if (destinationFileInfo == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (writeFileStreamTo == null)
            {
                throw new ArgumentNullException("writeFileStreamTo");
            }

            using (FileLock lockedTemporary = MakeAlternatePath(destinationFileInfo, ".tmp"))
            {
                try
                {
                    using (Stream temporaryStream = lockedTemporary.DataStore.OpenWrite())
                    {
                        writeFileStreamTo(temporaryStream);
                    }
                }
                catch (Exception)
                {
                    if (lockedTemporary.DataStore.IsAvailable)
                    {
                        Wipe(lockedTemporary.DataStore, progress);
                    }
                    throw;
                }

                if (destinationFileInfo.IsAvailable)
                {
                    using (FileLock lockedAlternate = MakeAlternatePath(destinationFileInfo, ".bak"))
                    {
                        IDataStore backupFileInfo = TypeMap.Resolve.New<IDataStore>(destinationFileInfo.FullName);

                        backupFileInfo.MoveTo(lockedAlternate.DataStore.FullName);
                        lockedTemporary.DataStore.MoveTo(destinationFileInfo.FullName);
                        Wipe(backupFileInfo, progress);
                    }
                }
                else
                {
                    lockedTemporary.DataStore.MoveTo(destinationFileInfo.FullName);
                }
            }
        }

        private static FileLock MakeAlternatePath(IDataStore fileInfo, string extension)
        {
            string alternatePath = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(fileInfo.FullName), Resolve.Portable.Path().GetFileNameWithoutExtension(fileInfo.Name) + extension);
            return alternatePath.CreateUniqueFile();
        }

        public static string MakeAxCryptFileName(IDataItem fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            string axCryptFileName = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(fileInfo.FullName), MakeAxCryptFileName(fileInfo.Name));
            return axCryptFileName;
        }

        public static string MakeAxCryptFileName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            string axCryptExtension = OS.Current.AxCryptExtension;
            string originalExtension = Resolve.Portable.Path().GetExtension(name);
            string modifiedExtension = originalExtension.Length == 0 ? String.Empty : "-" + originalExtension.Substring(1);
            string axCryptFileName = Resolve.Portable.Path().GetFileNameWithoutExtension(name) + modifiedExtension + axCryptExtension;

            return axCryptFileName;
        }

        public virtual void Wipe(IDataStore fileInfo, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }
            if (!fileInfo.IsAvailable)
            {
                return;
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wiping '{0}'.".InvariantFormat(fileInfo.Name));
            }
            bool cancelPending = false;
            progress.NotifyLevelStart();

            string randomName;
            do
            {
                randomName = GenerateRandomFileName(fileInfo.FullName);
            } while (TypeMap.Resolve.New<IDataStore>(randomName).IsAvailable);
            IDataStore moveToFileInfo = TypeMap.Resolve.New<IDataStore>(fileInfo.FullName);
            moveToFileInfo.MoveTo(randomName);

            using (Stream stream = moveToFileInfo.OpenWrite())
            {
                long length = stream.Length + OS.Current.StreamBufferSize - stream.Length % OS.Current.StreamBufferSize;
                progress.AddTotal(length);
                for (long position = 0; position < length; position += OS.Current.StreamBufferSize)
                {
                    byte[] random = Resolve.RandomGenerator.Generate(OS.Current.StreamBufferSize);
                    stream.Write(random, 0, random.Length);
                    stream.Flush();
                    try
                    {
                        progress.AddCount(random.Length);
                    }
                    catch (OperationCanceledException)
                    {
                        cancelPending = true;
                        progress.AddCount(random.Length);
                    }
                }
            }

            moveToFileInfo.Delete();
            progress.NotifyLevelFinished();
            if (cancelPending)
            {
                throw new OperationCanceledException("Delayed cancel during wipe.");
            }
        }

        private static string GenerateRandomFileName(string originalFullName)
        {
            const string validFileNameChars = "abcdefghijklmnopqrstuvwxyz";

            string directory = Resolve.Portable.Path().GetDirectoryName(originalFullName);
            string fileName = Resolve.Portable.Path().GetFileNameWithoutExtension(originalFullName);

            int randomLength = fileName.Length < 8 ? 8 : fileName.Length;
            StringBuilder randomName = new StringBuilder(randomLength + 4);
            byte[] random = Resolve.RandomGenerator.Generate(randomLength);
            for (int i = 0; i < randomLength; ++i)
            {
                randomName.Append(validFileNameChars[random[i] % validFileNameChars.Length]);
            }
            randomName.Append(".tmp");

            return Resolve.Portable.Path().Combine(directory, randomName.ToString());
        }
    }
}