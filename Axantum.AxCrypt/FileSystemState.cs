using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public sealed class FileSystemState
    {
        private static FileSystemState _current = new FileSystemState();

        public static FileSystemState Current
        {
            get
            {
                return _current;
            }
            set
            {
                _current = value;
            }
        }

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

        private object _lock;

        public IEnumerable<ActiveFile> ActiveFiles
        {
            get
            {
                return _activeFilesByDecryptedPath.Values;
            }
            set
            {
                lock (_lock)
                {
                    SetRangeInternal(value);
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
                SetRangeInternal(value);
            }
        }

        private void SetRangeInternal(IEnumerable<ActiveFile> activeFiles)
        {
            _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();
            _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
            foreach (ActiveFile activeFile in activeFiles)
            {
                AddInternal(activeFile);
            }
        }

        private string _path;

        public static void Load(IRuntimeFileInfo path)
        {
            lock (Current._lock)
            {
                if (!path.Exists)
                {
                    Current = new FileSystemState();
                    Current._path = path.FullName;
                    return;
                }

                DataContractSerializer serializer = CreateSerializer();
                using (FileStream fileSystemStateStream = new FileStream(path.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    FileSystemState fileSystemState = (FileSystemState)serializer.ReadObject(fileSystemStateStream);
                    fileSystemState._path = path.FullName;
                    Current = fileSystemState;
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
            DataContractSerializer serializer = CreateSerializer();
            using (FileStream fileSystemStateStream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                lock (_lock)
                {
                    serializer.WriteObject(fileSystemStateStream, this);
                }
            }
        }

        private static DataContractSerializer CreateSerializer()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(FileSystemState), "FileSystemState", "http://www.axantum.com/Serialization/");
            return serializer;
        }
    }
}