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
        private FileSystemState _fileSystemState;

        private IFileWatcher _fileWatcher;

        private bool _disposed = false;

        public ActiveFileMonitor(FileSystemState fileSystemState)
        {
            _fileSystemState = fileSystemState;

            _fileWatcher = AxCryptEnvironment.Current.FileWatcher(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName);
            _fileWatcher.FileChanged += new EventHandler<FileWatcherEventArgs>(_fileWatcher_FileChanged);
        }

        public event EventHandler<EventArgs> Changed;

        private void _fileWatcher_FileChanged(object sender, FileWatcherEventArgs e)
        {
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
            _fileSystemState.ForEach(forceChanged, (ActiveFile activeFile) =>
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
                if (!activeFile.DecryptedFileInfo.Exists)
                {
                    return activeFile;
                }
                activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);
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
                Logging.Info("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
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
                using (Stream activeFileStream = activeFile.DecryptedFileInfo.OpenRead())
                {
                    AxCryptFile.WriteToFileWithBackup(activeFile.EncryptedFileInfo, (Stream destination) =>
                    {
                        AxCryptFile.Encrypt(activeFile.DecryptedFileInfo, destination, activeFile.Key, AxCryptOptions.EncryptWithCompression, progress);
                    });
                }
            }
            catch (IOException)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Failed exclusive open modified for '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Wrote back '{0}' to '{1}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, activeFile.DecryptedFileInfo.LastWriteTimeUtc, ActiveFileStatus.AssumedOpenAndDecrypted);
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
            _fileSystemState.ForEach(false, (ActiveFile activeFile) =>
            {
                if (FileLock.IsLocked(activeFile.DecryptedFileInfo))
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                    }
                }
                if (activeFile.IsModified)
                {
                    if (activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
                    {
                        activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
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
                    Logging.Info("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            if (activeFile.IsModified)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Deleting '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile.DecryptedFileInfo.Delete();
            }
            catch (IOException)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Delete failed for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted, null);

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Deleted '{0}' from '{1}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }

            return activeFile;
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
                if (_fileWatcher != null)
                {
                    _fileWatcher.Dispose();
                }
                _fileWatcher = null;
            }
            _disposed = true;
        }
    }
}