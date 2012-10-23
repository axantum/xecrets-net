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
using Axantum.AxCrypt.Core.Runtime;
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
                    if (OS.Log.IsInfoEnabled)
                    {
                        OS.Log.LogInfo("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                    }
                    return activeFile;
                }
                if (activeFile.IsModified)
                {
                    if (activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
                    {
                        activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
                    }
                    activeFile = CheckIfTimeToUpdate(activeFile, progress);
                }
                if (activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    activeFile = TryDelete(activeFile);
                }
                return activeFile;
            });
        }

        public static void CheckActiveFiles(this FileSystemState fileSystemState, ChangedEventMode mode, ProgressContext progress)
        {
            progress.NotifyLevelStart();
            progress.AddTotal(fileSystemState.ActiveFileCount);
            fileSystemState.ForEach(mode, (ActiveFile activeFile) =>
            {
                try
                {
                    if (FileLock.IsLocked(activeFile.DecryptedFileInfo, activeFile.EncryptedFileInfo))
                    {
                        return activeFile;
                    }
                    if (OS.Current.UtcNow - activeFile.LastActivityTimeUtc <= new TimeSpan(0, 0, 5))
                    {
                        return activeFile;
                    }
                    activeFile = CheckActiveFileActions(fileSystemState, activeFile, progress);
                    return activeFile;
                }
                finally
                {
                    progress.AddCount(1);
                }
            });
            progress.NotifyLevelFinished();
        }

        public static bool UpdateActiveFileWithKeyIfKeyMatchesThumbprint(this FileSystemState fileSystemState, AesKey key)
        {
            bool keyMatch = false;
            fileSystemState.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFile) =>
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

        public static void RemoveRecentFile(this FileSystemState fileSystemState, string encryptedPath)
        {
            ActiveFile activeFile = fileSystemState.FindEncryptedPath(encryptedPath);
            if (activeFile == null)
            {
                return;
            }
            fileSystemState.Remove(activeFile);
            fileSystemState.Save();
        }

        private static ActiveFile CheckActiveFileActions(FileSystemState fileSystemState, ActiveFile activeFile, ProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(fileSystemState, activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = CheckIfTimeToUpdate(activeFile, progress);
            activeFile = CheckIfTimeToDelete(activeFile);
            return activeFile;
        }

        private static ActiveFile CheckIfKeyIsKnown(FileSystemState fileSystemState, ActiveFile activeFile)
        {
            if ((activeFile.Status & (ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.DecryptedIsPendingDelete | ActiveFileStatus.NotDecrypted)) == 0)
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
            if (activeFile.Status != ActiveFileStatus.NotDecrypted)
            {
                return activeFile;
            }

            if (!activeFile.DecryptedFileInfo.Exists)
            {
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);

            return activeFile;
        }

        private static ActiveFile CheckIfProcessExited(ActiveFile activeFile)
        {
            if (activeFile.Process == null || !activeFile.Process.HasExited)
            {
                return activeFile;
            }
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable, null);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToUpdate(ActiveFile activeFile, ProgressContext progress)
        {
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
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
                if (OS.Log.IsWarningEnabled)
                {
                    OS.Log.LogWarning("Failed exclusive open modified for '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Wrote back '{0}' to '{1}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, activeFile.DecryptedFileInfo.LastWriteTimeUtc, ActiveFileStatus.AssumedOpenAndDecrypted);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToDelete(ActiveFile activeFile)
        {
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return activeFile;
            }
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
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
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            if (activeFile.IsModified)
            {
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            try
            {
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Deleting '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile.DecryptedFileInfo.Delete();
            }
            catch (IOException)
            {
                if (OS.Log.IsWarningEnabled)
                {
                    OS.Log.LogWarning("Delete failed for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted, null);

            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Deleted '{0}' from '{1}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }

            return activeFile;
        }
    }
}