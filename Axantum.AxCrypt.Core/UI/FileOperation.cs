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

        public FileOperationContext OpenAndLaunchApplication(string file, IEnumerable<LogOnIdentity> keys, IProgressContext progress)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            IDataStore fileInfo = New<IDataStore>(file);
            if (!fileInfo.IsAvailable)
            {
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Tried to open non-existing '{0}'.".InvariantFormat(fileInfo.FullName));
                }
                return new FileOperationContext(fileInfo.FullName, ErrorStatus.FileDoesNotExist);
            }

            ActiveFile destinationActiveFile = _fileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);

            if (destinationActiveFile == null || !destinationActiveFile.DecryptedFileInfo.IsAvailable)
            {
                IDataContainer destinationFolderInfo = GetTemporaryDestinationFolder(destinationActiveFile);
                destinationActiveFile = TryDecrypt(fileInfo, destinationFolderInfo, keys, progress);
            }
            else
            {
                destinationActiveFile = CheckKeysForAlreadyDecryptedFile(destinationActiveFile, keys, progress);
            }

            if (destinationActiveFile == null)
            {
                return new FileOperationContext(fileInfo.FullName, ErrorStatus.InvalidKey);
            }

            _fileSystemState.Add(destinationActiveFile);
            _fileSystemState.Save();

            FileOperationContext status = LaunchApplicationForDocument(destinationActiveFile);
            return status;
        }

        public virtual FileOperationContext OpenAndLaunchApplication(string encryptedFile, LogOnIdentity passphrase, IDataStore axCryptDataStore, IProgressContext progress)
        {
            if (encryptedFile == null)
            {
                throw new ArgumentNullException("encryptedFile");
            }
            if (passphrase == null)
            {
                throw new ArgumentNullException("passphrase");
            }
            if (axCryptDataStore == null)
            {
                throw new ArgumentNullException("axCryptDataStore");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            IDataStore encryptedFileInfo = New<IDataStore>(encryptedFile);

            ActiveFile encryptedActiveFile = _fileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            using (IAxCryptDocument document = New<AxCryptFile>().Document(axCryptDataStore, passphrase, progress))
            {
                encryptedActiveFile = EnsureDecryptedFolder(passphrase, document, encryptedFileInfo, encryptedActiveFile);
                _fileSystemState.Add(encryptedActiveFile);
                _fileSystemState.Save();

                if (!encryptedActiveFile.DecryptedFileInfo.IsAvailable)
                {
                    DecryptActiveFileDocument(encryptedActiveFile, document, progress);
                }
            }
            return LaunchApplicationForDocument(encryptedActiveFile);
        }

        private static ActiveFile EnsureDecryptedFolder(LogOnIdentity passphrase, IAxCryptDocument document, IDataStore encryptedFileInfo, ActiveFile encryptedActiveFile)
        {
            if (encryptedActiveFile != null && encryptedActiveFile.DecryptedFileInfo.IsAvailable)
            {
                encryptedActiveFile = new ActiveFile(encryptedActiveFile, passphrase);
                return encryptedActiveFile;
            }
            IDataContainer destinationFolderInfo = GetTemporaryDestinationFolder(encryptedActiveFile);
            encryptedActiveFile = DestinationFileInfoFromDocument(encryptedFileInfo, destinationFolderInfo, passphrase, document);
            return encryptedActiveFile;
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

        private static ActiveFile TryDecrypt(IDataStore sourceDataStore, IDataContainer destinationContainer, IEnumerable<LogOnIdentity> passphrases, IProgressContext progress)
        {
            ActiveFile destinationActiveFile = null;
            foreach (LogOnIdentity passphrase in passphrases)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Decrypting '{0}'".InvariantFormat(sourceDataStore.FullName));
                }
                using (FileLock sourceLock = FileLock.Lock(sourceDataStore))
                {
                    using (IAxCryptDocument document = New<AxCryptFile>().Document(sourceDataStore, passphrase, new ProgressContext()))
                    {
                        if (!document.PassphraseIsValid)
                        {
                            continue;
                        }

                        destinationActiveFile = DestinationFileInfoFromDocument(sourceDataStore, destinationContainer, passphrase, document);
                        DecryptActiveFileDocument(destinationActiveFile, document, progress);
                        break;
                    }
                }
            }
            return destinationActiveFile;
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

        private static ActiveFile DestinationFileInfoFromDocument(IDataStore sourceFileInfo, IDataContainer destinationFolderInfo, LogOnIdentity passphrase, IAxCryptDocument document)
        {
            string destinationName = document.FileName;
            string destinationPath = Resolve.Portable.Path().Combine(destinationFolderInfo.FullName, destinationName);

            IDataStore destinationFileInfo = New<IDataStore>(destinationPath);
            ActiveFile destinationActiveFile = new ActiveFile(sourceFileInfo, destinationFileInfo, passphrase, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.IgnoreChange, document.CryptoFactory.Id);
            return destinationActiveFile;
        }

        private static IDataContainer GetTemporaryDestinationFolder(ActiveFile destinationActiveFile)
        {
            if (destinationActiveFile != null)
            {
                return destinationActiveFile.DecryptedFileInfo.Container;
            }
            return New<WorkFolder>().CreateTemporaryFolder();
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