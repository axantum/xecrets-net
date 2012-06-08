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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Session
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class FileSystemState
    {
        private object _lock;

        private FileSystemState()
        {
            Initialize();
        }

        private void Initialize()
        {
            _lock = new object();
        }

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();

        private Dictionary<string, ActiveFile> _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();

        public event EventHandler<EventArgs> Changed;

        protected virtual void OnChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = Changed;
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
            set
            {
                lock (_lock)
                {
                    SetRangeInternal(value, ActiveFileStatus.None);
                }
            }
        }

        public ActiveFile FindEncryptedPath(string encryptedPath)
        {
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

        public ActiveFile FindDecryptedPath(string decryptedPath)
        {
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

        public void Add(ActiveFile activeFile)
        {
            lock (_lock)
            {
                AddInternal(activeFile);
            }
        }

        public void Remove(ActiveFile activeFile)
        {
            lock (_lock)
            {
                _activeFilesByDecryptedPath.Remove(activeFile.DecryptedPath);
                _activeFilesByEncryptedPath.Remove(activeFile.EncryptedPath);
            }
        }

        private void AddInternal(ActiveFile activeFile)
        {
            _activeFilesByEncryptedPath[activeFile.EncryptedPath] = activeFile;
            _activeFilesByDecryptedPath[activeFile.DecryptedPath] = activeFile;
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

        private string _path;

        public string DirectoryPath
        {
            get { return _path; }
        }

        public static FileSystemState Load(IRuntimeFileInfo path)
        {
            if (!path.Exists)
            {
                FileSystemState state = new FileSystemState();
                state._path = path.FullName;
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("No existing FileSystemState. Save location is '{0}'.".InvariantFormat(state._path));
                }
                return state;
            }

            DataContractSerializer serializer = CreateSerializer();
            using (FileStream fileSystemStateStream = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                FileSystemState fileSystemState = (FileSystemState)serializer.ReadObject(fileSystemStateStream);
                fileSystemState._path = path.FullName;
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Loaded FileSystemState from '{0}'.".InvariantFormat(fileSystemState._path));
                }
                return fileSystemState;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        public void Save()
        {
            using (FileStream fileSystemStateStream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                lock (_lock)
                {
                    DataContractSerializer serializer = CreateSerializer();
                    serializer.WriteObject(fileSystemStateStream, this);
                }
            }
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Wrote FileSystemState to '{0}'.".InvariantFormat(_path));
            }
        }

        private static DataContractSerializer CreateSerializer()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(FileSystemState), "FileSystemState", "http://www.axantum.com/Serialization/");
            return serializer;
        }
    }
}