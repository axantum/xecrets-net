using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileWatcher : IDisposable
    {
        private Dictionary<string, IFileWatcher> _activeFileFolderWatchers = new Dictionary<string, IFileWatcher>();

        public ActiveFileWatcher()
        {
        }

        public void Add(IDataItem file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }

            string folder = Resolve.Portable.Path().GetDirectoryName(file.FullName);
            lock (_activeFileFolderWatchers)
            {
                if (_activeFileFolderWatchers.ContainsKey(folder))
                {
                    return;
                }
                IFileWatcher fileWatcher = TypeMap.Resolve.New<IFileWatcher>(folder);
                fileWatcher.FileChanged += HandleActiveFileFolderChangedEvent;
                _activeFileFolderWatchers.Add(folder, fileWatcher);
            }
        }

        private void HandleActiveFileFolderChangedEvent(object sender, FileWatcherEventArgs e)
        {
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.UpdateActiveFiles, e.FullNames));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeInternal();
            }
        }

        private void DisposeInternal()
        {
            lock (_activeFileFolderWatchers)
            {
                foreach (IFileWatcher fileWatcher in _activeFileFolderWatchers.Values)
                {
                    fileWatcher.Dispose();
                }
                _activeFileFolderWatchers.Clear();
            }
        }
    }
}