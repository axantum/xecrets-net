using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class ActiveFileMonitor
    {
        private static IDictionary<string, ActiveFile> _activeFiles = new Dictionary<string, ActiveFile>();
        private static readonly object _lock = new object();

        public static event EventHandler<EventArgs> Changed;

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_lock)
            {
                _activeFiles[activeFile.EncryptedPath] = activeFile;
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

        internal static void CheckActiveFilesStatus()
        {
            bool isChanged = false;
            lock (_lock)
            {
                IDictionary<string, ActiveFile> activeFiles = new Dictionary<string, ActiveFile>();

                foreach (ActiveFile activeFile in _activeFiles.Values)
                {
                    ActiveFile updatedActiveFile = CheckActiveFileStatus(activeFile);
                    activeFiles[updatedActiveFile.EncryptedPath] = updatedActiveFile;
                    isChanged |= updatedActiveFile != activeFile;
                }
                _activeFiles = activeFiles;
            }
            if (isChanged)
            {
                OnChanged(new EventArgs());
            }
        }

        private static ActiveFile CheckActiveFileStatus(ActiveFile activeFile)
        {
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

            activeFile = new ActiveFile(activeFile.EncryptedPath, activeFile.DecryptedPath, ActiveFileStatus.Deleted);
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
    }
}