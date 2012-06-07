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
    /// <summary>
    ///
    /// </summary>
    internal class EncryptedFileManager : Component
    {
        public event EventHandler<EventArgs> Changed;

        private ActiveFileMonitor _activeFileMonitor;

        private bool _disposed = false;

        public EncryptedFileManager()
        {
            _activeFileMonitor = new ActiveFileMonitor();
            _activeFileMonitor.Changed += new EventHandler<EventArgs>(ActiveFileMonitor_Changed);
        }

        private void ActiveFileMonitor_Changed(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        public void ForEach(bool forceChange, Func<ActiveFile, ActiveFile> action)
        {
            _activeFileMonitor.ForEach(forceChange, action);
        }

        public FileOperationStatus Open(string file, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            return OpenInternal(file, keys, progress);
        }

        public void CheckActiveFilesStatus(ProgressContext progress)
        {
            _activeFileMonitor.CheckActiveFilesStatus(progress);
        }

        public void ForceActiveFilesStatus(ProgressContext progress)
        {
            _activeFileMonitor.ForceActiveFilesStatus(progress);
        }

        public void PurgeActiveFiles(ProgressContext progress)
        {
            _activeFileMonitor.PurgeActiveFiles(progress);
        }

        public IList<ActiveFile> FindOpenFiles()
        {
            List<ActiveFile> activeFiles = new List<ActiveFile>();
            ForEach(false, (ActiveFile activeFile) =>
            {
                if (activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete) || activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted))
                {
                    activeFiles.Add(activeFile);
                }
                return activeFile;
            });
            return activeFiles;
        }

        public bool UpdateActiveFileIfKeyMatchesThumbprint(AesKey key)
        {
            bool keyMatch = false;
            ForEach(false, (ActiveFile activeFile) =>
            {
                if (activeFile.Key != null)
                {
                    return activeFile;
                }
                if (!activeFile.ThumbprintMatch(key))
                {
                    return activeFile;
                }
                keyMatch = true;

                activeFile = new ActiveFile(activeFile, key);
                return activeFile;
            });
            return keyMatch;
        }

        public void RemoveRecentFile(string encryptedPath)
        {
            ActiveFile activeFile = _activeFileMonitor.FindActiveFile(encryptedPath);
            _activeFileMonitor.RemoveActiveFile(activeFile);
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

        public static void EncryptFile(IRuntimeFileInfo sourceFileInfo, IRuntimeFileInfo destinationFileInfo, AesKey key, ProgressContext progress)
        {
            try
            {
                using (Stream activeFileStream = sourceFileInfo.OpenRead())
                {
                    AxCryptFile.WriteToFileWithBackup(destinationFileInfo.FullName, (Stream destination) =>
                    {
                        AxCryptFile.Encrypt(sourceFileInfo, destination, key, AxCryptOptions.EncryptWithCompression, progress);
                    });
                }
                AxCryptFile.Wipe(sourceFileInfo);
            }
            catch (IOException)
            {
            }
        }

        private FileOperationStatus OpenInternal(string file, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
            {
                if (Logging.IsWarningEnabled)
                {
                    Logging.Warning("Tried to open non-existing '{0}'.".InvariantFormat(fileInfo.FullName));
                }
                return FileOperationStatus.FileDoesNotExist;
            }

            ActiveFile destinationActiveFile = _activeFileMonitor.FindActiveFile(fileInfo.FullName);

            string destinationPath = null;
            if (destinationActiveFile == null || !File.Exists(destinationActiveFile.DecryptedPath))
            {
                destinationActiveFile = TryDecrypt(destinationActiveFile, keys, fileInfo, destinationPath, progress);
            }
            else
            {
                destinationActiveFile = CheckKeysForAlreadyDecryptedFile(destinationActiveFile, keys, progress);
            }

            if (destinationActiveFile == null)
            {
                return FileOperationStatus.InvalidKey;
            }

            _activeFileMonitor.AddActiveFile(destinationActiveFile);

            return LaunchApplicationForDocument(destinationActiveFile);
        }

        private ActiveFile TryDecrypt(ActiveFile destinationActiveFile, IEnumerable<AesKey> keys, FileInfo fileInfo, string destinationPath, ProgressContext progress)
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

            destinationActiveFile = null;
            foreach (AesKey key in keys)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Decrypting '{0}'".InvariantFormat(source.FullName));
                }
                using (FileLock sourceLock = FileLock.Lock(source.FullName))
                {
                    using (AxCryptDocument document = AxCryptFile.Document(source, key, progress))
                    {
                        if (!document.PassphraseIsValid)
                        {
                            continue;
                        }

                        string destinationName = document.DocumentHeaders.FileName;
                        destinationPath = Path.Combine(destinationFolder, destinationName);

                        IRuntimeFileInfo destinationFileInfo = AxCryptEnvironment.Current.FileInfo(destinationPath);
                        using (FileLock fileLock = FileLock.Lock(destinationFileInfo.FullName))
                        {
                            AxCryptFile.Decrypt(document, destinationFileInfo, AxCryptOptions.SetFileTimes, progress);
                        }
                        destinationActiveFile = new ActiveFile(fileInfo.FullName, destinationFileInfo.FullName, key, ActiveFileStatus.AssumedOpenAndDecrypted | ActiveFileStatus.IgnoreChange, null);
                        if (Logging.IsInfoEnabled)
                        {
                            Logging.Info("File decrypted from '{0}' to '{1}'".InvariantFormat(source.FullName, destinationActiveFile.DecryptedPath));
                        }
                        break;
                    }
                }
            }
            return destinationActiveFile;
        }

        private ActiveFile CheckKeysForAlreadyDecryptedFile(ActiveFile destinationActiveFile, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            IRuntimeFileInfo source = AxCryptEnvironment.Current.FileInfo(destinationActiveFile.EncryptedPath);
            foreach (AesKey key in keys)
            {
                using (AxCryptDocument document = AxCryptFile.Document(source, key, progress))
                {
                    if (document.PassphraseIsValid)
                    {
                        if (Logging.IsWarningEnabled)
                        {
                            Logging.Warning("File was already decrypted and the key was known for '{0}' to '{1}'".InvariantFormat(source.FullName, destinationActiveFile.DecryptedPath));
                        }
                        return new ActiveFile(destinationActiveFile, key);
                    }
                }
            }
            return null;
        }

        private FileOperationStatus LaunchApplicationForDocument(ActiveFile destinationActiveFile)
        {
            Process process;
            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Starting process for '{0}'".InvariantFormat(destinationActiveFile.DecryptedPath));
                }
                process = Process.Start(destinationActiveFile.DecryptedPath);
                if (process == null)
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Starting process for '{0}' did not start a process, assumed handled by the shell.".InvariantFormat(destinationActiveFile.DecryptedPath));
                    }
                    return FileOperationStatus.Success;
                }
                process.Exited += new EventHandler(process_Exited);
            }
            catch (Win32Exception w32ex)
            {
                if (Logging.IsErrorEnabled)
                {
                    Logging.Error("Could not launch application for '{0}', Win32Exception was '{1}'.".InvariantFormat(destinationActiveFile.DecryptedPath, w32ex.Message));
                }
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

        private void process_Exited(object sender, EventArgs e)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Process exit event for '{0}'.".InvariantFormat(((Process)sender).StartInfo.FileName));
            }

            OnChanged(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_activeFileMonitor != null)
                {
                    _activeFileMonitor.Dispose();
                    _activeFileMonitor = null;
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}