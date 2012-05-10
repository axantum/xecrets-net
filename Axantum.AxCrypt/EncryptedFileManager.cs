using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            string destinationFileName = Path.Combine(_tempDir.FullName, fileInfo.Name);
            FileInfo copy;
            try
            {
                copy = fileInfo.CopyTo(destinationFileName);
            }
            catch (IOException)
            {
                return FileOperationStatus.CannotWriteDestination;
            }

            ActiveFile activeFile = new ActiveFile(fileInfo.FullName, copy.FullName, fileInfo.LastWriteTimeUtc);

            try
            {
                Process.Start(destinationFileName);
            }
            catch (Win32Exception)
            {
                return FileOperationStatus.CannotStartApplication;
            }

            ActiveFileState.ActiveFiles.Add(activeFile);

            return FileOperationStatus.Success;
        }
    }
}