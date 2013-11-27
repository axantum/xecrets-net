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

        private FileSystemState(IRuntimeFileInfo path)
            : this()
        {
            _path = path;
        }

        public static IRuntimeFileInfo DefaultPathInfo
        {
            get
            {
                return OS.Current.FileInfo(Path.Combine(OS.Current.WorkFolder.FullName, "FileSystemState.xml"));
            }
        }

        [OnDeserializing]
        private void Initialize(StreamingContext context)
        {
            Identities = new List<PassphraseIdentity>();
            Settings = new UserSettings();
            _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
        }

        [OnDeserialized]
        private void Finalize(StreamingContext context)
        {
            Identities = new List<PassphraseIdentity>(Identities);
        }

        public FileSystemStateActions Actions
        {
            get
            {
                return FactoryRegistry.Instance.Create<FileSystemState, FileSystemStateActions>(this);
            }
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath;

        private long? _keyWrapIterations = null;

        [DataMember(Name = "KeyWrapIterations")]
        public long KeyWrapIterations
        {
            get
            {
                if (!_keyWrapIterations.HasValue)
                {
                    _keyWrapIterations = OS.Current.KeyWrapIterations;
                }

                return _keyWrapIterations.Value;
            }
            private set
            {
                _keyWrapIterations = value;
            }
        }

        private KeyWrapSalt _thumbprintSalt;

        [DataMember(Name = "ThumbprintSalt")]
        public KeyWrapSalt ThumbprintSalt
        {
            get
            {
                if (_thumbprintSalt == null)
                {
                    _thumbprintSalt = new KeyWrapSalt(AesKey.DefaultKeyLength);
                }
                return _thumbprintSalt;
            }
            private set
            {
                _thumbprintSalt = value;
            }
        }

        [DataMember(Name = "PassphraseIdentities")]
        public IList<PassphraseIdentity> Identities
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
                return WatchedFoldersInternal;
            }
            private set
            {
                foreach (WatchedFolder watchedFolder in value)
                {
                    AddWatchedFolderInternal(watchedFolder);
                }
            }
        }

        public void AddWatchedFolder(WatchedFolder watchedFolder)
        {
            if (AddWatchedFolderInternal(watchedFolder))
            {
                OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.WatchedFolderAdded, Instance.KnownKeys.DefaultEncryptionKey, watchedFolder.Path));
            }
        }

        private bool AddWatchedFolderInternal(WatchedFolder watchedFolder)
        {
            if (WatchedFoldersInternal.Contains(watchedFolder))
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
        }

        public void RemoveWatchedFolder(IRuntimeFileInfo fileInfo)
        {
            WatchedFoldersInternal.Remove(new WatchedFolder(fileInfo.FullName, AesKeyThumbprint.Zero));
            OS.Current.NotifySessionChanged(new SessionEvent(SessionEventType.WatchedFolderRemoved, Instance.KnownKeys.DefaultEncryptionKey, fileInfo.FullName));
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
        public ActiveFile FindEncryptedPath(string encryptedPath)
        {
            if (encryptedPath == null)
            {
                throw new ArgumentNullException("encryptedPath");
            }
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
            Instance.ProcessState.Add(process, activeFile);
            Add(activeFile);
        }

        /// <summary>
        /// Remove a file from the volatile file system state. To persist, call Save().
        /// </summary>
        /// <param name="activeFile">An active file to remove</param>
        public void Remove(ActiveFile activeFile)
        {
            if (activeFile == null)
            {
                throw new ArgumentNullException("activeFile");
            }
            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath.Remove(activeFile.EncryptedFileInfo.FullName);
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NoLongerActive);
            OnActiveFileChanged(new ActiveFileChangedEventArgs(activeFile));
        }

        private void AddInternal(ActiveFile activeFile)
        {
            lock (_activeFilesByEncryptedPath)
            {
                _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            }
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

        [DataMember(Name = "UserSettings")]
        public UserSettings Settings
        {
            get;
            set;
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
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                ActiveFile updatedActiveFile = action(activeFile);
                activeFiles.Add(updatedActiveFile);
                bool isModified = updatedActiveFile != activeFile;
                if (isModified || mode == ChangedEventMode.RaiseAlways)
                {
                    OnActiveFileChanged(new ActiveFileChangedEventArgs(updatedActiveFile));
                }
                isAnyModified |= isModified;
            }
            if (isAnyModified)
            {
                SetRangeInternal(activeFiles, ActiveFileStatus.None);
                Save();
            }
        }

        private IRuntimeFileInfo _path;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The actual exception thrown by the de-serialization varies, even by platform, and the idea is to catch those and let the user continue.")]
        public static FileSystemState Create(IRuntimeFileInfo path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Exists)
            {
                return CreateFileSystemState(path);
            }

            FileSystemState fileSystemState = new FileSystemState(path);
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("No existing FileSystemState. Save location is '{0}'.".InvariantFormat(path.FullName));
            }
            return fileSystemState;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If the state can't be read, the software is rendered useless, so it's better to revert to empty here.")]
        private static FileSystemState CreateFileSystemState(IRuntimeFileInfo path)
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
                    if (OS.Log.IsErrorEnabled)
                    {
                        OS.Log.LogError("Exception {1} reading {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName, ex.Message));
                    }
                    return new FileSystemState(path);
                }
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Loaded FileSystemState from '{0}'.".InvariantFormat(path));
                }
                fileSystemState._path = path;
                return fileSystemState;
            }
        }

        public void Save()
        {
            if (_path == null)
            {
                return;
            }
            lock (_activeFilesByEncryptedPath)
            {
                using (Stream fileSystemStateStream = _path.OpenWrite())
                {
                    fileSystemStateStream.SetLength(0);
                    DataContractSerializer serializer = CreateSerializer();
                    serializer.WriteObject(fileSystemStateStream, this);
                }
            }
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Wrote FileSystemState to '{0}'.".InvariantFormat(_path));
            }
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