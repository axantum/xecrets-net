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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public virtual void PurgeActiveFiles(IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                Resolve.FileSystemState.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFile) =>
                {
                    if (activeFile.Status.HasMask(ActiveFileStatus.Exception))
                    {
                        return activeFile;
                    }
                    if (FileLock.IsLocked(activeFile.DecryptedFileInfo))
                    {
                        if (Resolve.Log.IsInfoEnabled)
                        {
                            Resolve.Log.LogInfo("Not deleting '{0}' because it is marked as locked.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
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
        public virtual void CheckActiveFiles(ChangedEventMode mode, IProgressContext progress)
        {
            if (progress == null)
            {
                throw new ArgumentNullException("progress");
            }

            progress.NotifyLevelStart();
            try
            {
                progress.AddTotal(Resolve.FileSystemState.ActiveFileCount);
                Resolve.FileSystemState.ForEach(mode, (ActiveFile activeFile) =>
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

        public virtual void ClearExceptionState()
        {
            Resolve.FileSystemState.ForEach(ChangedEventMode.RaiseNever, (ActiveFile activeFile) =>
            {
                return new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.Exception);
            });
        }

        public virtual ActiveFile CheckActiveFile(ActiveFile activeFile, IProgressContext progress)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }

            if (activeFile.Status.HasMask(ActiveFileStatus.Exception))
            {
                return activeFile;
            }

            if (FileLock.IsLocked(activeFile.DecryptedFileInfo, activeFile.EncryptedFileInfo))
            {
                return activeFile;
            }

            using (FileLock encryptedFileLock = FileLock.Lock(activeFile.DecryptedFileInfo))
            {
                using (FileLock decryptedFileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
                {
                    activeFile = CheckActiveFileActions(activeFile, progress);
                    return activeFile;
                }
            }
        }

        /// <summary>
        /// For each active file, check if provided key matches the thumbprint of an active file that does not yet have
        /// a known key. If so, update the active file with the now known key.
        /// </summary>
        /// <param name="_fileSystemState">The FileSystemState that contains the list of active files.</param>
        /// <param name="key">The newly added key to check the files for a match with.</param>
        /// <returns>True if any file was updated with the new key, False otherwise.</returns>
        public virtual bool UpdateActiveFileWithKeyIfKeyMatchesThumbprint(LogOnIdentity key)
        {
            bool keyMatch = false;
            Resolve.FileSystemState.ForEach(ChangedEventMode.RaiseOnlyOnModified, (ActiveFile activeFile) =>
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

        public virtual void RemoveRecentFiles(IEnumerable<IDataStore> encryptedPaths, IProgressContext progress)
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
                Resolve.FileSystemState.Save();
            }
            finally
            {
                progress.NotifyLevelFinished();
            }
        }

        private static ActiveFile CheckActiveFileActions(ActiveFile activeFile, IProgressContext progress)
        {
            activeFile = CheckIfKeyIsKnown(activeFile);
            activeFile = CheckIfCreated(activeFile);
            activeFile = CheckIfProcessExited(activeFile);
            activeFile = CheckIfTimeToUpdate(activeFile, progress);
            activeFile = CheckIfTimeToDelete(activeFile, progress);
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
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status & ~ActiveFileStatus.NotShareable);
            return activeFile;
        }

        private static ActiveFile CheckIfTimeToUpdate(ActiveFile activeFile, IProgressContext progress)
        {
            if (!activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasMask(ActiveFileStatus.NotShareable))
            {
                return activeFile;
            }
            if (activeFile.Identity == LogOnIdentity.Empty)
            {
                return activeFile;
            }
            if (!activeFile.IsModified)
            {
                return activeFile;
            }

            return activeFile.UpdateDecrypted(progress);
        }

        private static ActiveFile CheckIfTimeToDelete(ActiveFile activeFile, IProgressContext progress)
        {
            if (OS.Current.Platform != Platform.WindowsDesktop)
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

            activeFile = TryDelete(activeFile, progress);
            return activeFile;
        }

        private static ActiveFile TryDelete(ActiveFile activeFile, IProgressContext progress)
        {
            if (Resolve.ProcessState.HasActiveProcess(activeFile))
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Not deleting '{0}' because it has an active process.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            if (activeFile.IsModified)
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                return activeFile;
            }

            try
            {
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Deleting '{0}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                if (activeFile.DecryptedFileInfo.IsWriteProtected)
                {
                    activeFile.DecryptedFileInfo.IsWriteProtected = false;
                }
                New<AxCryptFile>().Wipe(activeFile.DecryptedFileInfo, progress);
                if (activeFile.DecryptedFileInfo.Container.IsAvailable)
                {
                    activeFile.DecryptedFileInfo.Container.Delete();
                }
            }
            catch (IOException)
            {
                if (Resolve.Log.IsWarningEnabled)
                {
                    Resolve.Log.LogWarning("Wiping failed for '{0}'".InvariantFormat(activeFile.DecryptedFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable);
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile, ActiveFileStatus.NotDecrypted);

            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Deleted '{0}' from '{1}'.".InvariantFormat(activeFile.DecryptedFileInfo.FullName, activeFile.EncryptedFileInfo.FullName));
            }

            return activeFile;
        }
    }
}