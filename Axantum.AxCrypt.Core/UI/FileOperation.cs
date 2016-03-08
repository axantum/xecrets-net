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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.UI
{
    public class FileOperation
    {
        private FileSystemState _fileSystemState;
        private SessionNotify _sessionNotify;

        public FileOperation(FileSystemState fileSystemState, SessionNotify sessionNotify)
        {
            _fileSystemState = fileSystemState;
            _sessionNotify = sessionNotify;
        }

        public FileOperationContext OpenAndLaunchApplication(string encryptedFile, IEnumerable<LogOnIdentity> identities, IProgressContext progress)
        {
            if (encryptedFile == null)
            {
                throw new ArgumentNullException(nameof(encryptedFile));
            }
            if (identities == null)
            {
                throw new ArgumentNullException(nameof(identities));
            }
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            IDataStore encryptedDataStore = New<IDataStore>(encryptedFile);
            if (!encryptedDataStore.IsAvailable)
            {
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Tried to open non-existing '{0}'.".InvariantFormat(encryptedDataStore.FullName));
                }
                return new FileOperationContext(encryptedDataStore.FullName, ErrorStatus.FileDoesNotExist);
            }

            ActiveFile activeFile = _fileSystemState.FindActiveFileFromEncryptedPath(encryptedDataStore.FullName);

            if (activeFile == null || !activeFile.DecryptedFileInfo.IsAvailable)
            {
                activeFile = TryDecryptToActiveFile(encryptedDataStore, identities, progress);
            }
            else
            {
                activeFile = CheckKeysForAlreadyDecryptedFile(activeFile, identities, progress);
            }

            if (activeFile == null)
            {
                return new FileOperationContext(encryptedDataStore.FullName, ErrorStatus.InvalidKey);
            }

            using (FileLock destinationLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                if (!activeFile.DecryptedFileInfo.IsAvailable)
                {
                    activeFile = Decrypt(activeFile.Identity, activeFile.EncryptedFileInfo, activeFile, progress);
                }
                _fileSystemState.Add(activeFile);
                _fileSystemState.Save();

                FileOperationContext status = LaunchApplicationForDocument(activeFile);
                return status;
            }
        }

        public virtual FileOperationContext OpenAndLaunchApplication(LogOnIdentity identity, IDataStore encryptedDataStore, IProgressContext progress)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (encryptedDataStore == null)
            {
                throw new ArgumentNullException(nameof(encryptedDataStore));
            }
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            ActiveFile activeFile = _fileSystemState.FindActiveFileFromEncryptedPath(encryptedDataStore.FullName);
            if (activeFile == null || !activeFile.DecryptedFileInfo.IsAvailable)
            {
                activeFile = TryDecryptToActiveFile(encryptedDataStore, new LogOnIdentity[] { identity }, progress);
            }

            if (activeFile == null)
            {
                return new FileOperationContext(encryptedDataStore.FullName, ErrorStatus.InvalidKey);
            }

            using (FileLock destinationLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                if (!activeFile.DecryptedFileInfo.IsAvailable)
                {
                    activeFile = Decrypt(activeFile.Identity, activeFile.EncryptedFileInfo, activeFile, progress);
                }
                _fileSystemState.Add(activeFile);
                _fileSystemState.Save();

                FileOperationContext status = LaunchApplicationForDocument(activeFile);
                return status;
            }
        }

        private ActiveFile Decrypt(LogOnIdentity identity, IDataStore encryptedDataStore, ActiveFile activeFile, IProgressContext progress)
        {
            using (IAxCryptDocument document = New<AxCryptFile>().Document(encryptedDataStore, identity, progress))
            {
                activeFile = EnsureDecryptedFolder(identity, document, encryptedDataStore, activeFile);

                if (!activeFile.DecryptedFileInfo.IsAvailable)
                {
                    DecryptActiveFileDocument(activeFile, document, progress);
                }

                activeFile = new ActiveFile(activeFile, activeFile.DecryptedFileInfo.LastWriteTimeUtc, activeFile.Status);
            }

            return activeFile;
        }

        private static ActiveFile TryDecryptToActiveFile(IDataStore encryptedDataStore, IEnumerable<LogOnIdentity> identities, IProgressContext progress)
        {
            ActiveFile activeFile = null;
            foreach (LogOnIdentity identity in identities)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypting '{0}'".InvariantFormat(encryptedDataStore.FullName));
                }
                using (FileLock encryptedLock = FileLock.Lock(encryptedDataStore))
                {
                    using (IAxCryptDocument document = New<AxCryptFile>().Document(encryptedDataStore, identity, new ProgressContext()))
                    {
                        if (!document.PassphraseIsValid)
                        {
                            continue;
                        }

                        activeFile = DestinationActiveFileFromDocument(encryptedDataStore, activeFile?.DecryptedFileInfo?.Container, identity, document);
                        break;
                    }
                }
            }
            return activeFile;
        }

        private static ActiveFile EnsureDecryptedFolder(LogOnIdentity identity, IAxCryptDocument document, IDataStore encryptedDataStore, ActiveFile activeFile)
        {
            if (activeFile != null && activeFile.DecryptedFileInfo.IsAvailable)
            {
                activeFile = new ActiveFile(activeFile, identity);
                return activeFile;
            }
            activeFile = DestinationActiveFileFromDocument(encryptedDataStore, activeFile?.DecryptedFileInfo?.Container, identity, document);
            return activeFile;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Launching of external application can cause just about anything.")]
        private FileOperationContext LaunchApplicationForDocument(ActiveFile destinationActiveFile)
        {
            ActiveFileStatus status = ActiveFileStatus.AssumedOpenAndDecrypted;
            ILauncher process;
            try
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Starting process for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                }
                process = New<ILauncher>();
                using (FileLock decryptedFileLock = FileLock.Lock(destinationActiveFile.DecryptedFileInfo))
                {
                    process.Launch(destinationActiveFile.DecryptedFileInfo.FullName);
                }
                if (process.WasStarted)
                {
                    process.Exited += new EventHandler(process_Exited);
                }
                else
                {
                    status |= ActiveFileStatus.NoProcessKnown;
                    if (Resolve.Log.IsInfoEnabled)
                    {
                        Resolve.Log.LogInfo("Starting process for '{0}' did not start a process, assumed handled by the shell.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                    }
                }
            }
            catch (Exception ex)
            {
                if (Resolve.Log.IsErrorEnabled)
                {
                    Resolve.Log.LogError("Could not launch application for '{0}', Exception was '{1}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName, ex.Message));
                }
                return new FileOperationContext(destinationActiveFile.DecryptedFileInfo.FullName, ErrorStatus.CannotStartApplication);
            }

            if (Resolve.Log.IsWarningEnabled)
            {
                if (process.HasExited)
                {
                    Resolve.Log.LogWarning("The process seems to exit immediately for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                }
            }

            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Launched and opened '{0}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
            }

            destinationActiveFile = new ActiveFile(destinationActiveFile, status);
            _fileSystemState.Add(destinationActiveFile, process);
            _fileSystemState.Save();

            return new FileOperationContext(String.Empty, ErrorStatus.Success);
        }

        private void process_Exited(object sender, EventArgs e)
        {
            string path = ((ILauncher)sender).Path;
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Process exit event for '{0}'.".InvariantFormat(path));
            }

            _sessionNotify.Notify(new SessionNotification(SessionNotificationType.ProcessExit, path));
        }

        private static void DecryptActiveFileDocument(ActiveFile destinationActiveFile, IAxCryptDocument document, IProgressContext progress)
        {
            using (FileLock fileLock = FileLock.Lock(destinationActiveFile.DecryptedFileInfo))
            {
                New<AxCryptFile>().Decrypt(document, destinationActiveFile.DecryptedFileInfo, AxCryptOptions.SetFileTimes, progress);
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("File decrypted from '{0}' to '{1}'".InvariantFormat(destinationActiveFile.EncryptedFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName));
            }
        }

        private static ActiveFile DestinationActiveFileFromDocument(IDataStore encryptedDataStore, IDataContainer decryptedContainer, LogOnIdentity passphrase, IAxCryptDocument document)
        {
            if (decryptedContainer == null)
            {
                decryptedContainer = New<WorkFolder>().CreateTemporaryFolder();
            }

            string destinationName = document.FileName;
            string destinationPath = Resolve.Portable.Path().Combine(decryptedContainer.FullName, destinationName);

            IDataStore destinationFileInfo = New<IDataStore>(destinationPath);
            ActiveFile destinationActiveFile = new ActiveFile(encryptedDataStore, destinationFileInfo, passphrase, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.IgnoreChange, document.CryptoFactory.Id);
            return destinationActiveFile;
        }

        public static string GetTemporaryDestinationName(string fileName)
        {
            string destinationFolder = Resolve.Portable.Path().Combine(New<WorkFolder>().FileInfo.FullName, Resolve.Portable.Path().GetFileNameWithoutExtension(Resolve.Portable.Path().GetRandomFileName()) + Resolve.Portable.Path().DirectorySeparatorChar);
            return Resolve.Portable.Path().Combine(destinationFolder, Resolve.Portable.Path().GetFileName(fileName));
        }

        private static ActiveFile CheckKeysForAlreadyDecryptedFile(ActiveFile destinationActiveFile, IEnumerable<LogOnIdentity> keys, IProgressContext progress)
        {
            foreach (LogOnIdentity key in keys)
            {
                using (IAxCryptDocument document = New<AxCryptFile>().Document(destinationActiveFile.EncryptedFileInfo, key, progress))
                {
                    if (document.PassphraseIsValid)
                    {
                        if (Resolve.Log.IsWarningEnabled)
                        {
                            Resolve.Log.LogWarning("File was already decrypted and the key was known for '{0}' to '{1}'".InvariantFormat(destinationActiveFile.EncryptedFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName));
                        }
                        return new ActiveFile(destinationActiveFile, key);
                    }
                }
            }
            return null;
        }
    }
}