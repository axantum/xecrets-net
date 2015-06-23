#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Axantum.AxCrypt.Core.Session
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class FileSystemState : IDisposable
    {
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

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            KnownPassphrases = new List<Passphrase>();
            _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
        }

        [OnDeserialized]
        private void Finalize(StreamingContext context)
        {
            KnownPassphrases = new List<Passphrase>(KnownPassphrases);
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath;

        [DataMember(Name = "PassphraseIdentities")]
        public virtual IList<Passphrase> KnownPassphrases
        {
            get;
            private set;
        }

        private List<WatchedFolder> _watchedFolders;

        private IList<WatchedFolder> WatchedFoldersInternal
        {
            get
            {
                if (_watchedFolders == null)
                {
                    _watchedFolders = new List<WatchedFolder>();
                }
                return _watchedFolders;
            }
        }

        [DataMember(Name = "WatchedFolders")]
        public IEnumerable<WatchedFolder> WatchedFolders
        {
            get
            {
                IEnumerable<WatchedFolder> folders = WatchedFoldersInternal.Where(folder => TypeMap.Resolve.New<IDataContainer>(folder.Path).IsAvailable).ToList();
                return folders;
            }
            private set
            {
                foreach (WatchedFolder watchedFolder in value)
                {
                    AddWatchedFolderInternal(watchedFolder);
                }
            }
        }

        public virtual void AddWatchedFolder(WatchedFolder watchedFolder)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException("watchedFolder");
            }

            if (AddWatchedFolderInternal(watchedFolder))
            {
                Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderAdded, Resolve.KnownIdentities.DefaultEncryptionIdentity, watchedFolder.Path));
            }
            else
            {
                Resolve.StatusChecker.CheckStatusAndShowMessage(FileOperationStatus.FolderAlreadyWatched, watchedFolder.Path);
            }
        }

        private bool AddWatchedFolderInternal(WatchedFolder watchedFolder)
        {
            if (WatchedFoldersInternal.Contains(watchedFolder))
            {
                return false;
            }
            if (!TypeMap.Resolve.New<IDataContainer>(watchedFolder.Path).IsAvailable)
            {
                return false;
            }

            WatchedFolder copy = new WatchedFolder(watchedFolder);
            copy.Changed += watchedFolder_Changed;
            WatchedFoldersInternal.Add(copy);
            return true;
        }

        private void watchedFolder_Changed(object sender, FileWatcherEventArgs e)
        {
            WatchedFolder watchedFolder = (WatchedFolder)sender;
            IDataStore fileInfo = TypeMap.Resolve.New<IDataStore>(e.FullName);
            HandleWatchedFolderChanges(watchedFolder, fileInfo);
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderChange, fileInfo.FullName));
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

            WatchedFoldersInternal.Remove(new WatchedFolder(folderInfo.FullName, SymmetricKeyThumbprint.Zero));
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WatchedFolderRemoved, Resolve.KnownIdentities.DefaultEncryptionIdentity, folderInfo.FullName));
        }

        public event EventHandler<ActiveFileChangedEventArgs> ActiveFileChanged;

        protected virtual void OnActiveFileChanged(ActiveFileChangedEventArgs e)
        {
            EventHandler<ActiveFileChangedEventArgs> handler = ActiveFileChanged;
            if (handler != null)
            {
                handler(this, e);
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
            OnActiveFileChanged(new ActiveFileChangedEventArgs(activeFile));
        }

        public void Add(ActiveFile activeFile, ILauncher process)
        {
            Resolve.ProcessState.Add(process, activeFile);
            Add(activeFile);
        }

        public virtual void PurgeActiveFiles()
        {
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                using (FileLock fileLock = FileLock.Lock(activeFile.EncryptedFileInfo))
                {
                    if (!activeFile.EncryptedFileInfo.IsAvailable)
                    {
                        RemoveActiveFile(activeFile);
                        continue;
                    }
                    if (!activeFile.Status.HasFlag(ActiveFileStatus.Inactive))
                    {
                        continue;
                    }
                    Guid cryptoId;
                    LogOnIdentity passphrase = activeFile.EncryptedFileInfo.TryFindPassphrase(out cryptoId);
                    if (passphrase == null)
                    {
                        continue;
                    }
                    ActiveFile resurrectedActiveFile = new ActiveFile(activeFile.EncryptedFileInfo, activeFile.DecryptedFileInfo, passphrase, activeFile.Status & ~ActiveFileStatus.Inactive, cryptoId);
                    Add(resurrectedActiveFile);
                }
            }
            Save();
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
            lock (_activeFilesByEncryptedPath)
            {
                activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.Inactive);
                _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            }
            OnActiveFileChanged(new ActiveFileChangedEventArgs(activeFile));
        }

        public virtual void ChangeActiveFile(string oldFullName, string newFullName)
        {
            ActiveFile activeFile;
            lock (_activeFilesByEncryptedPath)
            {
                activeFile = FindActiveFileFromEncryptedPath(oldFullName);
                if (activeFile == null)
                {
                    return;
                }

                IDataStore newFileInfo = TypeMap.Resolve.New<IDataStore>(newFullName);
                if (!newFileInfo.IsEncrypted())
                {
                    RemoveActiveFile(activeFile);
                    return;
                }

                _activeFilesByEncryptedPath.Remove(activeFile.EncryptedFileInfo.FullName);
                activeFile = new ActiveFile(activeFile, TypeMap.Resolve.New<IDataStore>(newFullName));
                _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            }
            OnActiveFileChanged(new ActiveFileChangedEventArgs(activeFile));
        }

        private void AddInternal(ActiveFile activeFile)
        {
            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            }
            TypeMap.Resolve.Singleton<ActiveFileWatcher>().Add(activeFile.EncryptedFileInfo);
        }

        [DataMember(Name = "ActiveFiles")]
        private ICollection<ActiveFile> ActiveFilesForSerialization
        {
            get
            {
                lock (_activeFilesByEncryptedPath)
                {
                    return new ActiveFileCollection(_activeFilesByEncryptedPath.Values);
                }
            }
            set
            {
                SetRangeInternal(value, ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable);
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
        /// <param name="mode">RaiseAlways to raise Changed event for each active file, RaiseOnlyOnModified to only raise for modified active files.</param>
        /// <param name="action">A delegate with an action to take for each active file, returning the same or updated active file as need be.</param>
        public void ForEach(ChangedEventMode mode, Func<ActiveFile, ActiveFile> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            bool isAnyModified = false;
            List<ActiveFile> activeFiles = new List<ActiveFile>();
            List<ActiveFile> updatedActiveFiles = new List<ActiveFile>();
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                ActiveFile updatedActiveFile = action(activeFile);
                activeFiles.Add(updatedActiveFile);
                bool isModified = updatedActiveFile != activeFile;
                if (isModified || mode == ChangedEventMode.RaiseAlways)
                {
                    updatedActiveFiles.Add(updatedActiveFile);
                }
                isAnyModified |= isModified;
            }
            if (isAnyModified)
            {
                SetRangeInternal(activeFiles, ActiveFileStatus.None);
                Save();
            }
            foreach (ActiveFile updatedActiveFile in updatedActiveFiles)
            {
                OnActiveFileChanged(new ActiveFileChangedEventArgs(updatedActiveFile));
            }
        }

        private IDataStore _path;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The actual exception thrown by the de-serialization varies, even by platform, and the idea is to catch those and let the user continue.")]
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
            using (Stream fileSystemStateStream = path.OpenRead())
            {
                FileSystemState fileSystemState;
                try
                {
                    DataContractSerializer serializer = CreateSerializer();
                    fileSystemState = (FileSystemState)serializer.ReadObject(fileSystemStateStream);
                }
                catch (Exception ex)
                {
                    if (Resolve.Log.IsErrorEnabled)
                    {
                        Resolve.Log.LogError("Exception {1} reading {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName, ex.Message));
                    }
                    return new FileSystemState(path);
                }
                if (Resolve.Log.IsInfoEnabled)
                {
                    Resolve.Log.LogInfo("Loaded FileSystemState from '{0}'.".InvariantFormat(path));
                }
                fileSystemState._path = path;
                return fileSystemState;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It is not disposed multiple times because CloseOutput=false")]
        public virtual void Save()
        {
            lock (_activeFilesByEncryptedPath)
            {
                using (Stream fileSystemStateStream = _path.OpenWrite())
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
#if DEBUG
                        Indent = true,
#endif
                        CloseOutput = false,
                    };

                    using (XmlWriter writer = XmlWriter.Create(fileSystemStateStream, settings))
                    {
                        fileSystemStateStream.SetLength(0);
                        DataContractSerializer serializer = CreateSerializer();
                        serializer.WriteObject(writer, this);
                    }
                }
            }
            if (Resolve.Log.IsInfoEnabled)
            {
                Resolve.Log.LogInfo("Wrote FileSystemState to '{0}'.".InvariantFormat(_path));
            }
        }

        public void Delete()
        {
            _path.Delete();
        }

        private static DataContractSerializer CreateSerializer()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(FileSystemState), "FileSystemState", "http://www.axantum.com/Serialization/");
            return serializer;
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