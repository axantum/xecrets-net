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
        private static readonly object _lock = new object();

        private static DirectoryInfo MakeTemporaryDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "AxCrypt");
            DirectoryInfo tempInfo = new DirectoryInfo(tempDir);
            tempInfo.Create();

            return tempInfo;
        }

        public static FileOperationStatus Open(string file)
        {
            lock (_lock)
            {
                return OpenInternal(file);
            }
        }

        public static void CheckActiveFilesStatus()
        {
            lock (_lock)
            {
                ActiveFileMonitor.CheckActiveFilesStatus();
            }
        }

        private static FileOperationStatus OpenInternal(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                return FileOperationStatus.FileDoesNotExist;
            }

            ActiveFile destinationActiveFile = ActiveFileMonitor.FindActiveFile(fileInfo.FullName);

            string destinationPath;
            if (destinationActiveFile == null)
            {
                destinationPath = Path.Combine(_tempDir.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                Directory.CreateDirectory(destinationPath);
                destinationPath = Path.Combine(destinationPath, fileInfo.Name);
            }
            else
            {
                destinationPath = destinationActiveFile.DecryptedPath;
            }

            try
            {
                if (!File.Exists(destinationPath))
                {
                    fileInfo.CopyTo(destinationPath);
                }
            }
            catch (IOException)
            {
                return FileOperationStatus.CannotWriteDestination;
            }

            try
            {
                Process.Start(destinationPath);
            }
            catch (Win32Exception)
            {
                return FileOperationStatus.CannotStartApplication;
            }

            destinationActiveFile = new ActiveFile(fileInfo.FullName, destinationPath, ActiveFileStatus.Locked);
            ActiveFileMonitor.AddActiveFile(destinationActiveFile);
            return FileOperationStatus.Success;
        }
    }
}