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
    public static class FileSystemStateExtensions
    {
        public static void PurgeActiveFiles(this FileSystemState fileSystemState, ProgressContext progress)
        {
            fileSystemState.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFile) =>
            {
                if (FileLock.IsLocked(activeFile.DecryptedFileInfo))
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                    }
                    return activeFile;
                }
                if (activeFile.IsModified)
                {
                    if (activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
                    {
                        activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
                    }
                    activeFile = CheckIfTimeToUpdate(activeFile, progress);
                }
                if (activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    activeFile = TryDelete(activeFile);
                }
                return activeFile;
            });
        }

        public static void CheckActiveFiles(this FileSystemState fileSystemState, ChangedEventMode mode, bool trackProcess, ProgressContext progress)
        {
            fileSystemState.ForEach(mode, (ActiveFile activeFile) =>
            {
                if (FileLock.IsLocked(activeFile.DecryptedFileInfo, activeFile.EncryptedFileInfo))
                {
                    return activeFile;
                }
                if (AxCryptEnvironment.Current.UtcNow - activeFile.LastActivityTimeUtc <= new TimeSpan(0, 0, 5))
                {
                    return activeFile;
                }
                activeFile = CheckActiveFileActions(fileSystemState, activeFile, trackProcess, progress);
                return activeFile;
            });
        }

        private static ActiveFile CheckActiveFileActions(FileSystemState fileSystemState, ActiveFile activeFile, bool trackProcess, ProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(fileSystemState, activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile, trackProcess);
            activeFile = CheckIfTimeToUpdate(activeFile, progress);
            activeFile = CheckIfTimeToDelete(activeFile);
            return activeFile;
        }

        private static ActiveFile CheckIfKeyIsKnown(FileSystemState fileSystemState, ActiveFile activeFile)
        {
            if (!activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) && !activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete))
            {
                return activeFile;
            }
            if (activeFile.Key != null)
            {
                return activeFile;
            }
            foreach (AesKey key in fileSystemState.KnownKeys.Keys)
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

        private static ActiveFile CheckIfProcessExited(ActiveFile activeFile, bool trackProcess)
        {
            if (activeFile.Process == null || !trackProcess || !activeFile.Process.HasExited)
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

            activeFile = TryDelete(activeFile);
            return activeFile;
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
    }
}