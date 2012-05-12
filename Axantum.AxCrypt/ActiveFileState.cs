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

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_activeFiles)
            {
                _activeFiles.Add(activeFile);
            }
        }

        public static void CheckActiveFileStatus()
        {
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
                }
                _activeFiles = activeFiles;
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
                    CopyToWithBackup(activeFileStream, activeFile.EncryptedPath);
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

        private static void CopyToWithBackup(FileStream activeFileStream, string destinationPath)
        {
            FileInfo destinationFileInfo = new FileInfo(destinationPath);
            FileInfo temporaryDestination;
            do
            {
                temporaryDestination = new FileInfo(Path.Combine(destinationFileInfo.DirectoryName, Path.GetRandomFileName()));
            }
            while (temporaryDestination.Exists);
            using (FileStream destinationStream = temporaryDestination.Open(FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                activeFileStream.CopyTo(destinationStream);
            }
            string backupFilePath;
            int version = 0;
            do
            {
                backupFilePath = Path.Combine(destinationFileInfo.DirectoryName, destinationFileInfo.Name + "." + version.ToString(CultureInfo.InvariantCulture));
                ++version;
            } while (File.Exists(backupFilePath));
            destinationFileInfo.MoveTo(backupFilePath);
            temporaryDestination.MoveTo(destinationPath);
            File.Delete(backupFilePath);
        }
    }
}