using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt
{
    internal static class ActiveFileMonitor
    {
        private static readonly object _lock = new object();

        private static ActiveFileDictionary _activeFiles = Load();

        public static event EventHandler<EventArgs> Changed;

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_lock)
            {
                _activeFiles[activeFile.EncryptedPath] = activeFile;
                Save();
            }
            OnChanged(new EventArgs());
        }

        public static void ForEach(Action<ActiveFile> action)
        {
            lock (_lock)
            {
                foreach (ActiveFile activeFile in _activeFiles.Values)
                {
                    action(activeFile);
                }
            }
        }

        public static ActiveFile FindActiveFile(string encryptedPath)
        {
            lock (_lock)
            {
                ActiveFile activeFile;
                if (_activeFiles.TryGetValue(encryptedPath, out activeFile))
                {
                    return activeFile;
                }
                return null;
            }
        }

        private static void OnChanged(EventArgs eventArgs)
        {
            EventHandler<EventArgs> changed = Changed;
            if (changed != null)
            {
                changed(null, eventArgs);
            }
        }

        public static void CheckActiveFilesStatus()
        {
            CheckActiveFilesStatusInternal(false);
        }

        public static void ForceActiveFilesStatus()
        {
            CheckActiveFilesStatusInternal(true);
        }

        private static void CheckActiveFilesStatusInternal(bool forceChanged)
        {
            bool isChanged = forceChanged;
            lock (_lock)
            {
                ActiveFileDictionary activeFiles = new ActiveFileDictionary();

                foreach (ActiveFile activeFile in _activeFiles.Values)
                {
                    ActiveFile updatedActiveFile = activeFile;
                    if (activeFile.Status != ActiveFileStatus.Locked || (DateTime.UtcNow - activeFile.LastAccessTimeUtc) > new TimeSpan(0, 0, 5))
                    {
                        updatedActiveFile = CheckActiveFileStatus(activeFile);
                    }
                    activeFiles[updatedActiveFile.EncryptedPath] = updatedActiveFile;
                    isChanged |= updatedActiveFile != activeFile;
                }
                _activeFiles = activeFiles;
            }
            if (isChanged)
            {
                Save();
                OnChanged(new EventArgs());
            }
        }

        private static ActiveFile CheckActiveFileStatus(ActiveFile activeFile)
        {
            if (activeFile.Status == ActiveFileStatus.Deleted)
            {
                return activeFile;
            }

            FileInfo activeFileInfo = new FileInfo(activeFile.DecryptedPath);
            FileStream activeFileStream = null;
            try
            {
                activeFileStream = activeFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return activeFile;
            }

            if (activeFile.Process != null)
            {
                if (!activeFile.Process.HasExited)
                {
                    activeFileStream.Close();
                    return activeFile;
                }
            }

            try
            {
                if (activeFileInfo.LastWriteTimeUtc > activeFile.LastWriteTimeUtc)
                {
                    WriteToFileWithBackup(activeFile.EncryptedPath, (Stream destination) => { activeFileStream.CopyTo(destination); });
                }
            }
            finally
            {
                if (activeFileStream != null)
                {
                    activeFileStream.Close();
                }
            }

            ActiveFile newActiveFile = new ActiveFile(activeFile.EncryptedPath, activeFile.DecryptedPath, ActiveFileStatus.Deleted, null);
            activeFile.Dispose();
            activeFile = newActiveFile;

            activeFileInfo.Delete();
            return activeFile;
        }

        private static void WriteToFileWithBackup(string destinationFilePath, Action<Stream> writeFileStreamTo)
        {
            FileInfo destinationFileInfo = new FileInfo(destinationFilePath);
            string temporaryFilePath = MakeAlternatePath(destinationFileInfo, ".tmp");
            FileInfo temporaryFileInfo = new FileInfo(temporaryFilePath);

            using (FileStream temporaryStream = temporaryFileInfo.Open(FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                writeFileStreamTo(temporaryStream);
            }

            string backupFilePath = MakeAlternatePath(destinationFileInfo, ".bak");
            destinationFileInfo.MoveTo(backupFilePath);
            temporaryFileInfo.MoveTo(destinationFilePath);
            File.Delete(backupFilePath);
        }

        private static string MakeAlternatePath(FileInfo fileInfo, string extension)
        {
            string alternatePath;
            int version = 0;
            do
            {
                string alternateExtension = (version > 0 ? "." + version.ToString(CultureInfo.InvariantCulture) : String.Empty) + extension;
                alternatePath = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(fileInfo.Name) + alternateExtension);
                ++version;
            } while (File.Exists(alternatePath));

            return alternatePath;
        }

        private static DirectoryInfo _temporaryFolderInfo;

        public static DirectoryInfo TemporaryFolderInfo
        {
            get
            {
                if (_temporaryFolderInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), "AxCrypt");
                    DirectoryInfo temporaryFolderInfo = new DirectoryInfo(temporaryFolderPath);
                    temporaryFolderInfo.Create();
                    _temporaryFolderInfo = temporaryFolderInfo;
                }

                return _temporaryFolderInfo;
            }
        }

        private static ActiveFileDictionary Load()
        {
            DataContractSerializer serializer = CreateSerializer();
            string path = Path.Combine(TemporaryFolderInfo.FullName, "ActiveFiles.xml");
            if (!File.Exists(path))
            {
                return new ActiveFileDictionary();
            }
            using (FileStream activeFileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                lock (_lock)
                {
                    ActiveFileDictionary activeFiles = (ActiveFileDictionary)serializer.ReadObject(activeFileStream);
                    return activeFiles;
                }
            }
        }

        private static DataContractSerializer CreateSerializer()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ActiveFileDictionary), "ActiveFiles", "http://www.axantum.com/Serialization/");
            return serializer;
        }

        private static void Save()
        {
            DataContractSerializer serializer = CreateSerializer();
            string path = Path.Combine(TemporaryFolderInfo.FullName, "ActiveFiles.xml");
            using (FileStream activeFileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                lock (_lock)
                {
                    serializer.WriteObject(activeFileStream, _activeFiles);
                }
            }
        }
    }
}