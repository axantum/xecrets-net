﻿#region Coypright and License

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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Common;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Extensions;
using AxCrypt.Core.IO;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Session
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
                throw new ArgumentNullException(nameof(progress));
            }

            progress.NotifyLevelStart();
            try
            {
                await Resolve.FileSystemState.ForEach(async (ActiveFile activeFile) =>
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
                        using FileLock decryptedFileLock = New<FileLocker>().Acquire(activeFile.DecryptedFileInfo);
                        activeFile = await CheckIfTimeToUpdate(activeFile, encryptedFileLock, decryptedFileLock, progress).Free();
                        if (activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                        {
                            activeFile = await TryDelete(activeFile, decryptedFileLock, progress).Free();
                        }
                    }
                    return activeFile;
                }).Free();
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
                throw new ArgumentNullException(nameof(progress));
            }

            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(Resolve.FileSystemState.ActiveFileCount);
                await Resolve.FileSystemState.ForEach(async (ActiveFile activeFile) =>
                {
                    try
                    {
                        activeFile = await CheckActiveFile(activeFile, progress).Free();
                        if (activeFile.Status == ActiveFileStatus.NotDecrypted && !activeFile.EncryptedFileInfo.IsAvailable && !activeFile.EncryptedFileInfo.IsNetworkPath)
                        {
                            return null;
                        }
                        return activeFile;
                    }
                    finally
                    {
                        progress.AddCount(1);
                    }
                }).Free();
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
                return Task.FromResult((ActiveFile?)activeFile);
            }).Free();
        }

        public virtual async Task<ActiveFile> CheckActiveFile(ActiveFile activeFile, IProgressContext progress)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException(nameof(activeFile));
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
                using FileLock decryptedFileLock = New<FileLocker>().Acquire(activeFile.DecryptedFileInfo);
                using FileLock encryptedFileLock = New<FileLocker>().Acquire(activeFile.EncryptedFileInfo);
                activeFile = await CheckActiveFileActions(activeFile, encryptedFileLock, decryptedFileLock, progress).Free();
                return activeFile;
            }
            catch (Exception ex) when (ex is not AxCryptException)
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
                    return Task.FromResult<ActiveFile?>(activeFile);
                }
                if (!activeFile.ThumbprintMatch(key.Passphrase))
                {
                    return Task.FromResult<ActiveFile?>(activeFile);
                }
                keyMatch = true;

                activeFile = new ActiveFile(activeFile, key);
                return Task.FromResult<ActiveFile?>(activeFile);
            }).Free();
            return keyMatch;
        }

        public virtual async Task RemoveRecentFiles(IEnumerable<IDataStore> encryptedPaths, IProgressContext progress)
        {
            if (encryptedPaths == null)
            {
                throw new ArgumentNullException(nameof(encryptedPaths));
            }
            if (progress == null)
            {
                throw new ArgumentNullException(nameof(progress));
            }

            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(encryptedPaths.Count());
                foreach (IDataStore encryptedPath in encryptedPaths)
                {
                    ActiveFile? activeFile = Resolve.FileSystemState.FindActiveFileFromEncryptedPath(encryptedPath.FullName);
                    if (activeFile != null)
                    {
                        Resolve.FileSystemState.RemoveActiveFile(activeFile);
                    }
                    progress.AddCount(1);
                }
                await Resolve.FileSystemState.Save().Free();
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        private static async Task<ActiveFile> CheckActiveFileActions(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = await CheckIfTimeToUpdate(activeFile, encryptedFileLock, decryptedFileLock, progress).Free();
            activeFile = await CheckIfTimeToDelete(activeFile, decryptedFileLock, progress).Free();
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

        private static async Task<ActiveFile> CheckIfTimeToUpdate(ActiveFile activeFile, FileLock encryptedFileLock, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (New<KnownIdentities>().DefaultEncryptionIdentity == LogOnIdentity.Empty && activeFile.Identity == LogOnIdentity.Empty)
            {
                return activeFile;
            }

            return await activeFile.CheckUpdateDecrypted(encryptedFileLock, decryptedFileLock, progress).Free();
        }

        private static async Task<ActiveFile> CheckIfTimeToDelete(ActiveFile activeFile, FileLock decryptedFileLock, IProgressContext progress)
        {
            switch (OS.Current.Platform)
            {
                case Platform.WindowsDesktop:
                case Platform.Linux:
                case Platform.MacOsx:
                    break;
                default:
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

            activeFile = await TryDelete(activeFile, decryptedFileLock, progress).Free();
            return activeFile;
        }

        private static async Task<ActiveFile> TryDelete(ActiveFile activeFile, FileLock decryptedFileLock, IProgressContext progress)
        {
            if (Resolve.ProcessState.HasActiveProcess(activeFile))
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            if (activeFile.IsModified && (New<LicensePolicy>().Capabilities.Has(LicenseCapability.EditExistingFiles) || !New<KnownIdentities>().IsLoggedOn))
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
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

            await CleanLocalActiveFileFolderAsync(activeFile, progress).Free();

            if (activeFile.DecryptedFileInfo.Container.IsAvailable)
            {
                activeFile.DecryptedFileInfo.Container.Delete();
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);
            return activeFile;
        }

        private static async Task CleanLocalActiveFileFolderAsync(ActiveFile activeFile, IProgressContext progress)
        {
            IEnumerable<IDataStore> files = activeFile.DecryptedFileInfo.Container.Files.Where(f => f.Type() == FileInfoTypes.EncryptableFile && f.IsEncryptable);
            foreach (IDataStore file in files)
            {
                if (file.FullName == activeFile.DecryptedFileInfo.FullName)
                {
                    continue;
                }

                string destinationFilePath = New<IPath>().Combine(activeFile.EncryptedFileInfo.Container.ToString()!, file.Name.CreateEncryptedName());
                using FileLock lockedDestination = destinationFilePath.CreateUniqueFile();
                EncryptionParameters encryptionParameters = new EncryptionParameters(activeFile.Properties.CryptoId, activeFile.Identity);
                await New<AxCryptFile>().EncryptFileWithBackupAndWipeAsync(file, lockedDestination, encryptionParameters, progress).Free();
            }
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
