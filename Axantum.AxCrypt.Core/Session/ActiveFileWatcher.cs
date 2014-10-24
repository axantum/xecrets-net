using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileWatcher : IDisposable
    {
        private Dictionary<string, IFileWatcher> _activeFileFolderWatchers = new Dictionary<string, IFileWatcher>();

        public ActiveFileWatcher()
        {
        }

        public void Add(IDataStore file)
        {
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
            if (String.IsNullOrEmpty(e.OldName))
            {
                Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.PurgeActiveFiles, e.FullName));
                return;
            }

            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.FileMove, e.FullName, e.OldName));
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