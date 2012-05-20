using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class FileSystemState
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

        private Dictionary<string, ActiveFile> _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();

        private Dictionary<string, ActiveFile> _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();

        private object _lock = new object();

        public IEnumerable<ActiveFile> ActiveFiles
        {
            get
            {
                return _activeFilesByDecryptedPath.Values;
            }
            set
            {
                SetRange(value);
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
                _activeFilesByEncryptedPath[activeFile.EncryptedPath] = activeFile;
                _activeFilesByDecryptedPath[activeFile.DecryptedPath] = activeFile;
            }
        }

        [DataMember(Name = "ActiveFiles")]
        protected ActiveFileList ActiveFilesForSerialization
        {
            get
            {
                return new ActiveFileList(_activeFilesByEncryptedPath.Values);
            }
            set
            {
                _lock = new object();
                SetRange(value);
            }
        }

        private void SetRange(IEnumerable<ActiveFile> activeFiles)
        {
            lock (_lock)
            {
                _activeFilesByDecryptedPath = new Dictionary<string, ActiveFile>();
                _activeFilesByEncryptedPath = new Dictionary<string, ActiveFile>();
                foreach (ActiveFile activeFile in activeFiles)
                {
                    Add(activeFile);
                }
            }
        }

        private string _path;

        public static void Load(string path)
        {
            lock (Current._lock)
            {
                if (!File.Exists(path))
                {
                    Current = new FileSystemState();
                    Current._path = path;
                    return;
                }
                DataContractSerializer serializer = CreateSerializer();
                using (FileStream fileSystemStateStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    FileSystemState fileSystemState = (FileSystemState)serializer.ReadObject(fileSystemStateStream);
                    fileSystemState._path = path;
                    Current = fileSystemState;
                }
            }
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