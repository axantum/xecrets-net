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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

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

        public FileOperationContext OpenAndLaunchApplication(string file, IEnumerable<Passphrase> keys, IProgressContext progress)
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

            IRuntimeFileInfo fileInfo = Factory.New<IRuntimeFileInfo>(file);
            if (!fileInfo.IsExistingFile)
            {
                if (Instance.Log.IsWarningEnabled)
                {
                    Instance.Log.LogWarning("Tried to open non-existing '{0}'.".InvariantFormat(fileInfo.FullName));
                }
                return new FileOperationContext(fileInfo.FullName, FileOperationStatus.FileDoesNotExist);
            }

            ActiveFile destinationActiveFile = _fileSystemState.FindActiveFileFromEncryptedPath(fileInfo.FullName);

            if (destinationActiveFile == null || !destinationActiveFile.DecryptedFileInfo.IsExistingFile)
            {
                IRuntimeFileInfo destinationFolderInfo = GetTemporaryDestinationFolder(destinationActiveFile);
                destinationActiveFile = TryDecrypt(fileInfo, destinationFolderInfo, keys, progress);
            }
            else
            {
                destinationActiveFile = CheckKeysForAlreadyDecryptedFile(destinationActiveFile, keys, progress);
            }

            if (destinationActiveFile == null)
            {
                return new FileOperationContext(fileInfo.FullName, FileOperationStatus.InvalidKey);
            }

            _fileSystemState.Add(destinationActiveFile);
            _fileSystemState.Save();

            FileOperationContext status = LaunchApplicationForDocument(destinationActiveFile);
            return status;
        }

        public virtual FileOperationContext OpenAndLaunchApplication(string encryptedFile, IAxCryptDocument document, IProgressContext progress)
        {
            if (encryptedFile == null)
            {
                throw new ArgumentNullException("encryptedFile");
            }
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            IRuntimeFileInfo encryptedFileInfo = Factory.New<IRuntimeFileInfo>(encryptedFile);

            ActiveFile encryptedActiveFile = _fileSystemState.FindActiveFileFromEncryptedPath(encryptedFileInfo.FullName);
            if (encryptedActiveFile == null || !encryptedActiveFile.DecryptedFileInfo.IsExistingFile)
            {
                IRuntimeFileInfo destinationFolderInfo = GetTemporaryDestinationFolder(encryptedActiveFile);
                encryptedActiveFile = DecryptActiveFileDocument(encryptedFileInfo, destinationFolderInfo, document, progress);
            }
            else
            {
                encryptedActiveFile = new ActiveFile(encryptedActiveFile, document.KeyEncryptingCrypto.Key.Passphrase);
            }

            _fileSystemState.Add(encryptedActiveFile);
            _fileSystemState.Save();

            FileOperationContext status = LaunchApplicationForDocument(encryptedActiveFile);
            return status;
        }

        private FileOperationContext LaunchApplicationForDocument(ActiveFile destinationActiveFile)
        {
            ILauncher process;
            try
            {
                if (Instance.Log.IsInfoEnabled)
                {
                    Instance.Log.LogInfo("Starting process for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                }
                process = OS.Current.Launch(destinationActiveFile.DecryptedFileInfo.FullName);
                if (process.WasStarted)
                {
                    process.Exited += new EventHandler(process_Exited);
                }
                else
                {
                    if (Instance.Log.IsInfoEnabled)
                    {
                        Instance.Log.LogInfo("Starting process for '{0}' did not start a process, assumed handled by the shell.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                    }
                }
            }
            catch (Win32Exception w32ex)
            {
                if (Instance.Log.IsErrorEnabled)
                {
                    Instance.Log.LogError("Could not launch application for '{0}', Win32Exception was '{1}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName, w32ex.Message));
                }
                return new FileOperationContext(destinationActiveFile.DecryptedFileInfo.FullName, FileOperationStatus.CannotStartApplication);
            }

            if (Instance.Log.IsWarningEnabled)
            {
                if (process.HasExited)
                {
                    Instance.Log.LogWarning("The process seems to exit immediately for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
                }
            }

            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Launched and opened '{0}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName));
            }

            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.AssumedOpenAndDecrypted);
            _fileSystemState.Add(destinationActiveFile, process);
            _fileSystemState.Save();

            return new FileOperationContext(String.Empty, FileOperationStatus.Success);
        }

        private void process_Exited(object sender, EventArgs e)
        {
            string path = ((ILauncher)sender).Path;
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("Process exit event for '{0}'.".InvariantFormat(path));
            }

            _sessionNotify.Notify(new SessionNotification(SessionNotificationType.ProcessExit, path));
        }

        private static ActiveFile TryDecrypt(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFolderInfo, IEnumerable<Passphrase> keys, IProgressContext progress)
        {
            ActiveFile destinationActiveFile = null;
            foreach (Passphrase key in keys)
            {
                if (Instance.Log.IsInfoEnabled)
                {
                    Instance.Log.LogInfo("Decrypting '{0}'".InvariantFormat(sourceFileInfo.FullName));
                }
                using (FileLock sourceLock = FileLock.Lock(sourceFileInfo))
                {
                    using (IAxCryptDocument document = Factory.New<AxCryptFile>().Document(sourceFileInfo, key, new ProgressContext()))
                    {
                        if (!document.PassphraseIsValid)
                        {
                            continue;
                        }

                        destinationActiveFile = DecryptActiveFileDocument(sourceFileInfo, destinationFolderInfo, document, progress);
                        break;
                    }
                }
            }
            return destinationActiveFile;
        }

        private static ActiveFile DecryptActiveFileDocument(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFolderInfo, IAxCryptDocument document, IProgressContext progress)
        {
            string destinationName = document.FileName;
            string destinationPath = Path.Combine(destinationFolderInfo.FullName, destinationName);

            IRuntimeFileInfo destinationFileInfo = Factory.New<IRuntimeFileInfo>(destinationPath);
            using (FileLock fileLock = FileLock.Lock(destinationFileInfo))
            {
                Factory.New<AxCryptFile>().Decrypt(document, destinationFileInfo, AxCryptOptions.SetFileTimes, progress);
            }
            ActiveFile destinationActiveFile = new ActiveFile(sourceFileInfo, destinationFileInfo, document.KeyEncryptingCrypto.Key.Passphrase, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.IgnoreChange, document.KeyEncryptingCrypto.Key.CryptoId);
            if (Instance.Log.IsInfoEnabled)
            {
                Instance.Log.LogInfo("File decrypted from '{0}' to '{1}'".InvariantFormat(sourceFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName));
            }
            return destinationActiveFile;
        }

        private static IRuntimeFileInfo GetTemporaryDestinationFolder(ActiveFile destinationActiveFile)
        {
            string destinationFolder;
            if (destinationActiveFile != null)
            {
                destinationFolder = Path.GetDirectoryName(destinationActiveFile.DecryptedFileInfo.FullName);
            }
            else
            {
                destinationFolder = Path.Combine(Factory.Instance.Singleton<WorkFolder>().FileInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.DirectorySeparatorChar);
            }
            IRuntimeFileInfo destinationFolderInfo = Factory.New<IRuntimeFileInfo>(destinationFolder);
            destinationFolderInfo.CreateFolder();
            return destinationFolderInfo;
        }

        public static string GetTemporaryDestinationName(string fileName)
        {
            string destinationFolder = Path.Combine(Factory.Instance.Singleton<WorkFolder>().FileInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.DirectorySeparatorChar);
            return Path.Combine(destinationFolder, Path.GetFileName(fileName));
        }

        private static ActiveFile CheckKeysForAlreadyDecryptedFile(ActiveFile destinationActiveFile, IEnumerable<Passphrase> keys, IProgressContext progress)
        {
            foreach (Passphrase key in keys)
            {
                using (IAxCryptDocument document = Factory.New<AxCryptFile>().Document(destinationActiveFile.EncryptedFileInfo, key, progress))
                {
                    if (document.PassphraseIsValid)
                    {
                        if (Instance.Log.IsWarningEnabled)
                        {
                            Instance.Log.LogWarning("File was already decrypted and the key was known for '{0}' to '{1}'".InvariantFormat(destinationActiveFile.EncryptedFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName));
                        }
                        return new ActiveFile(destinationActiveFile, key);
                    }
                }
            }
            return null;
        }
    }
}