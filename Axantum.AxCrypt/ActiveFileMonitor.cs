using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt
{
    internal static class ActiveFileMonitor
    {
        private static readonly object _lock = new object();

        private static string _fileSystemStateFullName = Path.Combine(TemporaryDirectoryInfo.FullName, "FileSystemState.xml");

        private static FileSystemWatcher _TemporaryDirectoryWatcher = InitializeTemporaryDirectoryWatcher();

        private static FileSystemWatcher InitializeTemporaryDirectoryWatcher()
        {
            FileSystemWatcher temporaryDirectoryWatcher = new FileSystemWatcher(TemporaryDirectoryInfo.FullName);
            temporaryDirectoryWatcher.Changed += TemporaryDirectoryWatcher_Changed;
            temporaryDirectoryWatcher.Created += TemporaryDirectoryWatcher_Changed;
            temporaryDirectoryWatcher.Deleted += TemporaryDirectoryWatcher_Changed;
            temporaryDirectoryWatcher.IncludeSubdirectories = true;
            temporaryDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite;

            FileSystemState.Load(_fileSystemStateFullName);

            return temporaryDirectoryWatcher;
        }

        public static event EventHandler<EventArgs> Changed;

        public static void AddActiveFile(ActiveFile activeFile)
        {
            lock (_lock)
            {
                FileSystemState.Current.Add(activeFile);
                FileSystemState.Current.Save();
            }
            OnChanged(new EventArgs());
        }

        public static void ForEach(bool forceChange, Func<ActiveFile, ActiveFile> action)
        {
            bool isChanged = forceChange;
            lock (_lock)
            {
                List<ActiveFile> activeFiles = new List<ActiveFile>();
                foreach (ActiveFile activeFile in FileSystemState.Current.ActiveFiles)
                {
                    ActiveFile updatedActiveFile = action(activeFile);
                    activeFiles.Add(updatedActiveFile);
                    if (updatedActiveFile != activeFile)
                    {
                        activeFile.Dispose();
                    }
                    isChanged |= updatedActiveFile != activeFile;
                }
                if (isChanged)
                {
                    FileSystemState.Current.ActiveFiles = activeFiles;
                    FileSystemState.Current.Save();
                    OnChanged(new EventArgs());
                }
            }
        }

        private static bool _ignoreApplication;

        public static bool IgnoreApplication
        {
            get
            {
                return _ignoreApplication;
            }
            set
            {
                lock (_lock)
                {
                    _ignoreApplication = value;
                }
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
            CheckActiveFilesStatusInternal(false);
        }

        public static void ForceActiveFilesStatus()
        {
            CheckActiveFilesStatusInternal(true);
        }

        private static void CheckActiveFilesStatusInternal(bool forceChanged)
        {
            ForEach(forceChanged, (ActiveFile activeFile) =>
            {
                if (DateTime.UtcNow - activeFile.LastAccessTimeUtc > new TimeSpan(0, 0, 5))
                {
                    activeFile = CheckActiveFileStatus(activeFile);
                }
                return activeFile;
            });
        }

        private static ActiveFile CheckActiveFileStatus(ActiveFile activeFile)
        {
            if (activeFile.Status == ActiveFileStatus.NotDecrypted)
            {
                if (!File.Exists(activeFile.DecryptedPath))
                {
                    return activeFile;
                }
                activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, activeFile.Process);
            }

            if (activeFile.Process != null && !IgnoreApplication)
            {
                if (activeFile.Process.HasExited)
                {
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Process exit for '{0}'".InvariantFormat(activeFile.DecryptedPath));
                    }
                    activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, null);
                }
            }

            FileInfo activeFileInfo = new FileInfo(activeFile.DecryptedPath);

            FileStream activeFileStream = null;
            try
            {
                if (activeFileInfo.LastWriteTimeUtc > activeFile.LastWriteTimeUtc)
                {
                    if (activeFile.Key == null || activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
                    {
                        return activeFile;
                    }
                    try
                    {
                        activeFileStream = activeFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (IOException)
                    {
                        if (Logging.IsWarningEnabled && !IgnoreApplication)
                        {
                            Logging.Warning("Failed exclusive open modified for '{0}'.".InvariantFormat(activeFileInfo.FullName));
                        }
                        activeFile = new ActiveFile(activeFile, activeFile.Status | ActiveFileStatus.NotShareable, activeFile.Process);
                        return activeFile;
                    }
                    IRuntimeFileInfo sourceFileInfo = AxCryptEnvironment.Current.FileInfo(activeFile.DecryptedPath);
                    WriteToFileWithBackup(activeFile.EncryptedPath, (Stream destination) =>
                    {
                        AxCryptFile.Encrypt(sourceFileInfo, destination, activeFile.Key, AxCryptOptions.EncryptWithCompression);
                    });
                    if (Logging.IsInfoEnabled)
                    {
                        Logging.Info("Wrote back '{0}' to '{1}'".InvariantFormat(activeFile.DecryptedPath, activeFile.EncryptedPath));
                    }
                    activeFile = new ActiveFile(activeFile.EncryptedPath, activeFile.DecryptedPath, activeFile.Key, ActiveFileStatus.AssumedOpenAndDecrypted, activeFile.Process);
                }
            }
            finally
            {
                if (activeFileStream != null)
                {
                    activeFileStream.Close();
                }
            }

            if (AxCryptEnvironment.Current.IsDesktopWindows)
            {
                if (activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) && !activeFile.Status.HasFlag(ActiveFileStatus.NotShareable))
                {
                    activeFile = TryDelete(activeFile);
                }
            }

            return activeFile;
        }

        public static void PurgeActiveFiles()
        {
            ForEach(false, (ActiveFile activeFile) =>
            {
                if (activeFile.Status.HasFlag(ActiveFileStatus.AssumedOpenAndDecrypted) || activeFile.Status.HasFlag(ActiveFileStatus.DecryptedIsPendingDelete))
                {
                    activeFile = TryDelete(activeFile);
                }
                return activeFile;
            });
        }

        private static ActiveFile TryDelete(ActiveFile activeFile)
        {
            FileInfo activeFileInfo = new FileInfo(activeFile.DecryptedPath);

            if (activeFileInfo.LastWriteTimeUtc > activeFile.LastWriteTimeUtc)
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Tried delete '{0}' but it is modified.".InvariantFormat(activeFile.DecryptedPath));
                }
                activeFile = new ActiveFile(activeFile, ActiveFileStatus.AssumedOpenAndDecrypted, activeFile.Process);
                return activeFile;
            }

            try
            {
                if (Logging.IsInfoEnabled)
                {
                    Logging.Info("Deleting '{0}'.".InvariantFormat(activeFile.DecryptedPath));
                }
                activeFileInfo.Delete();
            }
            catch (IOException)
            {
                if (Logging.IsErrorEnabled)
                {
                    Logging.Error("Delete failed for '{0}'".InvariantFormat(activeFileInfo.FullName));
                }
                activeFile = new ActiveFile(activeFile, ActiveFileStatus.DecryptedIsPendingDelete, activeFile.Process);
                return activeFile;
            }

            activeFile = new ActiveFile(activeFile.EncryptedPath, activeFile.DecryptedPath, activeFile.Key, ActiveFileStatus.NotDecrypted, null);

            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Deleted '{0}' from '{1}'.".InvariantFormat(activeFile.DecryptedPath, activeFile.EncryptedPath));
            }

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

        private static DirectoryInfo _temporaryDirectoryInfo;

        public static DirectoryInfo TemporaryDirectoryInfo
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), "AxCrypt");
                    DirectoryInfo temporaryFolderInfo = new DirectoryInfo(temporaryFolderPath);
                    temporaryFolderInfo.Create();
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }

                return _temporaryDirectoryInfo;
            }
        }

        public static void TemporaryDirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            ActiveFile changedFile = FileSystemState.Current.FindDecryptedPath(e.FullPath);
            if (changedFile == null)
            {
                return;
            }
        }

        public static ActiveFile FindActiveFile(string path)
        {
            lock (_lock)
            {
                return FileSystemState.Current.FindEncryptedPath(path);
            }
        }
    }
}