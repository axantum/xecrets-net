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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core
{
    /// <summary>
    /// File-centric methods for encryption and decryption.
    /// </summary>
    public class AxCryptFile
    {
        public static void Encrypt(Stream sourceStream, string sourceFileName, IDataStore destinationStore, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException("sourceStream");
            }
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationStore == null)
            {
                throw new ArgumentNullException("destinationStore");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream destinationStream = destinationStore.OpenWrite())
            {
                using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters))
                {
                    document.FileName = sourceFileName;
                    document.CreationTimeUtc = New<INow>().Utc;
                    document.LastAccessTimeUtc = document.CreationTimeUtc;
                    document.LastWriteTimeUtc = document.CreationTimeUtc;

                    document.EncryptTo(sourceStream, destinationStream, options);
                }
            }
        }

        public virtual void Encrypt(IDataStore sourceStore, IDataStore destinationStore, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (destinationStore == null)
            {
                throw new ArgumentNullException("destinationStore");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (Stream destinationStream = destinationStore.OpenWrite())
            {
                Encrypt(sourceStore, destinationStream, encryptionParameters, options, progress);
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
            {
                destinationStore.SetFileTimes(sourceStore.CreationTimeUtc, sourceStore.LastAccessTimeUtc, sourceStore.LastWriteTimeUtc);
            }
        }

        public virtual void Encrypt(IDataStore sourceFile, Stream destinationStream, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
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
                using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters))
                {
                    document.FileName = sourceFile.Name;
                    document.CreationTimeUtc = sourceFile.CreationTimeUtc;
                    document.LastAccessTimeUtc = sourceFile.LastAccessTimeUtc;
                    document.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

                    document.EncryptTo(sourceStream, destinationStream, options);
                }
            }
        }

        public static void Encrypt(Stream sourceStream, Stream destinationStream, EncryptedProperties properties, EncryptionParameters encryptionParameters, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStream == null)
            {
                throw new ArgumentNullException("sourceStream");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(encryptionParameters))
            {
                document.FileName = properties.FileName;
                document.CreationTimeUtc = properties.CreationTimeUtc;
                document.LastAccessTimeUtc = properties.LastAccessTimeUtc;
                document.LastWriteTimeUtc = properties.LastWriteTimeUtc;

                document.EncryptTo(sourceStream, destinationStream, options);
            }
        }

        public void EncryptFileWithBackupAndWipe(string sourceFileName, string destinationFileName, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            if (encryptionParameters == null)
            {
                throw new ArgumentNullException("encryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            IDataStore sourceFileInfo = New<IDataStore>(sourceFileName);
            IDataStore destinationFileInfo = New<IDataStore>(destinationFileName);
            using (FileLock destinationFileLock = FileLock.Acquire(destinationFileInfo))
            {
                EncryptFileWithBackupAndWipe(sourceFileInfo, destinationFileLock, encryptionParameters, progress);
            }
        }

        public virtual void EncryptFoldersUniqueWithBackupAndWipe(IEnumerable<IDataContainer> containers, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (containers == null)
            {
                throw new ArgumentNullException("containers");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                IEnumerable<IDataStore> files = containers.SelectMany((folder) => folder.ListEncryptable(containers, Resolve.UserSettings.FolderOperationMode));
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

        public virtual void EncryptFileUniqueWithBackupAndWipe(IDataStore sourceStore, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            IDataStore destinationFileInfo = sourceStore.CreateEncryptedName();
            using (FileLock lockedDestination = destinationFileInfo.FullName.CreateUniqueFile())
            {
                EncryptFileWithBackupAndWipe(sourceStore, lockedDestination, encryptionParameters, progress);
            }
        }

        public virtual void EncryptFileWithBackupAndWipe(IDataStore sourceStore, FileLock destinationStore, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (destinationStore == null)
            {
                throw new ArgumentNullException("destinationStore");
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
            try
            {
                using (Stream activeFileStream = sourceStore.OpenRead())
                {
                    WriteToFileWithBackup(destinationStore, (Stream destination) =>
                    {
                        Encrypt(sourceStore, destination, encryptionParameters, AxCryptOptions.EncryptWithCompression, progress);
                    }, progress);

                    if (sourceStore.IsWriteProtected)
                    {
                        destinationStore.DataStore.IsWriteProtected = true;
                    }
                }
                if (sourceStore.IsWriteProtected)
                {
                    sourceStore.IsWriteProtected = false;
                }
                Wipe(sourceStore, progress);
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        /// <summary>
        /// Changes the encryption for all encrypted files in the provided containers, using the original identity to decrypt
        /// and the provided encryption parameters for the new encryption.
        /// </summary>
        /// <remarks>
        /// If a file is already encrypted with the appropriate parameters, nothing happens.
        /// </remarks>
        /// <param name="containers">The containers.</param>
        /// <param name="identity">The identity.</param>
        /// <param name="encryptionParameters">The encryption parameters.</param>
        /// <param name="progress">The progress.</param>
        /// <exception cref="System.ArgumentNullException">
        /// containers
        /// or
        /// progress
        /// </exception>
        public virtual void ChangeEncryption(IEnumerable<IDataContainer> containers, LogOnIdentity identity, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (containers == null)
            {
                throw new ArgumentNullException("containers");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                IEnumerable<IDataStore> files = containers.SelectMany((folder) => folder.ListEncrypted());
                ChangeEncryption(files, identity, encryptionParameters, progress);
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.ActiveFileChange));
        }

        public void ChangeEncryption(IEnumerable<IDataStore> files, LogOnIdentity identity, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }
            if (files == null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            progress.AddTotal(files.Count());
            foreach (IDataStore file in files)
            {
                ChangeEncryption(file, identity, encryptionParameters, progress);
                progress.AddCount(1);
            }
        }

        /// <summary>
        /// Re-encrypt a file, using the provided original identity to decrypt and the provided encryption parameters
        /// for the new encryption. This can for example be used to change passhrase for a file, or to add or remove
        /// sharing recipients.
        /// </summary>
        /// <remarks>
        /// If the file is already encrypted with the appropriate parameters, nothing happens.
        /// </remarks>
        /// <param name="from">From.</param>
        /// <param name="encryptionParameters">The encryption parameters.</param>
        /// <param name="progress">The progress.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public void ChangeEncryption(IDataStore from, LogOnIdentity identity, EncryptionParameters encryptionParameters, IProgressContext progress)
        {
            using (CancellationTokenSource tokenSource = new CancellationTokenSource())
            {
                if (IsFileInUse(from))
                {
                    throw new FileOperationException("File is in use, cannot write to it.", from.FullName, Abstractions.ErrorStatus.FileLocked, null);
                }

                using (PipelineStream pipeline = new PipelineStream(tokenSource.Token))
                {
                    EncryptedProperties encryptedProperties = EncryptedProperties.Create(from, identity);
                    if (!EncryptionChangeNecessary(identity, encryptedProperties, encryptionParameters))
                    {
                        return;
                    }

                    using (FileLock fileLock = FileLock.Acquire(from))
                    {
                        Task decryption = Task.Run(() =>
                        {
                            Decrypt(from, pipeline, identity);
                            pipeline.Complete();
                        });
                        decryption.ContinueWith((t) => { if (t.IsFaulted) tokenSource.Cancel(); }, tokenSource.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                        Task encryption = Task.Run(() =>
                        {
                            bool isWriteProteced = from.IsWriteProtected;
                            if (isWriteProteced)
                            {
                                from.IsWriteProtected = false;
                            }
                            WriteToFileWithBackup(fileLock, (Stream s) =>
                            {
                                Encrypt(pipeline, s, encryptedProperties, encryptionParameters, AxCryptOptions.EncryptWithCompression, progress);
                            }, progress);
                            from.IsWriteProtected = isWriteProteced;
                        });
                        encryption.ContinueWith((t) => { if (t.IsFaulted) tokenSource.Cancel(); }, tokenSource.Token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

                        try
                        {
                            Task.WaitAll(decryption, encryption);
                        }
                        catch (AggregateException ae)
                        {
                            New<IReport>().Exception(ae);
                            IEnumerable<Exception> exceptions = ae.InnerExceptions.Where(ex1 => ex1.GetType() != typeof(OperationCanceledException));
                            if (!exceptions.Any())
                            {
                                return;
                            }

                            IEnumerable<Exception> axCryptExceptions = exceptions.Where(ex2 => ex2 is AxCryptException);
                            if (axCryptExceptions.Any())
                            {
                                ExceptionDispatchInfo.Capture(axCryptExceptions.First()).Throw();
                            }

                            Exception ex = exceptions.First();
                            throw new InternalErrorException(ex.Message, Abstractions.ErrorStatus.Exception, ex);
                        }
                    }
                }
            }
        }

        private static bool IsFileInUse(IDataStore destinationFileInfo)
        {
            if (destinationFileInfo.IsAvailable)
            {
                return destinationFileInfo.IsLocked();
            }
            return false;
        }

        private static bool EncryptionChangeNecessary(LogOnIdentity identity, EncryptedProperties encryptedProperties, EncryptionParameters encryptionParameters)
        {
            if (encryptedProperties.DecryptionParameter == null)
            {
                return false;
            }

            if (encryptedProperties.DecryptionParameter.Passphrase != identity.Passphrase)
            {
                return true;
            }

            if (encryptedProperties.SharedKeyHolders.Count() != encryptionParameters.PublicKeys.Count())
            {
                return true;
            }

            foreach (UserPublicKey userPublicKey in encryptionParameters.PublicKeys)
            {
                if (!encryptedProperties.SharedKeyHolders.Contains(userPublicKey))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Decrypt(IDataStore sourceStore, Stream destinationStream, LogOnIdentity passphrase)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (destinationStream == null)
            {
                throw new ArgumentNullException("destinationStream");
            }
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }

            using (IAxCryptDocument document = Document(sourceStore, passphrase, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }

                using (Stream sourceStream = sourceStore.OpenRead())
                {
                    Decrypt(sourceStream, destinationStream, passphrase, sourceStore.FullName, new ProgressContext());
                }
            }
            return true;
        }

        public virtual EncryptedProperties Decrypt(Stream encryptedStream, Stream decryptedStream, LogOnIdentity identity)
        {
            using (IAxCryptDocument document = Document(encryptedStream, identity, String.Empty, new ProgressContext()))
            {
                if (document.PassphraseIsValid)
                {
                    document.DecryptTo(decryptedStream);
                }
                return document.Properties;
            }
        }

        /// <summary>
        /// Decrypts the specified encrypted stream using the provided decryption parameters.
        /// </summary>
        /// <param name="encryptedStream">The encrypted stream.</param>
        /// <param name="decryptedStream">The decrypted stream.</param>
        /// <param name="decryptionParameters">The decryption parameters.</param>
        /// <returns></returns>
        public virtual EncryptedProperties Decrypt(Stream encryptedStream, Stream decryptedStream, IEnumerable<DecryptionParameter> decryptionParameters)
        {
            using (IAxCryptDocument document = Document(encryptedStream, decryptionParameters, String.Empty, new ProgressContext()))
            {
                if (document.PassphraseIsValid)
                {
                    document.DecryptTo(decryptedStream);
                }
                return document.Properties;
            }
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceStore">The source file</param>
        /// <param name="destinationStore">The destination file</param>
        /// <param name="logOnIdentity">The passphrase</param>
        /// <returns>true if the passphrase was correct</returns>
        public bool Decrypt(IDataStore sourceStore, IDataStore destinationStore, LogOnIdentity logOnIdentity, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (destinationStore == null)
            {
                throw new ArgumentNullException("destinationStore");
            }
            if (logOnIdentity == null)
            {
                throw new ArgumentNullException("logOnIdentity");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            using (IAxCryptDocument document = Document(sourceStore, logOnIdentity, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return false;
                }
                Decrypt(document, destinationStore, options, progress);
            }
            return true;
        }

        public static void Decrypt(Stream sourceStream, Stream destinationStream, LogOnIdentity logOnIdentity, string displayContext, IProgressContext progress)
        {
            using (IAxCryptDocument document = Document(sourceStream, logOnIdentity, displayContext, progress))
            {
                document.DecryptTo(destinationStream);
            }
        }

        /// <summary>
        /// Decrypt from loaded AxCryptDocument to a destination file
        /// </summary>
        /// <param name="document">The loaded AxCryptDocument</param>
        /// <param name="destinationStore">The destination file</param>
        /// <param name="options">The options.</param>
        /// <param name="progress">The progress.</param>
        /// <exception cref="System.ArgumentNullException">
        /// document
        /// or
        /// destinationStore
        /// or
        /// progress
        /// </exception>
        public void Decrypt(IAxCryptDocument document, IDataStore destinationStore, AxCryptOptions options, IProgressContext progress)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (destinationStore == null)
            {
                throw new ArgumentNullException("destinationStore");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            try
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypting to '{0}'.".InvariantFormat(destinationStore.Name));
                }

                using (Stream destinationStream = destinationStore.OpenWrite())
                {
                    document.DecryptTo(destinationStream);
                }

                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypted to '{0}'.".InvariantFormat(destinationStore.Name));
                }
            }
            catch (Exception)
            {
                if (destinationStore.IsAvailable)
                {
                    Wipe(destinationStore, progress);
                }
                throw;
            }
            if (options.HasMask(AxCryptOptions.SetFileTimes))
            {
                destinationStore.SetFileTimes(document.CreationTimeUtc, document.LastAccessTimeUtc, document.LastWriteTimeUtc);
            }
        }

        /// <summary>
        /// Decrypt a source file to a destination file, given a passphrase
        /// </summary>
        /// <param name="sourceStore">The source file</param>
        /// <param name="destinationContainerName">Name of the destination container.</param>
        /// <param name="logOnIdentity">The key.</param>
        /// <param name="options">The options.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>
        /// true if the passphrase was correct
        /// </returns>
        /// <exception cref="System.ArgumentNullException">sourceStore
        /// or
        /// destinationContainerName
        /// or
        /// key
        /// or
        /// progress</exception>
        public string Decrypt(IDataStore sourceStore, string destinationContainerName, LogOnIdentity logOnIdentity, AxCryptOptions options, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (destinationContainerName == null)
            {
                throw new ArgumentNullException("destinationContainerName");
            }
            if (logOnIdentity == null)
            {
                throw new ArgumentNullException("logOnIdentity");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }
            string destinationFileName = null;
            using (IAxCryptDocument document = Document(sourceStore, logOnIdentity, new ProgressContext()))
            {
                if (!document.PassphraseIsValid)
                {
                    return destinationFileName;
                }
                destinationFileName = document.FileName;
                IDataStore destinationFullPath = New<IDataStore>(Resolve.Portable.Path().Combine(destinationContainerName, destinationFileName));
                Decrypt(document, destinationFullPath, options, progress);
            }
            return destinationFileName;
        }

        public virtual async Task DecryptFilesInsideFolderUniqueWithWipeOfOriginalAsync(IDataContainer sourceContainer, LogOnIdentity logOnIdentity, IStatusChecker statusChecker, IProgressContext progress)
        {
            IEnumerable<IDataStore> files = sourceContainer.ListEncrypted().ToList();
            await Resolve.ParallelFileOperation.DoFilesAsync(files, (file, context) =>
            {
                return DecryptFileUniqueWithWipeOfOriginalAsync(file, logOnIdentity, context);
            },
            (status) =>
            {
                Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.UpdateActiveFiles));
                statusChecker.CheckStatusAndShowMessage(status.ErrorStatus, status.FullName, status.InternalMessage);
            }).Free();
        }

        public Task<FileOperationContext> DecryptFileUniqueWithWipeOfOriginalAsync(IDataStore sourceStore, LogOnIdentity logOnIdentity, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                using (IAxCryptDocument document = New<AxCryptFile>().Document(sourceStore, logOnIdentity, progress))
                {
                    if (!document.PassphraseIsValid)
                    {
                        return Task.FromResult(new FileOperationContext(sourceStore.FullName, Abstractions.ErrorStatus.Canceled));
                    }

                    IDataStore destinationStore = New<IDataStore>(Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(sourceStore.FullName), document.FileName));
                    using (FileLock lockedDestination = destinationStore.FullName.CreateUniqueFile())
                    {
                        DecryptFile(document, lockedDestination.DataStore.FullName, progress);
                    }
                }
                Wipe(sourceStore, progress);
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
            return Task.FromResult(new FileOperationContext(string.Empty, Abstractions.ErrorStatus.Success));
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

            IDataStore decryptedFileInfo = New<IDataStore>(decryptedFileFullName);
            Decrypt(document, decryptedFileInfo, AxCryptOptions.SetFileTimes, progress);
        }

        public virtual void TryDecryptBrokenFile(IAxCryptDocument document, string decryptedFileFullName, IProgressContext progress)
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

            IDataStore decryptedFileInfo = New<IDataStore>(decryptedFileFullName);

            try
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypting to '{0}'.".InvariantFormat(decryptedFileInfo.Name));
                }

                using (Stream destinationStream = decryptedFileInfo.OpenWrite())
                {
                    document.DecryptTo(destinationStream);
                }

                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypted to '{0}'.".InvariantFormat(decryptedFileInfo.Name));
                }
            }
            finally
            {
                decryptedFileInfo.SetFileTimes(document.CreationTimeUtc, document.LastAccessTimeUtc, document.LastWriteTimeUtc);
            }
        }

        /// <summary>
        /// Load an AxCryptDocument from a source file with a passphrase
        /// </summary>
        /// <param name="sourceStore">The source file</param>
        /// <param name="logOnIdentity">The log on identity.</param>
        /// <param name="progress">The progress.</param>
        /// <returns>
        /// An instance of AxCryptDocument. Use IsPassphraseValid property to determine validity.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// sourceStore
        /// or
        /// logOnIdentity
        /// or
        /// progress
        /// </exception>
        public virtual IAxCryptDocument Document(IDataStore sourceStore, LogOnIdentity logOnIdentity, IProgressContext progress)
        {
            if (sourceStore == null)
            {
                throw new ArgumentNullException("sourceStore");
            }
            if (logOnIdentity == null)
            {
                throw new ArgumentNullException("logOnIdentity");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            return Document(sourceStore.OpenRead(), logOnIdentity, sourceStore.FullName, progress);
        }

        /// <summary>
        /// Creates encrypted properties for an encrypted file.
        /// </summary>
        /// <param name="dataStore">The data store.</param>
        /// <param name="identity">The identity.</param>
        /// <returns>The EncrypedProperties, if possible.</returns>
        public virtual EncryptedProperties CreateEncryptedProperties(IDataStore dataStore, LogOnIdentity identity)
        {
            return EncryptedProperties.Create(dataStore, identity);
        }

        /// <summary>
        /// Creates an IAxCryptDocument instance from the specified source stream.
        /// </summary>
        /// <param name="source">The source stream. Ownership is passed to the IAxCryptDocument instance which disposes the stream when it is.</param>
        /// <param name="logOnIdentity">The log on identity.</param>
        /// <param name="displayContext">The display context.</param>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">source
        /// or
        /// key
        /// or
        /// progress</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "displayContext")]
        private static IAxCryptDocument Document(Stream source, LogOnIdentity logOnIdentity, string displayContext, IProgressContext progress)
        {
            if (logOnIdentity == null)
            {
                throw new ArgumentNullException("logOnIdentity");
            }

            IEnumerable<DecryptionParameter> decryptionParameters = DecryptionParameter.CreateAll(new Passphrase[] { logOnIdentity.Passphrase }, logOnIdentity.PrivateKeys, Resolve.CryptoFactory.OrderedIds);
            IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(decryptionParameters, new ProgressStream(source, progress));
            return document;
        }

        /// <summary>
        /// Decrypts the header part of specified source stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="decryptionParameters">The decryption parameters.</param>
        /// <param name="displayContext">The display context.</param>
        /// <param name="progress">The progress.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// source
        /// or
        /// decryptionParameters
        /// or
        /// progress
        /// </exception>
        private static IAxCryptDocument Document(Stream source, IEnumerable<DecryptionParameter> decryptionParameters, string displayContext, IProgressContext progress)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (decryptionParameters == null)
            {
                throw new ArgumentNullException("decryptionParameters");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            try
            {
                IAxCryptDocument document = New<AxCryptFactory>().CreateDocument(decryptionParameters, new ProgressStream(source, progress));
                return document;
            }
            catch (AxCryptException ace)
            {
                ace.DisplayContext = displayContext;
                throw;
            }
            catch (Exception ex)
            {
                AxCryptException ace = new InternalErrorException("An unhandled exception occurred.", Abstractions.ErrorStatus.Unknown, ex);
                ace.DisplayContext = displayContext;
                throw ace;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void WriteToFileWithBackup(FileLock destinationFileLock, Action<Stream> writeFileStreamTo, IProgressContext progress)
        {
            if (destinationFileLock == null)
            {
                throw new ArgumentNullException("destinationFileInfo");
            }
            if (writeFileStreamTo == null)
            {
                throw new ArgumentNullException("writeFileStreamTo");
            }

            using (FileLock lockedTemporary = MakeAlternatePath(destinationFileLock.DataStore, ".tmp"))
            {
                try
                {
                    using (Stream temporaryStream = lockedTemporary.DataStore.OpenWrite())
                    {
                        writeFileStreamTo(temporaryStream);
                    }
                }
                catch (Exception ex)
                {
                    if (lockedTemporary.DataStore.IsAvailable)
                    {
                        Wipe(lockedTemporary.DataStore, progress);
                    }
                    HandleException(ex, lockedTemporary.DataStore);
                }

                if (destinationFileLock.DataStore.IsAvailable)
                {
                    using (FileLock lockedBackup = MakeAlternatePath(destinationFileLock.DataStore, ".bak"))
                    {
                        IDataStore backupDataStore = New<IDataStore>(destinationFileLock.DataStore.FullName);
                        try
                        {
                            backupDataStore.MoveTo(lockedBackup.DataStore.FullName);
                        }
                        catch (Exception ex)
                        {
                            lockedBackup.DataStore.Delete();
                            lockedTemporary.DataStore.Delete();

                            HandleException(ex, destinationFileLock.DataStore);
                        }
                        try
                        {
                            lockedTemporary.DataStore.MoveTo(destinationFileLock.DataStore.FullName);
                        }
                        catch (Exception ex)
                        {
                            lockedTemporary.DataStore.Delete();
                            lockedBackup.DataStore.MoveTo(destinationFileLock.DataStore.FullName);

                            HandleException(ex, destinationFileLock.DataStore);
                        }
                        try
                        {
                            Wipe(backupDataStore, progress);
                        }
                        catch (Exception ex)
                        {
                            backupDataStore.Delete();

                            HandleException(ex, backupDataStore);
                        }
                    }
                    return;
                }

                try
                {
                    lockedTemporary.DataStore.MoveTo(destinationFileLock.DataStore.FullName);
                }
                catch (Exception ex)
                {
                    HandleException(ex, destinationFileLock.DataStore);
                }
            }
        }

        private static void HandleException(Exception ex, IDataStore dataStore)
        {
            if (ex is OperationCanceledException)
            {
                throw ex;
            }
            throw new FileOperationException(ex.Message, dataStore.FullName, ErrorStatus(ex), ex);
        }

        private static ErrorStatus ErrorStatus(Exception ex)
        {
            return ((ex as AxCryptException)?.ErrorStatus).GetValueOrDefault(Abstractions.ErrorStatus.Exception);
        }

        private static FileLock MakeAlternatePath(IDataStore store, string extension)
        {
            string alternatePath = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(store.FullName), Resolve.Portable.Path().GetFileNameWithoutExtension(store.Name) + extension);
            return alternatePath.CreateUniqueFile();
        }

        public static string MakeAxCryptFileName(IDataItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            string axCryptFileName = Resolve.Portable.Path().Combine(Resolve.Portable.Path().GetDirectoryName(item.FullName), MakeAxCryptFileName(item.Name));
            return axCryptFileName;
        }

        public static string MakeAxCryptFileName(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            string axCryptExtension = OS.Current.AxCryptExtension;
            string originalExtension = Resolve.Portable.Path().GetExtension(fileName);
            string modifiedExtension = originalExtension.Length == 0 ? String.Empty : "-" + originalExtension.Substring(1);
            string axCryptFileName = Resolve.Portable.Path().GetFileNameWithoutExtension(fileName) + modifiedExtension + axCryptExtension;

            return axCryptFileName;
        }

        public virtual void Wipe(IDataStore store, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (!store.IsAvailable)
            {
                return;
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wiping '{0}'.".InvariantFormat(store.Name));
            }
            progress.Cancel = false;
            bool cancelPending = false;

            progress.NotifyLevelStart();
            try
            {
                string randomName;
                do
                {
                    randomName = GenerateRandomFileName(store.FullName);
                } while (New<IDataStore>(randomName).IsAvailable);
                IDataStore moveToFileInfo = New<IDataStore>(store.FullName);
                moveToFileInfo.MoveTo(randomName);

                using (Stream stream = moveToFileInfo.OpenUpdate())
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
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
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