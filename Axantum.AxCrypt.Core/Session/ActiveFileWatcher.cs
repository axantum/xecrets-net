using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Core.Session
{
    public class ActiveFileWatcher : IDisposable
    {
        private Dictionary<string, IFileWatcher> _workFolderWatchers = new Dictionary<string, IFileWatcher>();

        public ActiveFileWatcher()
        {
        }

        public void Add(IRuntimeFileInfo file)
        {
            string folder = Path.GetDirectoryName(file.FullName);
            lock (_workFolderWatchers)
            {
                if (_workFolderWatchers.ContainsKey(folder))
                {
                    return;
                }
                IFileWatcher fileWatcher = Factory.New<IFileWatcher>(folder);
                fileWatcher.FileChanged += HandleWorkFolderFileChangedEvent;
                _workFolderWatchers.Add(folder, fileWatcher);
            }
        }

        private void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            if (String.IsNullOrEmpty(e.OldName))
            {
                Instance.SessionNotify.Notify(new SessionNotification(SessionNotificationType.PurgeActiveFiles, e.FullName));
                return;
            }

            Instance.SessionNotify.Notify(new SessionNotification(SessionNotificationType.FileMove, e.FullName, e.OldName));
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
            lock (_workFolderWatchers)
            {
                foreach (IFileWatcher fileWatcher in _workFolderWatchers.Values)
                {
                    fileWatcher.Dispose();
                }
                _workFolderWatchers.Clear();
            }
        }
    }
}