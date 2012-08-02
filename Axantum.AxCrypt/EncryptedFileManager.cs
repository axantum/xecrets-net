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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    ///
    /// </summary>
    internal class EncryptedFileManager : Component, ISupportInitialize
    {
        public event EventHandler<EventArgs> Changed;

        public event EventHandler<VersionEventArgs> VersionChecked;

        public FileSystemState FileSystemState { get; private set; }

        private IFileWatcher _fileWatcher;

        private UpdateCheck _updateCheck;

        private bool _disposed = false;

        public EncryptedFileManager()
        {
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (DesignMode)
            {
                return;
            }

            string fileSystemStateFullName = Path.Combine(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName, "FileSystemState.xml"); //MLHIDE
            FileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(fileSystemStateFullName));
            FileSystemState.Changed += new EventHandler<EventArgs>(File_Changed);

            _fileWatcher = AxCryptEnvironment.Current.FileWatcher(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName);
            _fileWatcher.FileChanged += new EventHandler<FileWatcherEventArgs>(File_Changed);

            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            _updateCheck = new UpdateCheck(myVersion, Settings.Default.AxCrypt2VersionCheckUrl, Settings.Default.UpdateUrl, FileSystemState.LastVersionCheckUtc);
            _updateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(UpdateCheck_VersionUpdate);
        }

        private void UpdateCheck_VersionUpdate(object sender, VersionEventArgs e)
        {
            EventHandler<VersionEventArgs> handler = VersionChecked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void VersionCheckInBackground()
        {
            _updateCheck.CheckInBackground();
        }

        private void File_Changed(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        public FileOperationStatus Open(string file, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            return OpenInternal(file, keys, progress);
        }

        public bool UpdateActiveFileIfKeyMatchesThumbprint(AesKey key)
        {
            bool keyMatch = false;
            FileSystemState.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFile) =>
            {
                if (activeFile.Key != null)
                {
                    return activeFile;
                }
                if (!activeFile.ThumbprintMatch(key))
                {
                    return activeFile;
                }
                keyMatch = true;

                activeFile = new ActiveFile(activeFile, key);
                return activeFile;
            });
            return keyMatch;
        }

        public void RemoveRecentFile(string encryptedPath)
        {
            ActiveFile activeFile = FileSystemState.FindEncryptedPath(encryptedPath);
            FileSystemState.Remove(activeFile);
            FileSystemState.Save();
        }

        private void OnChanged(EventArgs eventArgs)
        {
            EventHandler<EventArgs> changed = Changed;
            if (changed != null)
            {
                changed(null, eventArgs);
            }
        }

        public static void EncryptFile(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFileInfo, AesKey key, ProgressContext progress)
        {
            try
            {
                using (Stream activeFileStream = sourceFileInfo.OpenRead())
                {
                    AxCryptFile.WriteToFileWithBackup(destinationFileInfo, (Stream destination) =>
                    {
                        AxCryptFile.Encrypt(sourceFileInfo, destination, key, AxCryptOptions.EncryptWithCompression, progress);
                    });
                }
                AxCryptFile.Wipe(sourceFileInfo);
            }
            catch (IOException)
            {
            }
        }

        private FileOperationStatus OpenInternal(string file, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            IRuntimeFileInfo fileInfo = AxCryptEnvironment.Current.FileInfo(file);
            if (!fileInfo.Exists)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Tried to open non-existing '{0}'.".InvariantFormat(fileInfo.FullName)); //MLHIDE
                }
                return FileOperationStatus.FileDoesNotExist;
            }

            ActiveFile destinationActiveFile = FileSystemState.FindEncryptedPath(fileInfo.FullName);

            if (destinationActiveFile == null || !destinationActiveFile.DecryptedFileInfo.Exists)
            {
                IRuntimeFileInfo destinationFolderInfo = GetDestinationFolder(destinationActiveFile);
                destinationActiveFile = TryDecrypt(destinationFolderInfo, keys, fileInfo, progress);
            }
            else
            {
                destinationActiveFile = CheckKeysForAlreadyDecryptedFile(destinationActiveFile, keys, progress);
            }

            if (destinationActiveFile == null)
            {
                return FileOperationStatus.InvalidKey;
            }

            FileSystemState.Add(destinationActiveFile);
            FileSystemState.Save();

            return LaunchApplicationForDocument(destinationActiveFile);
        }

        private static ActiveFile TryDecrypt(IRuntimeFileInfo destinationFolderInfo, IEnumerable<AesKey> keys, IRuntimeFileInfo sourceFileInfo, ProgressContext progress)
        {
            ActiveFile destinationActiveFile = null;
            foreach (AesKey key in keys)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Decrypting '{0}'".InvariantFormat(sourceFileInfo.FullName)); //MLHIDE
                }
                using (FileLock sourceLock = FileLock.Lock(sourceFileInfo))
                {
                    using (AxCryptDocument document = AxCryptFile.Document(sourceFileInfo, key, progress))
                    {
                        if (!document.PassphraseIsValid)
                        {
                            continue;
                        }

                        string destinationName = document.DocumentHeaders.FileName;
                        string destinationPath = Path.Combine(destinationFolderInfo.FullName, destinationName);

                        IRuntimeFileInfo destinationFileInfo = AxCryptEnvironment.Current.FileInfo(destinationPath);
                        using (FileLock fileLock = FileLock.Lock(destinationFileInfo))
                        {
                            AxCryptFile.Decrypt(document, destinationFileInfo, AxCryptOptions.SetFileTimes, progress);
                        }
                        destinationActiveFile = new ActiveFile(sourceFileInfo, destinationFileInfo, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.IgnoreChange, null);
                        if (Logging.IsInfoEnabled)
                        {
                            Logging.Info("File decrypted from '{0}' to '{1}'".InvariantFormat(sourceFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
                        }
                        break;
                    }
                }
            }
            return destinationActiveFile;
        }

        private static IRuntimeFileInfo GetDestinationFolder(ActiveFile destinationActiveFile)
        {
            string destinationFolder;
            if (destinationActiveFile != null)
            {
                destinationFolder = Path.GetDirectoryName(destinationActiveFile.DecryptedFileInfo.FullName);
            }
            else
            {
                destinationFolder = Path.Combine(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + @"\"); //MLHIDE
            }
            IRuntimeFileInfo destinationFolderInfo = AxCryptEnvironment.Current.FileInfo(destinationFolder);
            destinationFolderInfo.CreateDirectory();
            return destinationFolderInfo;
        }

        private static ActiveFile CheckKeysForAlreadyDecryptedFile(ActiveFile destinationActiveFile, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            foreach (AesKey key in keys)
            {
                using (AxCryptDocument document = AxCryptFile.Document(destinationActiveFile.EncryptedFileInfo, key, progress))
                {
                    if (document.PassphraseIsValid)
                    {
                        if (Logging.IsWarningEnabled)
                        {
                            Logging.Warning("File was already decrypted and the key was known for '{0}' to '{1}'".InvariantFormat(destinationActiveFile.EncryptedFileInfo.FullName, destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
                        }
                        return new ActiveFile(destinationActiveFile, key);
                    }
                }
            }
            return null;
        }

        private FileOperationStatus LaunchApplicationForDocument(ActiveFile destinationActiveFile)
        {
            ILauncher process;
            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Starting process for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
                }
                process = AxCryptEnvironment.Current.Launch(destinationActiveFile.DecryptedFileInfo.FullName);
                if (!process.WasStarted)
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Starting process for '{0}' did not start a process, assumed handled by the shell.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
                    }
                    return FileOperationStatus.Success;
                }
                process.Exited += new EventHandler(process_Exited);
            }
            catch (Win32Exception w32ex)
            {
                if (Logging.IsErrorEnabled)
                {
                    Logging.Error("Could not launch application for '{0}', Win32Exception was '{1}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName, w32ex.Message)); //MLHIDE
                }
                return FileOperationStatus.CannotStartApplication;
            }

            if (Logging.IsWarningEnabled)
            {
                if (process.HasExited)
                {
                    Logging.Warning("The process seems to exit immediately for '{0}'".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
                }
            }

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Launched and opened '{0}'.".InvariantFormat(destinationActiveFile.DecryptedFileInfo.FullName)); //MLHIDE
            }

            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.AssumedOpenAndDecrypted, process);
            FileSystemState.Add(destinationActiveFile);
            FileSystemState.Save();

            return FileOperationStatus.Success;
        }

        private void process_Exited(object sender, EventArgs e)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Process exit event for '{0}'.".InvariantFormat(((ILauncher)sender).Path)); //MLHIDE
            }

            OnChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                }
                if (_updateCheck != null)
                {
                    _updateCheck.Dispose();
                    _updateCheck = null;
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}