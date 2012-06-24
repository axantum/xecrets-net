using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileWatcher : IFileWatcher
    {
        private FileSystemWatcher _temporaryDirectoryWatcher;

        public FileWatcher(string path)
        {
            _temporaryDirectoryWatcher = new FileSystemWatcher(path);
            _temporaryDirectoryWatcher.Changed += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Created += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Deleted += TemporaryDirectoryWatcher_Changed;
            _temporaryDirectoryWatcher.Renamed += new RenamedEventHandler(_temporaryDirectoryWatcher_Renamed);
            _temporaryDirectoryWatcher.IncludeSubdirectories = true;
            _temporaryDirectoryWatcher.Filter = String.Empty;
            _temporaryDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            _temporaryDirectoryWatcher.EnableRaisingEvents = true;
        }

        protected virtual void OnChanged(FileWatcherEventArgs eventArgs)
        {
            EventHandler<FileWatcherEventArgs> fileChanged = FileChanged;
            if (fileChanged != null)
            {
                fileChanged(null, eventArgs);
            }
        }

        private void _temporaryDirectoryWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            FileSystemChanged(e.FullPath);
        }

        private void TemporaryDirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemChanged(e.FullPath);
        }

        private void FileSystemChanged(string fullPath)
        {
            if (Logging.IsInfoEnabled)
            {
                Logging.Info("Watcher says '{0}' changed.".InvariantFormat(fullPath));
            }
            OnChanged(new FileWatcherEventArgs(fullPath));
        }

        #region IFileWatcher Members

        public event EventHandler<FileWatcherEventArgs> FileChanged;

        #endregion IFileWatcher Members

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_temporaryDirectoryWatcher != null)
                {
                    _temporaryDirectoryWatcher.Dispose();
                }
                _temporaryDirectoryWatcher = null;
            }
            _disposed = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}