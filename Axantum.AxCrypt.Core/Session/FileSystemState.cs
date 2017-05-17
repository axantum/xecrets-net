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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Session
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FileSystemState : IDisposable
    {
        [JsonConstructor]
        public FileSystemState()
        {
            Initialize(new StreamingContext());
        }

        private FileSystemState(IDataStore path)
            : this()
        {
            _path = path;
        }

        public IDataStore PathInfo
        {
            get
            {
                return _path;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            KnownPassphrases = new List<Passphrase>();
            _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
            _watchedFolders = new List<WatchedFolder>();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "context")]
        [OnDeserialized]
        private void Finalize(StreamingContext context)
        {
            KnownPassphrases = new List<Passphrase>(KnownPassphrases);
            _activeFilesForSerialization = _activeFilesForSerialization.Where(af => af.EncryptedFileInfo.IsAvailable).ToList();
            SetRangeInternal(_activeFilesForSerialization, ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable);
            _activeFilesForSerialization = null;

            foreach (WatchedFolder watchedFolder in _watchedFolders)
            {
                watchedFolder.Changed += watchedFolder_Changed;
            }
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath;

        [JsonProperty("knownPassphrases")]
        public virtual IList<Passphrase> KnownPassphrases
        {
            get;
            private set;
        }

        private List<WatchedFolder> _watchedFolders;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Json.NET")]
        [JsonProperty("watchedFolders")]
        private IList<WatchedFolder> WatchedFoldersForSerialization
        {
            get
            {
                return _watchedFolders;
            }
        }

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

        public virtual void AddWatchedFolder(WatchedFolder watchedFolder)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException("watchedFolder");
            }

            AddWatchedFolderInternal(watchedFolder);
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderAdded, Resolve.KnownIdentities.DefaultEncryptionIdentity, watchedFolder.Path));
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

        private void watchedFolder_Changed(object sender, FileWatcherEventArgs e)
        {
            WatchedFolder watchedFolder = (WatchedFolder)sender;
            foreach (string fullName in e.FullNames)
            {
                IDataStore fileInfo = New<IDataStore>(fullName);
                HandleWatchedFolderChanges(watchedFolder, fileInfo);
                Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderChange, fileInfo.FullName));
            }
        }

        private void HandleWatchedFolderChanges(WatchedFolder watchedFolder, IDataItem fileInfo)
        {
            if (watchedFolder.Path == fileInfo.FullName && !fileInfo.IsAvailable)
            {
                RemoveWatchedFolder(fileInfo);
                Save();
                return;
            }
            if (!fileInfo.IsEncrypted())
            {
                return;
            }
            if (IsExisting(fileInfo))
            {
                return;
            }
            RemoveDeletedActiveFile(fileInfo);
        }

        private static bool IsExisting(IDataItem fileInfo)
        {
            return fileInfo.Type() != FileInfoTypes.NonExisting;
        }

        private void RemoveDeletedActiveFile(IDataItem fileInfo)
        {
            ActiveFile removedActiveFile = FindActiveFileFromEncryptedPath(fileInfo.FullName);
            if (removedActiveFile != null)
            {
                RemoveActiveFile(removedActiveFile);
                Save();
            }
        }

        public virtual void RemoveWatchedFolder(IDataItem folderInfo)
        {
            if (folderInfo == null)
            {
                throw new ArgumentNullException("folderInfo");
            }

            RemoveWatchedFolderInternal(folderInfo.FullName);
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, Resolve.KnownIdentities.DefaultEncryptionIdentity, folderInfo.FullName));
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
                    _watchedFolders[i].Dispose();
                    _watchedFolders.RemoveAt(i);
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

        public virtual void UpdateActiveFiles(IEnumerable<string> fullNames)
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
                using (FileLock fileLock = FileLock.Acquire(activeFile.EncryptedFileInfo))
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
                Save();
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

        private List<ActiveFile> _activeFilesForSerialization;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Json.NET")]
        [JsonProperty("activeFiles")]
        private IList<ActiveFile> ActiveFilesForSerialization
        {
            get
            {
                lock (_activeFilesByEncryptedPath)
                {
                    return _activeFilesForSerialization = _activeFilesByEncryptedPath.Values.ToList();
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
        /// <param name="action">A delegate with an action to take for each active file, returning the same or updated active file as need be.</param>
        public void ForEach(Func<ActiveFile, ActiveFile> action)
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
                    updatedActiveFile = action(activeFile);
                }
                catch
                {
                    updatedActiveFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.Exception | ActiveFileStatus.AssumedOpenAndDecrypted);
                    AddInternal(updatedActiveFile);
                    Save();
                    throw;
                }
                activeFiles.Add(updatedActiveFile);
                if (!object.ReferenceEquals(updatedActiveFile, activeFile))
                {
                    isAnyModified = true;
                }
            }
            if (isAnyModified)
            {
                SetRangeInternal(activeFiles, ActiveFileStatus.None);
                Save();
            }
        }

        private IDataStore _path;

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

            FileSystemState fileSystemState = new FileSystemState(path);
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
                if (fileSystemState == null && Resolve.Log.IsErrorEnabled)
                {
                    Resolve.Log.LogError("Empty {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName));
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
                return new FileSystemState(path);
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Loaded FileSystemState from '{0}'.".InvariantFormat(path));
            }
            fileSystemState._path = path;
            return fileSystemState;
        }

        public virtual void Save()
        {
            lock (_activeFilesByEncryptedPath)
            {
                string json = Resolve.Serializer.Serialize(this);
                using (StreamWriter writer = new StreamWriter(_path.OpenWrite(), Encoding.UTF8))
                {
                    writer.Write(json);
                }
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wrote FileSystemState to '{0}'.".InvariantFormat(_path));
            }
            New<SessionNotify>().Notify(new SessionNotification(SessionNotificationType.ActiveFileChange));
        }

        public void Delete()
        {
            _path.Delete();
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