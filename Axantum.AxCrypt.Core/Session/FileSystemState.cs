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
using Axantum.AxCrypt.Common;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FileSystemState : IDisposable
    {
        [JsonConstructor]
        public FileSystemState()
        {
        }

        public IDataStore PathInfo
        {
            get
            {
                return _dataStore;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (WatchedFolder watchedFolder in _watchedFolders)
            {
                watchedFolder.Changed += watchedFolder_Changed;
            }
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();

        [JsonProperty("knownPassphrases")]
        public virtual IList<Passphrase> KnownPassphrases
        {
            get;
            private set;
        } = new List<Passphrase>();

        [JsonProperty("watchedFolders")]
        private List<WatchedFolder> _watchedFolders = new List<WatchedFolder>();

        public IEnumerable<WatchedFolder> WatchedFolders
        {
            get
            {
                lock (_watchedFolders)
                {
                    IEnumerable<WatchedFolder> folders = _watchedFolders.Where(folder => New<IDataContainer>(folder.Path).IsAvailable).ToList();
                    return folders;
                }
            }
        }

        public IEnumerable<WatchedFolder> AllWatchedFolders
        {
            get
            {
                lock (_watchedFolders)
                {
                    IEnumerable<WatchedFolder> folders = _watchedFolders.Where(folder => New<IDataContainer>(folder.Path).IsAvailable || folder.IsDeleted).ToList();
                    return folders;
                }
            }
        }

        public virtual async Task AddWatchedFolderAsync(WatchedFolder watchedFolder)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException("watchedFolder");
            }

            AddWatchedFolderInternal(watchedFolder);
            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.WatchedFolderAdded, Resolve.KnownIdentities.DefaultEncryptionIdentity, watchedFolder.Path));
        }

        private void AddWatchedFolderInternal(WatchedFolder watchedFolder)
        {
            lock (_watchedFolders)
            {
                AddWatchedFolderInternalUnsafe(watchedFolder);
            }
        }

        private void AddWatchedFolderInternalUnsafe(WatchedFolder watchedFolder)
        {
            RemoveWatchedFolderInternal(watchedFolder.Path);

            watchedFolder.Changed += watchedFolder_Changed;
            _watchedFolders.Add(watchedFolder);
        }

        private async void watchedFolder_Changed(object sender, FileWatcherEventArgs e)
        {
            WatchedFolder watchedFolder = (WatchedFolder)sender;
            foreach (string fullName in e.FullNames)
            {
                IDataItem dataItem = New<IDataItem>(fullName);
                await HandleWatchedFolderChangesAsync(watchedFolder, dataItem);
                await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.WatchedFolderChange, dataItem.FullName));
            }
        }

        private async Task HandleWatchedFolderChangesAsync(WatchedFolder watchedFolder, IDataItem dataItem)
        {
            if (watchedFolder.Path == dataItem.FullName && !dataItem.IsAvailable)
            {
                await RemoveWatchedFolder(dataItem);
                await Save();
                return;
            }
            if (!dataItem.IsEncrypted())
            {
                return;
            }
            if (IsExisting(dataItem))
            {
                return;
            }
            await RemoveDeletedActiveFile(dataItem);
        }

        private static bool IsExisting(IDataItem dataItem)
        {
            return dataItem.Type() != FileInfoTypes.NonExisting;
        }

        private async Task RemoveDeletedActiveFile(IDataItem dataItem)
        {
            ActiveFile removedActiveFile = FindActiveFileFromEncryptedPath(dataItem.FullName);
            if (removedActiveFile != null)
            {
                RemoveActiveFile(removedActiveFile);
                await Save();
            }
        }

        public virtual async Task RemoveWatchedFolder(IDataItem dataItem)
        {
            if (dataItem == null)
            {
                throw new ArgumentNullException("folderInfo");
            }

            RemoveWatchedFolderInternal(dataItem.FullName);
            await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, Resolve.KnownIdentities.DefaultEncryptionIdentity, dataItem.FullName));
        }

        private void RemoveWatchedFolderInternal(string path)
        {
            lock (_watchedFolders)
            {
                for (int i = 0; i < _watchedFolders.Count;)
                {
                    if (!_watchedFolders[i].Matches(path))
                    {
                        ++i;
                        continue;
                    }
                    _watchedFolders[i].IsDeleted = true;
                }
            }
        }

        public IEnumerable<ActiveFile> ActiveFiles
        {
            get
            {
                lock (_activeFilesByEncryptedPath)
                {
                    return new List<ActiveFile>(_activeFilesByEncryptedPath.Values);
                }
            }
        }

        public int ActiveFileCount
        {
            get
            {
                return _activeFilesByEncryptedPath.Count;
            }
        }

        public IList<ActiveFile> DecryptedActiveFiles
        {
            get
            {
                List<ActiveFile> activeFiles = new List<ActiveFile>();
                foreach (ActiveFile activeFile in ActiveFiles)
                {
                    if (activeFile.Status.HasMask(ActiveFileStatus.DecryptedIsPendingDelete) || activeFile.Status.HasMask(ActiveFileStatus.AssumedOpenAndDecrypted))
                    {
                        activeFiles.Add(activeFile);
                    }
                }
                return activeFiles;
            }
        }

        /// <summary>
        /// Find an active file by way of it's encrypted full path.
        /// </summary>
        /// <param name="decryptedPath">Full path to an encrypted file.</param>
        /// <returns>An ActiveFile instance, or null if not found in file system state.</returns>
        public virtual ActiveFile FindActiveFileFromEncryptedPath(string encryptedPath)
        {
            if (encryptedPath == null)
            {
                throw new ArgumentNullException("encryptedPath");
            }
            encryptedPath = encryptedPath.NormalizeFilePath();
            ActiveFile activeFile;
            lock (_activeFilesByEncryptedPath)
            {
                if (_activeFilesByEncryptedPath.TryGetValue(encryptedPath, out activeFile))
                {
                    return activeFile;
                }
            }
            return null;
        }

        /// <summary>
        /// Add a file to the volatile file system state. To persist, call Save().
        /// </summary>
        /// <param name="activeFile">The active file to save</param>
        public void Add(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            AddInternal(activeFile);
        }

        public void Add(ActiveFile activeFile, ILauncher process)
        {
            Resolve.ProcessState.Add(process, activeFile);
            Add(activeFile);
        }

        public virtual async Task UpdateActiveFiles(IEnumerable<string> fullNames)
        {
            if (fullNames == null)
            {
                throw new ArgumentNullException("fullNames");
            }

            bool dirty = false;
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                if (!fullNames.Contains(activeFile.EncryptedFileInfo.FullName))
                {
                    continue;
                }
                using (FileLock fileLock = New<FileLocker>().Acquire(activeFile.EncryptedFileInfo))
                {
                    if (!activeFile.EncryptedFileInfo.IsAvailable)
                    {
                        RemoveActiveFile(activeFile);
                        dirty = true;
                    }
                }
            }
            if (dirty)
            {
                await Save();
            }
        }

        /// <summary>
        /// Remove a file from the volatile file system state. To persist, call Save().
        /// </summary>
        /// <param name="activeFile">An active file to remove</param>
        public virtual void RemoveActiveFile(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            if (!activeFile.Status.HasFlag(ActiveFileStatus.NotDecrypted))
            {
                return;
            }

            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath.Remove(activeFile.EncryptedFileInfo.FullName);
            }
        }

        private void AddInternal(ActiveFile activeFile)
        {
            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            }
            New<ActiveFileWatcher>().Add(activeFile.EncryptedFileInfo);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Json.NET")]
        [JsonProperty("activeFiles")]
        private IList<ActiveFile> ActiveFilesForSerialization
        {
            get
            {
                lock (_activeFilesByEncryptedPath)
                {
                    return _activeFilesByEncryptedPath.Values.ToList();
                }
            }
            set
            {
                lock (_activeFilesByEncryptedPath)
                {
                    IEnumerable<ActiveFile> availableActiveFiles = value.Where(af => af.EncryptedFileInfo.IsAvailable);
                    SetRangeInternal(availableActiveFiles, ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable);
                }
            }
        }

        private void SetRangeInternal(IEnumerable<ActiveFile> activeFiles, ActiveFileStatus mask)
        {
            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath.Clear();
            }
            foreach (ActiveFile activeFile in activeFiles)
            {
                ActiveFile thisActiveFile = activeFile;
                if ((activeFile.Status & mask) != 0)
                {
                    thisActiveFile = new ActiveFile(activeFile, activeFile.Status & ~mask);
                }
                AddInternal(thisActiveFile);
            }
        }

        /// <summary>
        /// Iterate over all active files in the state.
        /// </summary>
        /// <param name="action">
        /// A delegate with an action to take for each active file, returning the same or updated active file as need be. If null is returned,
        /// the active file is removed from the list of active files.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public async Task ForEach(Func<ActiveFile, Task<ActiveFile>> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            bool isAnyModified = false;
            List<ActiveFile> activeFiles = new List<ActiveFile>();
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                ActiveFile updatedActiveFile;
                try
                {
                    updatedActiveFile = await action(activeFile);
                }
                catch
                {
                    updatedActiveFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.Exception | ActiveFileStatus.AssumedOpenAndDecrypted);
                    AddInternal(updatedActiveFile);
                    await Save();
                    throw;
                }
                if (updatedActiveFile != null)
                {
                    activeFiles.Add(updatedActiveFile);
                }
                if (!ReferenceEquals(updatedActiveFile, activeFile))
                {
                    isAnyModified = true;
                }
            }
            if (isAnyModified)
            {
                SetRangeInternal(activeFiles, ActiveFileStatus.None);
                await Save();
            }
        }

        private IDataStore _dataStore;

        public static FileSystemState Create(IDataStore path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.IsAvailable)
            {
                return CreateFileSystemState(path);
            }

            FileSystemState fileSystemState = new FileSystemState()
            {
                _dataStore = path,
            };
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("No existing FileSystemState. Save location is '{0}'.".InvariantFormat(path.FullName));
            }
            return fileSystemState;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If the state can't be read, the software is rendered useless, so it's better to revert to empty here.")]
        private static FileSystemState CreateFileSystemState(IDataStore path)
        {
            FileSystemState fileSystemState = null;
            try
            {
                fileSystemState = Resolve.Serializer.Deserialize<FileSystemState>(path);
                if (fileSystemState == null)
                {
                    if (Resolve.Log.IsErrorEnabled)
                    {
                        Resolve.Log.LogError("Empty {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName));
                    }
                }
                else
                {
                    if (Resolve.Log.IsInfoEnabled)
                    {
                        Resolve.Log.LogInfo("Loaded FileSystemState from '{0}'.".InvariantFormat(path));
                    }
                }
            }
            catch (Exception ex)
            {
                New<IReport>().Exception(ex);
                if (Resolve.Log.IsErrorEnabled)
                {
                    Resolve.Log.LogError("Exception {1} reading {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName, ex.Message));
                }
            }
            if (fileSystemState == null)
            {
                fileSystemState = new FileSystemState();
            }
            fileSystemState._dataStore = path;
            return fileSystemState;
        }

        public virtual async Task Save()
        {
            lock (_activeFilesByEncryptedPath)
            {
                string currentJson = string.Empty;
                if (_dataStore.IsAvailable)
                {
                    using (StreamReader reader = new StreamReader(_dataStore.OpenRead(), Encoding.UTF8))
                    {
                        currentJson = reader.ReadToEnd();
                    }
                }

                string updatedJson = Resolve.Serializer.Serialize(this);
                if (currentJson == updatedJson)
                {
                    return;
                }

                using (StreamWriter writer = new StreamWriter(_dataStore.OpenWrite(), Encoding.UTF8))
                {
                    writer.Write(updatedJson);
                }
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wrote FileSystemState to '{0}'.".InvariantFormat(_dataStore));
            }
            await New<SessionNotify>().NotifyAsync(new SessionNotification(SessionNotificationType.ActiveFileChange)).Free();
        }

        public void Delete()
        {
            _dataStore.Delete();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            if (_watchedFolders != null)
            {
                foreach (WatchedFolder watchedFolder in _watchedFolders)
                {
                    watchedFolder.Dispose();
                }
                _watchedFolders = null;
            }
        }

        #endregion IDisposable Members
    }
}