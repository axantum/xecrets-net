using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace Axantum.AxCrypt
{
    public static class EncryptedFileManager
    {
        private static readonly object _lock = InitializeGetLockObject();

        public static event EventHandler<EventArgs> Changed;

        private static void ActiveFileMonitor_Changed(object sender, EventArgs e)
        {
            lock (_lock)
            {
                OnChanged(e);
            }
        }

        private static object InitializeGetLockObject()
        {
            Initialize();

            return new object();
        }

        private static void Initialize()
        {
            ActiveFileMonitor.Changed += new EventHandler<EventArgs>(ActiveFileMonitor_Changed);
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

        public static void ForceActiveFilesStatus()
        {
            lock (_lock)
            {
                ActiveFileMonitor.ForceActiveFilesStatus();
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
                destinationPath = Path.Combine(ActiveFileMonitor.TemporaryFolderInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
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