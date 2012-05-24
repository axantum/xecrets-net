using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;

namespace Axantum.AxCrypt
{
    public class EncryptedFileManager
    {
        private static readonly object _lock = new object();

        public event EventHandler<EventArgs> Changed;

        private ProgressManager _progressManager;

        private ActiveFileMonitor _activeFileMonitor;

        public EncryptedFileManager(ProgressManager progressManager)
        {
            _progressManager = progressManager;
            _activeFileMonitor = new ActiveFileMonitor(progressManager);
            _activeFileMonitor.Changed += new EventHandler<EventArgs>(ActiveFileMonitor_Changed);
        }

        private void ActiveFileMonitor_Changed(object sender, EventArgs e)
        {
            lock (_lock)
            {
                OnChanged(e);
            }
        }

        public void ForEach(bool forceChange, Func<ActiveFile, ActiveFile> action)
        {
            lock (_lock)
            {
                _activeFileMonitor.ForEach(forceChange, action);
            }
        }

        public FileOperationStatus Open(string file, IEnumerable<AesKey> keys)
        {
            lock (_lock)
            {
                return OpenInternal(file, keys);
            }
        }

        public void CheckActiveFilesStatus()
        {
            lock (_lock)
            {
                _activeFileMonitor.CheckActiveFilesStatus();
            }
        }

        public void ForceActiveFilesStatus()
        {
            lock (_lock)
            {
                _activeFileMonitor.ForceActiveFilesStatus();
            }
        }

        public void PurgeActiveFiles()
        {
            lock (_lock)
            {
                _activeFileMonitor.PurgeActiveFiles();
            }
        }

        public bool IgnoreApplication
        {
            get
            {
                return _activeFileMonitor.IgnoreApplication;
            }
            set
            {
                _activeFileMonitor.IgnoreApplication = value;
            }
        }

        private void OnChanged(EventArgs eventArgs)
        {
            EventHandler<EventArgs> changed = Changed;
            if (changed != null)
            {
                changed(null, eventArgs);
            }
        }

        private FileOperationStatus OpenInternal(string file, IEnumerable<AesKey> keys)
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
                    IRuntimeFileInfo source = AxCryptEnvironment.Current.FileInfo(destinationActiveFile.EncryptedPath);
                    foreach (AesKey key in keys)
                    {
                        AxCryptDocument document = AxCryptFile.Document(source, key);
                        if (document.PassphraseIsValid)
                        {
                            destinationActiveFile = new ActiveFile(destinationActiveFile, key);
                            FileSystemState.Current.Add(destinationActiveFile);

                            return LaunchApplicationForDocument(destinationActiveFile);
                        }
                    }
                    return FileOperationStatus.InvalidKey;
                }
            }

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
                        destinationFolder = Path.Combine(_activeFileMonitor.TemporaryDirectoryInfo.FullName, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                    }
                    Directory.CreateDirectory(destinationFolder);

                    IRuntimeFileInfo source = AxCryptEnvironment.Current.FileInfo(fileInfo);

                    bool isDecrypted = false;
                    foreach (AesKey key in keys)
                    {
                        string destinationName = AxCryptFile.Decrypt(source, destinationFolder, key, AxCryptOptions.None, _progressManager.Create(Path.GetFileName(source.FullName)));
                        if (!String.IsNullOrEmpty(destinationName))
                        {
                            destinationPath = Path.Combine(destinationFolder, destinationName);
                            destinationActiveFile = new ActiveFile(fileInfo.FullName, destinationPath, key, ActiveFileStatus.AssumedOpenAndDecrypted, null);
                            isDecrypted = true;
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

            return LaunchApplicationForDocument(destinationActiveFile);
        }

        private FileOperationStatus LaunchApplicationForDocument(ActiveFile destinationActiveFile)
        {
            Process process;
            try
            {
                process = Process.Start(destinationActiveFile.DecryptedPath);
            }
            catch (Win32Exception)
            {
                return FileOperationStatus.CannotStartApplication;
            }

            if (Logging.IsWarningEnabled)
            {
                if (process.HasExited)
                {
                    Logging.Warning("The process seems to exit immediately for '{0}'".InvariantFormat(destinationActiveFile.DecryptedPath));
                }
            }

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Launched and opened '{0}'.".InvariantFormat(destinationActiveFile.DecryptedPath));
            }

            destinationActiveFile = new ActiveFile(destinationActiveFile, ActiveFileStatus.AssumedOpenAndDecrypted, process);
            _activeFileMonitor.AddActiveFile(destinationActiveFile);

            return FileOperationStatus.Success;
        }
    }
}