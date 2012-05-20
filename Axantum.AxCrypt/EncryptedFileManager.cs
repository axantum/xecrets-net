using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;

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

        public static FileOperationStatus Open(string file, IEnumerable<AesKey> keys)
        {
            lock (_lock)
            {
                return OpenInternal(file, keys);
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

        public static void PurgeActiveFiles()
        {
            lock (_lock)
            {
                ActiveFileMonitor.PurgeActiveFiles();
            }
        }

        public static bool IgnoreApplication
        {
            get
            {
                return ActiveFileMonitor.IgnoreApplication;
            }
            set
            {
                ActiveFileMonitor.IgnoreApplication = value;
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

        private static FileOperationStatus OpenInternal(string file, IEnumerable<AesKey> keys)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                return FileOperationStatus.FileDoesNotExist;
            }

            ActiveFile destinationActiveFile = ActiveFileMonitor.FindActiveFile(fileInfo.FullName);

            string destinationPath = null;
            if (destinationActiveFile != null)
            {
                if (File.Exists(destinationActiveFile.DecryptedPath))
                {
                    destinationPath = destinationActiveFile.DecryptedPath;
                }
            }

            AesKey usedKey = null;

            try
            {
                if (destinationPath == null)
                {
                    string destinationFolder;
                    if (destinationActiveFile != null)
                    {
                        destinationFolder = Path.GetDirectoryName(destinationActiveFile.DecryptedPath);
                    }
                    else
                    {
                        destinationFolder = Path.Combine(ActiveFileMonitor.TemporaryDirectoryInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                    }
                    Directory.CreateDirectory(destinationFolder);

                    IRuntimeFileInfo source = AxCryptEnvironment.Current.FileInfo(fileInfo);

                    bool isDecrypted = false;
                    foreach (AesKey key in keys)
                    {
                        string destinationName = AxCryptFile.Decrypt(source, destinationFolder, key, AxCryptOptions.None);
                        if (!String.IsNullOrEmpty(destinationName))
                        {
                            destinationPath = Path.Combine(destinationFolder, destinationName);
                            isDecrypted = true;
                            KnownKeys.Add(key);
                            usedKey = key;
                            break;
                        }
                    }
                    if (!isDecrypted)
                    {
                        return FileOperationStatus.InvalidKey;
                    }
                }
            }
            catch (IOException)
            {
                return FileOperationStatus.CannotWriteDestination;
            }

            Process process;
            try
            {
                process = Process.Start(destinationPath);
            }
            catch (Win32Exception)
            {
                return FileOperationStatus.CannotStartApplication;
            }

            if (Logging.IsWarningEnabled)
            {
                if (process.HasExited)
                {
                    Logging.Warning("The process seems to exit immediately for '{0}'".InvariantFormat(destinationPath));
                }
            }

            destinationActiveFile = new ActiveFile(fileInfo.FullName, destinationPath, usedKey, ActiveFileStatus.AssumedOpenAndDecrypted, process);
            ActiveFileMonitor.AddActiveFile(destinationActiveFile);

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Launched and opened '{0}'.".InvariantFormat(destinationActiveFile.DecryptedPath));
            }

            return FileOperationStatus.Success;
        }
    }
}