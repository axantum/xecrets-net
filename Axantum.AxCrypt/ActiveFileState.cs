using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class ActiveFileState
    {
        private static IList<ActiveFile> _activeFiles = new List<ActiveFile>();

        public static event EventHandler<EventArgs> Changed;

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_activeFiles)
            {
                _activeFiles.Add(activeFile);
            }
            OnChanged(new EventArgs());
        }

        public static IEnumerable<ActiveFile> ActiveFiles
        {
            get
            {
                return new List<ActiveFile>(_activeFiles);
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
            bool isChanged = false;
            lock (_activeFiles)
            {
                IList<ActiveFile> activeFiles = new List<ActiveFile>();

                foreach (ActiveFile activeFile in _activeFiles)
                {
                    ActiveFile updatedActiveFile = CheckActiveFileStatus(activeFile);
                    if (updatedActiveFile != null)
                    {
                        activeFiles.Add(activeFile);
                    }
                    isChanged |= updatedActiveFile == null || !updatedActiveFile.Equals(activeFile);
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
                    activeFile = new ActiveFile(activeFile.EncryptedPath, activeFile.DecryptedPath, activeFileInfo.LastWriteTimeUtc);
                }
            }
            finally
            {
                if (activeFileStream != null)
                {
                    activeFileStream.Close();
                }
            }

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