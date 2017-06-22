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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileAction
    {
        public ActiveFileAction()
        {
        }

        /// <summary>
        /// Try do delete files that have been decrypted temporarily, if the conditions are met for such a deletion,
        /// i.e. it is apparently not locked or in use etc.
        /// </summary>
        /// <param name="_fileSystemState">The instance of FileSystemState where active files are recorded.</param>
        /// <param name="progress">The context where progress may be reported.</param>
        public virtual async Task PurgeActiveFiles(IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                await Resolve.FileSystemState.ForEach((ActiveFile activeFile) =>
                {
                    if (activeFile.Status.HasMask(ActiveFileStatus.Exception))
                    {
                        return activeFile;
                    }
                    if (New<FileLocker>().IsLocked(activeFile.DecryptedFileInfo))
                    {
                        if (Resolve.Log.IsInfoEnabled)
                        {
                            Resolve.Log.LogInfo("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                        }
                        return activeFile;
                    }
                    if (activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
                    {
                        activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
                    }
                    using (FileLock encryptedFileLock = New<FileLocker>().Acquire(activeFile.EncryptedFileInfo))
                    {
                        using (FileLock decryptedFileLock = New<FileLocker>().Acquire(activeFile.DecryptedFileInfo))
                        {
                            activeFile = CheckIfTimeToUpdate(activeFile, encryptedFileLock, decryptedFileLock, progress);
                            if (activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                            {
                                activeFile = TryDelete(activeFile, encryptedFileLock, decryptedFileLock, progress);
                            }
                        }
                    }
                    return activeFile;
                });
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        /// <summary>
        /// Enumerate all files listed as active, checking for status changes and take appropriate actions such as updating status
        /// in the FileSystemState, re-encrypting or deleting temporary plaintext copies.
        /// </summary>
        /// <param name="_fileSystemState">The FileSystemState to enumerate and possibly update.</param>
        /// <param name="mode">Under what circumstances is the FileSystemState.Changed event raised.</param>
        /// <param name="progress">The ProgressContext to provide visual progress feedback via.</param>
        public virtual async Task CheckActiveFiles(IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(Resolve.FileSystemState.ActiveFileCount);
                await Resolve.FileSystemState.ForEach((ActiveFile activeFile) =>
                {
                    try
                    {
                        return CheckActiveFile(activeFile, progress);
                    }
                    finally
                    {
                        progress.AddCount(1);
                    }
                });
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        public virtual async Task ClearExceptionState()
        {
            await Resolve.FileSystemState.ForEach((ActiveFile activeFile) =>
            {
                if (activeFile.Status.HasFlag(ActiveFileStatus.Exception))
                {
                    activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.Exception);
                }
                return activeFile;
            });
        }

        public virtual ActiveFile CheckActiveFile(ActiveFile activeFile, IProgressContext progress)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }

            try
            {
                if (activeFile.Status.HasMask(ActiveFileStatus.Exception))
                {
                    return activeFile;
                }

                if (New<FileLocker>().IsLocked(activeFile.DecryptedFileInfo, activeFile.EncryptedFileInfo))
                {
                    return activeFile;
                }
                using (FileLock decryptedFileLock = New<FileLocker>().Acquire(activeFile.DecryptedFileInfo))
                {
                    using (FileLock encryptedFileLock = New<FileLocker>().Acquire(activeFile.EncryptedFileInfo))
                    {
                        activeFile = CheckActiveFileActions(activeFile, encryptedFileLock, decryptedFileLock, progress);
                        return activeFile;
                    }
                }
            }
            catch (Exception ex) when (!(ex is AxCryptException))
            {
                throw new FileOperationException("Unexpected exception checking active files.", $"{activeFile.EncryptedFileInfo.FullName} : {activeFile.DecryptedFileInfo.FullName}", ErrorStatus.Exception, ex);
            }
        }

        /// <summary>
        /// For each active file, check if provided key matches the thumbprint of an active file that does not yet have
        /// a known key. If so, update the active file with the now known key.
        /// </summary>
        /// <param name="_fileSystemState">The FileSystemState that contains the list of active files.</param>
        /// <param name="key">The newly added key to check the files for a match with.</param>
        /// <returns>True if any file was updated with the new key, False otherwise.</returns>
        public virtual async Task<bool> UpdateActiveFileWithKeyIfKeyMatchesThumbprint(LogOnIdentity key)
        {
            bool keyMatch = false;
            await Resolve.FileSystemState.ForEach((ActiveFile activeFile) =>
            {
                if (activeFile.Identity != LogOnIdentity.Empty)
                {
                    return activeFile;
                }
                if (!activeFile.ThumbprintMatch(key.Passphrase))
                {
                    return activeFile;
                }
                keyMatch = true;

                activeFile = new ActiveFile(activeFile, key);
                return activeFile;
            });
            return keyMatch;
        }

        public virtual async Task RemoveRecentFiles(IEnumerable<IDataStore> encryptedPaths, IProgressContext progress)
        {
            if (encryptedPaths == null)
            {
                throw new ArgumentNullException("encryptedPaths");
            }
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(encryptedPaths.Count());
                foreach (IDataStore encryptedPath in encryptedPaths)
                {
                    ActiveFile activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedPath.FullName);
                    if (activeFile != null)
                    {
                        Resolve.FileSystemState.RemoveActiveFile(activeFile);
                    }
                    progress.AddCount(1);
                }
                await Resolve.FileSystemState.Save();
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        private static ActiveFile CheckActiveFileActions(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = CheckIfTimeToUpdate(activeFile, encryptedFileLock, decryptedFileLock, progress);
            activeFile = CheckIfTimeToDelete(activeFile, encryptedFileLock, decryptedFileLock, progress);
            return activeFile;
        }

        private static ActiveFile CheckIfKeyIsKnown(ActiveFile activeFile)
        {
            if ((activeFile.Status & (ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.DecryptedIsPendingDelete | ActiveFileStatus.NotDecrypted)) == 0)
            {
                return activeFile;
            }

            LogOnIdentity key = FindKnownKeyOrEmpty(activeFile);
            if (activeFile.Identity != LogOnIdentity.Empty)
            {
                if (key != LogOnIdentity.Empty)
                {
                    return activeFile;
                }
                return new ActiveFile(activeFile);
            }

            if (key != LogOnIdentity.Empty)
            {
                return new ActiveFile(activeFile, key);
            }
            return activeFile;
        }

        private static LogOnIdentity FindKnownKeyOrEmpty(ActiveFile activeFile)
        {
            foreach (LogOnIdentity key in Resolve.KnownIdentities.Identities)
            {
                if (activeFile.ThumbprintMatch(key.Passphrase))
                {
                    return key;
                }
            }
            return LogOnIdentity.Empty;
        }

        private static ActiveFile CheckIfCreated(ActiveFile activeFile)
        {
            if (activeFile.Status != ActiveFileStatus.NotDecrypted)
            {
                return activeFile;
            }

            if (!activeFile.DecryptedFileInfo.IsAvailable)
            {
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted);

            return activeFile;
        }

        private static ActiveFile CheckIfProcessExited(ActiveFile activeFile)
        {
            if (Resolve.ProcessState.HasActiveProcess(activeFile))
            {
                return activeFile;
            }
            if (!activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (activeFile.DecryptedFileInfo.IsLocked())
            {
                return activeFile;
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToUpdate(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (activeFile.Identity == LogOnIdentity.Empty)
            {
                return activeFile;
            }

            return activeFile.CheckUpdateDecrypted(encryptedFileLock, decryptedFileLock, progress);
        }

        private static ActiveFile CheckIfTimeToDelete(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (OS.Current.Platform != Platform.WindowsDesktop &&
                OS.Current.Platform != Platform.Linux)
            {
                return activeFile;
            }
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
            {
                return activeFile;
            }
            if (activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (activeFile.Status.HasMask(ActiveFileStatus.NoProcessKnown))
            {
                return activeFile;
            }
            if (Resolve.ProcessState.HasActiveProcess(activeFile))
            {
                return activeFile;
            }

            activeFile = TryDelete(activeFile, encryptedFileLock, decryptedFileLock, progress);
            return activeFile;
        }

        private static ActiveFile TryDelete(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (Resolve.ProcessState.HasActiveProcess(activeFile))
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            if (activeFile.IsModified && New<LicensePolicy>().Capabilities.Has(LicenseCapability.EditExistingFiles))
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }
            if (activeFile.Identity == LogOnIdentity.Empty)
            {
                return activeFile;
            }
            try
            {
                WipeFile(decryptedFileLock, progress);
            }
            catch (IOException ioex)
            {
                New<IReport>().Exception(ioex);
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Wiping failed for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }

            if (activeFile.DecryptedFileInfo.Container.IsAvailable)
            {
                activeFile.DecryptedFileInfo.Container.Delete();
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            return activeFile;
        }

        private static void WipeFile(FileLock fileLock, IProgressContext progress)
        {
            if (!fileLock.DataStore.IsAvailable)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Found '{0}' to be already deleted.".InvariantFormat(fileLock.DataStore.FullName));
                }
                return;
            }

            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wiping '{0}'.".InvariantFormat(fileLock.DataStore.FullName));
            }

            if (fileLock.DataStore.IsWriteProtected)
            {
                fileLock.DataStore.IsWriteProtected = false;
            }
            New<AxCryptFile>().Wipe(fileLock, progress);

            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wiped '{0}'.".InvariantFormat(fileLock.DataStore.FullName));
            }
        }
    }
}