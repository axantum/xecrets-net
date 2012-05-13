using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class EncryptedFileManager
    {
        private static DirectoryInfo _tempDir = MakeTemporaryDirectory();

        private static DirectoryInfo MakeTemporaryDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "AxCrypt");
            DirectoryInfo tempInfo = new DirectoryInfo(tempDir);
            tempInfo.Create();

            return tempInfo;
        }

        public static FileOperationStatus Open(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                return FileOperationStatus.FileDoesNotExist;
            }

            ActiveFile destinationActiveFile = ActiveFileState.FindActiveFile(fileInfo.FullName);

            if (destinationActiveFile == null)
            {
                string destinationFileName = Path.Combine(_tempDir.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                Directory.CreateDirectory(destinationFileName);
                destinationFileName = Path.Combine(destinationFileName, fileInfo.Name);
                destinationActiveFile = new ActiveFile(fileInfo.FullName, destinationFileName, fileInfo.LastWriteTimeUtc);
            }

            try
            {
                fileInfo.CopyTo(destinationActiveFile.DecryptedPath);
            }
            catch (IOException)
            {
                return FileOperationStatus.CannotWriteDestination;
            }

            try
            {
                Process.Start(destinationActiveFile.DecryptedPath);
            }
            catch (Win32Exception)
            {
                return FileOperationStatus.CannotStartApplication;
            }

            ActiveFileState.AddActiveFile(destinationActiveFile);

            return FileOperationStatus.Success;
        }
    }
}