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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt.Core.Session
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class FileSystemState : IDisposable
    {
        private object _lock;

        public FileSystemState()
        {
            Initialize();
        }

        public static IRuntimeFileInfo DefaultPathInfo
        {
            get
            {
                return OS.Current.FileInfo(Path.Combine(OS.Current.TemporaryDirectoryInfo.FullName, "FileSystemState.xml"));
            }
        }

        private void Initialize()
        {
            _lock = new object();
            KnownKeys = new KnownKeys();
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();

        private Dictionary<string, ActiveFile> _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();

        public KnownKeys KnownKeys { get; private set; }

        public event EventHandler<ActiveFileChangedEventArgs> Changed;

        protected virtual void OnChanged(ActiveFileChangedEventArgs e)
        {
            EventHandler<ActiveFileChangedEventArgs> handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public IEnumerable<ActiveFile> ActiveFiles
        {
            get
            {
                lock (_lock)
                {
                    return new List<ActiveFile>(_activeFilesByDecryptedPath.Values);
                }
            }
        }

        public int ActiveFileCount
        {
            get
            {
                return _activeFilesByDecryptedPath.Count;
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
        /// <param name="decryptedPath">Full path to am encrypted file.</param>
        /// <returns>An ActiveFile instance, or null if not found in file system state.</returns>
        public ActiveFile FindEncryptedPath(string encryptedPath)
        {
            if (encryptedPath == null)
            {
                throw new ArgumentNullException("encryptedPath");
            }
            ActiveFile activeFile;
            lock (_lock)
            {
                if (_activeFilesByEncryptedPath.TryGetValue(encryptedPath, out activeFile))
                {
                    return activeFile;
                }
            }
            return null;
        }

        /// <summary>
        /// Find an active file by way of it's decrypted full path.
        /// </summary>
        /// <param name="decryptedPath">Full path to a decrypted file.</param>
        /// <returns>An ActiveFile instance, or null if not found in file system state.</returns>
        public ActiveFile FindDecryptedPath(string decryptedPath)
        {
            if (decryptedPath == null)
            {
                throw new ArgumentNullException("decryptedPath");
            }
            ActiveFile activeFile;
            lock (_lock)
            {
                if (_activeFilesByDecryptedPath.TryGetValue(decryptedPath, out activeFile))
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
            lock (_lock)
            {
                AddInternal(activeFile);
            }
            OnChanged(new ActiveFileChangedEventArgs(activeFile));
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
            lock (_lock)
            {
                _activeFilesByDecryptedPath.Remove(activeFile.DecryptedFileInfo.FullName);
                _activeFilesByEncryptedPath.Remove(activeFile.EncryptedFileInfo.FullName);
            }
            activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NoLongerActive);
            OnChanged(new ActiveFileChangedEventArgs(activeFile));
        }

        private void AddInternal(ActiveFile activeFile)
        {
            _activeFilesByEncryptedPath[activeFile.EncryptedFileInfo.FullName] = activeFile;
            _activeFilesByDecryptedPath[activeFile.DecryptedFileInfo.FullName] = activeFile;
        }

        [DataMember(Name = "ActiveFiles")]
        private ICollection<ActiveFile> ActiveFilesForSerialization
        {
            get
            {
                return new ActiveFileCollection(_activeFilesByEncryptedPath.Values);
            }
            set
            {
                SetRangeInternal(value, ActiveFileStatus.Error | ActiveFileStatus.IgnoreChange | ActiveFileStatus.NotShareable);
            }
        }

        private void SetRangeInternal(IEnumerable<ActiveFile> activeFiles, ActiveFileStatus mask)
        {
            _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();
            _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
            foreach (ActiveFile activeFile in activeFiles)
            {
                ActiveFile thisActiveFile = activeFile;
                if ((activeFile.Status & mask) != 0)
                {
                    thisActiveFile = new ActiveFile(activeFile, activeFile.Status & ~mask, null);
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

            bool isModified = false;
            List<ActiveFile> activeFiles = new List<ActiveFile>();
            foreach (ActiveFile activeFile in ActiveFiles)
            {
                ActiveFile updatedActiveFile = action(activeFile);
                activeFiles.Add(updatedActiveFile);
                if (updatedActiveFile != activeFile)
                {
                    isModified = true;
                    OnChanged(new ActiveFileChangedEventArgs(updatedActiveFile));
                    activeFile.Dispose();
                }
            }
            if (isModified)
            {
                lock (_lock)
                {
                    SetRangeInternal(activeFiles, ActiveFileStatus.None);
                }
                Save();
            }
            if (!isModified && mode == ChangedEventMode.RaiseAlways)
            {
                RaiseChangedForAll(activeFiles);
            }
        }

        private void RaiseChangedForAll(List<ActiveFile> activeFiles)
        {
            foreach (ActiveFile activeFile in activeFiles)
            {
                OnChanged(new ActiveFileChangedEventArgs(activeFile));
            }
        }

        private string _path;

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The actual exception thrown by the de-serialization varies, even by platform, and the idea is to catch those and let the user continue.")]
        public void Load(IRuntimeFileInfo path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (!path.Exists)
            {
                _path = path.FullName;
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("No existing FileSystemState. Save location is '{0}'.".InvariantFormat(_path));
                }
                return;
            }

            DataContractSerializer serializer = CreateSerializer();
            IRuntimeFileInfo loadInfo = OS.Current.FileInfo(path.FullName);

            using (Stream fileSystemStateStream = loadInfo.OpenRead())
            {
                FileSystemState fileSystemState;
                try
                {
                    fileSystemState = (FileSystemState)serializer.ReadObject(fileSystemStateStream);
                }
                catch (Exception)
                {
                    if (OS.Log.IsErrorEnabled)
                    {
                        OS.Log.LogError("Exception reading {0}. Ignoring and re-initializing state.".InvariantFormat(path.FullName));
                    }
                    fileSystemState = new FileSystemState();
                }
                _path = path.FullName;
                foreach (ActiveFile activeFile in fileSystemState.ActiveFiles)
                {
                    Add(activeFile);
                }
                if (OS.Log.IsInfoEnabled)
                {
                    OS.Log.LogInfo("Loaded FileSystemState from '{0}'.".InvariantFormat(fileSystemState._path));
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        public void Save()
        {
            IRuntimeFileInfo saveInfo = OS.Current.FileInfo(_path);
            lock (_lock)
            {
                using (Stream fileSystemStateStream = saveInfo.OpenWrite())
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
            if (_activeFilesByEncryptedPath == null)
            {
                return;
            }
            if (disposing)
            {
                foreach (ActiveFile activeFile in _activeFilesByEncryptedPath.Values)
                {
                    activeFile.Dispose();
                }
                _activeFilesByEncryptedPath = null;
                _activeFilesByDecryptedPath = null;
            }
        }

        #endregion IDisposable Members
    }
}