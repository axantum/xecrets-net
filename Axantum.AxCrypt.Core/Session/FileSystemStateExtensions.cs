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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public static class FileSystemStateExtensions
    {
        /// <summary>
        /// Try do delete files that have been decrypted temporarily, if the conditions are met for such a deletion,
        /// i.e. it is apparently not locked or in use etc.
        /// </summary>
        /// <param name="fileSystemState">The instance of FileSystemState where active files are recorded.</param>
        /// <param name="progress">The context where progress may be reported.</param>
        public static void PurgeActiveFiles(this FileSystemState fileSystemState, ProgressContext progress)
        {
            progress.NotifyLevelStart();
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
                    activeFile = TryDelete(activeFile, progress);
                }
                return activeFile;
            });
            progress.NotifyLevelFinished();
        }

        /// <summary>
        /// Enumerate all files listed as active, checking for status changes and take appropriate actions such as updating status
        /// in the FileSystemState, re-encrypting or deleting temporary plaintext copies.
        /// </summary>
        /// <param name="fileSystemState">The FileSystemState to enumerate and possibly update.</param>
        /// <param name="mode">Under what circumstances is the FileSystemState.Changed event raised.</param>
        /// <param name="progress">The ProgressContext to provide visual progress feedback via.</param>
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
                    activeFile = fileSystemState.CheckActiveFileActions(activeFile, progress);
                    return activeFile;
                }
                finally
                {
                    progress.AddCount(1);
                }
            });
            progress.NotifyLevelFinished();
        }

        public static void CheckWatchedFolders(this FileSystemState fileSystemState, ProgressContext progress)
        {
            if (fileSystemState == null)
            {
                throw new ArgumentNullException("fileSystemState");
            }
            AesKey encryptionKey = fileSystemState.KnownKeys.DefaultEncryptionKey;
            if (encryptionKey == null)
            {
                return;
            }
            IEnumerable<IRuntimeFileInfo> files = fileSystemState.DecryptedFilesInWatchedFolders();
            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(files.Count());
                foreach (IRuntimeFileInfo fileInfo in files)
                {
                    IRuntimeFileInfo destinationFileInfo = fileInfo.CreateEncryptedName();
                    AxCryptFile.EncryptFileWithBackupAndWipe(fileInfo, destinationFileInfo, encryptionKey, progress);
                    progress.AddCount(1);
                }
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        /// <summary>
        /// For each active file, check if provided key matches the thumbprint of an active file that does not yet have
        /// a known key. If so, update the active file with the now known key.
        /// </summary>
        /// <param name="fileSystemState">The FileSystemState that contains the list of active files.</param>
        /// <param name="key">The newly added key to check the files for a match with.</param>
        /// <returns>True if any file was updated with the new key, False otherwise.</returns>
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

        public static void RemoveRecentFiles(this FileSystemState fileSystemState, IEnumerable<string> encryptedPaths, ProgressContext progress)
        {
            progress.NotifyLevelStart();
            progress.AddTotal(encryptedPaths.Count());
            foreach (string encryptedPath in encryptedPaths)
            {
                ActiveFile activeFile = fileSystemState.FindEncryptedPath(encryptedPath);
                if (activeFile != null)
                {
                    fileSystemState.Remove(activeFile);
                }
                progress.AddCount(1);
            }
            fileSystemState.Save();
            progress.NotifyLevelFinished();
        }

        /// <summary>
        /// Enumerate all apparently plaintext files in the list of watched folders.
        /// </summary>
        /// <param name="fileSystemState">The associated <see cref="FileSystemState"/>.</param>
        /// <returns>An enumeration of found files.</returns>
        public static IEnumerable<IRuntimeFileInfo> DecryptedFilesInWatchedFolders(this FileSystemState fileSystemState)
        {
            IEnumerable<IRuntimeFileInfo> newFiles = new List<IRuntimeFileInfo>();
            foreach (WatchedFolder watchedFolder in fileSystemState.WatchedFolders)
            {
                IRuntimeFileInfo fileInfo = OS.Current.FileInfo(watchedFolder.Path);
                newFiles = newFiles.Concat(fileInfo.Files);
            }
            return newFiles.Where((IRuntimeFileInfo fileInfo) => { return !fileInfo.Name.IsEncryptedName(); });
        }

        private static ActiveFile CheckActiveFileActions(this FileSystemState fileSystemState, ActiveFile activeFile, ProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(fileSystemState, activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = CheckIfTimeToUpdate(activeFile, progress);
            activeFile = CheckIfTimeToDelete(activeFile, progress);
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
                    }, progress);
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

        private static ActiveFile CheckIfTimeToDelete(ActiveFile activeFile, ProgressContext progress)
        {
            if (OS.Current.Platform != Platform.WindowsDesktop)
            {
                return activeFile;
            }
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }

            activeFile = TryDelete(activeFile, progress);
            return activeFile;
        }

        private static ActiveFile TryDelete(ActiveFile activeFile, ProgressContext progress)
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
                AxCryptFile.Wipe(activeFile.DecryptedFileInfo, progress);
            }
            catch (IOException)
            {
                if (OS.Log.IsWarningEnabled)
                {
                    OS.Log.LogWarning("Wiping failed for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
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