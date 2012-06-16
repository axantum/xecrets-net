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
using System.IO;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileMonitor : IDisposable
    {
        private FileSystemWatcher _temporaryDirectoryWatcher;

        private FileSystemState _fileSystemState;

        private bool _disposed = false;

        public ActiveFileMonitor()
        {
            string fileSystemStateFullName = Path.Combine(TemporaryDirectoryInfo.FullName, "FileSystemState.xml");
            _fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(fileSystemStateFullName));
            _fileSystemState.Changed += new EventHandler<EventArgs>(FileSystemState_Changed);

            Watcher();
        }

        private void Watcher()
        {
            _temporaryDirectoryWatcher = new FileSystemWatcher(TemporaryDirectoryInfo.FullName);
            _temporaryDirectoryWatcher.Changed += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Created += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Deleted += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Renamed += new RenamedEventHandler(_temporaryDirectoryWatcher_Renamed);
            _temporaryDirectoryWatcher.IncludeSubdirectories = true;
            _temporaryDirectoryWatcher.Filter = String.Empty;
            _temporaryDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            _temporaryDirectoryWatcher.EnableRaisingEvents = true;
        }

        public event EventHandler<EventArgs> Changed;

        private void FileSystemState_Changed(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        public void AddActiveFile(ActiveFile activeFile)
        {
            _fileSystemState.Add(activeFile);
            _fileSystemState.Save();
            OnChanged(new EventArgs());
        }

        public void RemoveActiveFile(ActiveFile activeFile)
        {
            _fileSystemState.Remove(activeFile);
            _fileSystemState.Save();
            OnChanged(new EventArgs());
        }

        private bool _trackProcess;

        public bool TrackProcess
        {
            get
            {
                return _trackProcess;
            }
            set
            {
                _trackProcess = value;
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("ActiveFileMonitor.TrackProcess='{0}'".InvariantFormat(value));
                }
            }
        }

        private void OnChanged(EventArgs eventArgs)
        {
            EventHandler<EventArgs> changed = Changed;
            if (changed != null)
            {
                changed(null, eventArgs);
            }
        }

        public void ForEach(bool forceChange, Func<ActiveFile, ActiveFile> action)
        {
            bool isModified = false;
            List<ActiveFile> activeFiles = new List<ActiveFile>();
            foreach (ActiveFile activeFile in _fileSystemState.ActiveFiles)
            {
                ActiveFile updatedActiveFile = action(activeFile);
                activeFiles.Add(updatedActiveFile);
                if (updatedActiveFile != activeFile)
                {
                    activeFile.Dispose();
                }
                isModified |= updatedActiveFile != activeFile;
            }
            if (isModified)
            {
                _fileSystemState.ActiveFiles = activeFiles;
                _fileSystemState.Save();
            }
            if (isModified || forceChange)
            {
                OnChanged(new EventArgs());
            }
        }

        public void CheckActiveFilesStatus(ProgressContext progress)
        {
            CheckActiveFilesStatusInternal(false, progress);
        }

        public void ForceActiveFilesStatus(ProgressContext progress)
        {
            CheckActiveFilesStatusInternal(true, progress);
        }

        private void CheckActiveFilesStatusInternal(bool forceChanged, ProgressContext progress)
        {
            ForEach(forceChanged, (ActiveFile activeFile) =>
            {
                if (FileLock.IsLocked(activeFile.DecryptedFileInfo, activeFile.EncryptedFileInfo))
                {
                    return activeFile;
                }
                if (DateTime.UtcNow - activeFile.LastAccessTimeUtc <= new TimeSpan(0, 0, 5))
                {
                    return activeFile;
                }
                activeFile = CheckActiveFileActions(activeFile, progress);
                return activeFile;
            });
        }

        private ActiveFile CheckActiveFileActions(ActiveFile activeFile, ProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = CheckIfTimeToUpdate(activeFile, progress);
            activeFile = CheckIfTimeToDelete(activeFile);
            return activeFile;
        }

        private static ActiveFile CheckIfKeyIsKnown(ActiveFile activeFile)
        {
            if (!activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) && !activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete))
            {
                return activeFile;
            }
            if (activeFile.Key != null)
            {
                return activeFile;
            }
            foreach (AesKey key in KnownKeys.Keys)
            {
                if (activeFile.ThumbprintMatch(key))
                {
                    activeFile = new ActiveFile(activeFile, key);
                }
                return activeFile;
            }
            return activeFile;
        }

        private static ActiveFile CheckIfCreated(ActiveFile activeFile)
        {
            if (activeFile.Status == ActiveFileStatus.NotDecrypted)
            {
                if (!File.Exists(activeFile.DecryptedPath))
                {
                    return activeFile;
                }
                activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, activeFile.Process);
            }

            return activeFile;
        }

        private ActiveFile CheckIfProcessExited(ActiveFile activeFile)
        {
            if (activeFile.Process == null || !TrackProcess || !activeFile.Process.HasExited)
            {
                return activeFile;
            }
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedPath));
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable, null);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToUpdate(ActiveFile activeFile, ProgressContext progress)
        {
            if (activeFile.Status.HasFlag(ActiveFileStatus.NotShareable) || !activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                return activeFile;
            }
            if (activeFile.Key == null)
            {
                return activeFile;
            }
            if (!activeFile.IsModified)
            {
                return activeFile;
            }

            try
            {
                using (FileStream activeFileStream = File.Open(activeFile.DecryptedPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(activeFile.DecryptedPath);
                    AxCryptFile.WriteToFileWithBackup(activeFile.EncryptedFileInfo, (Stream destination) =>
                    {
                        AxCryptFile.Encrypt(sourceFileInfo, destination, activeFile.Key, AxCryptOptions.EncryptWithCompression, progress);
                    });
                }
            }
            catch (IOException)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Failed exclusive open modified for '{0}'.".InvariantFormat(activeFile.DecryptedPath));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable, activeFile.Process);
                return activeFile;
            }
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Wrote back '{0}' to '{1}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, DateTime.MinValue, ActiveFileStatus.AssumedOpenAndDecrypted);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToDelete(ActiveFile activeFile)
        {
            if (!AxCryptEnvironment.Current.IsDesktopWindows)
            {
                return activeFile;
            }
            if (!activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (activeFile.IsModified)
            {
                return activeFile;
            }

            activeFile = TryDelete(activeFile);
            return activeFile;
        }

        public void PurgeActiveFiles(ProgressContext progress)
        {
            ForEach(false, (ActiveFile activeFile) =>
            {
                if (FileLock.IsLocked(activeFile.DecryptedPath))
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedPath));
                    }
                }
                if (activeFile.IsModified)
                {
                    if (activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
                    {
                        activeFile = new ActiveFile(activeFile, activeFile.LastWriteTimeUtc, activeFile.Status & ~ActiveFileStatus.NotShareable);
                    }
                    activeFile = CheckIfTimeToUpdate(activeFile, progress);
                }
                if (activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) && !activeFile.IsModified)
                {
                    activeFile = TryDelete(activeFile);
                }
                return activeFile;
            });
        }

        private static ActiveFile TryDelete(ActiveFile activeFile)
        {
            if (activeFile.Process != null && !activeFile.Process.HasExited)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedPath));
                }
                return activeFile;
            }

            FileInfo activeFileInfo = new FileInfo(activeFile.DecryptedPath);

            if (activeFile.IsModified)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedPath));
                }
                return activeFile;
            }

            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Deleting '{0}'.".InvariantFormat(activeFile.DecryptedPath));
                }
                activeFileInfo.Delete();
            }
            catch (IOException)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Delete failed for '{0}'".InvariantFormat(activeFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable, activeFile.Process);
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted, null);

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Deleted '{0}' from '{1}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }

            return activeFile;
        }

        private void _temporaryDirectoryWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileSystemChanged(e.FullPath);
        }

        private void TemporaryDirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemChanged(e.FullPath);
        }

        private void FileSystemChanged(string fullPath)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Watcher says '{0}' changed.".InvariantFormat(fullPath));
            }
            OnChanged(new EventArgs());
        }

        public ActiveFile FindActiveFile(string encryptedPath)
        {
            return _fileSystemState.FindEncryptedPath(encryptedPath);
        }

        private DirectoryInfo _temporaryDirectoryInfo;

        public DirectoryInfo TemporaryDirectoryInfo
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), "AxCrypt");
                    DirectoryInfo temporaryFolderInfo = new DirectoryInfo(temporaryFolderPath);
                    temporaryFolderInfo.Create();
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }

                return _temporaryDirectoryInfo;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_temporaryDirectoryWatcher != null)
                {
                    _temporaryDirectoryWatcher.Dispose();
                }
                _temporaryDirectoryWatcher = null;
            }
            _disposed = true;
        }
    }
}