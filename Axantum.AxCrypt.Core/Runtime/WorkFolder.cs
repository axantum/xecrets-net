using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class WorkFolder : IDisposable
    {
        private IFileWatcher _workFolderWatcher;

        public WorkFolder(string path)
        {
            FileInfo = Factory.New<IRuntimeFileInfo>(path);
            FileInfo.CreateFolder();
            _workFolderWatcher = Factory.New<IFileWatcher>(FileInfo.FullName);
            _workFolderWatcher.FileChanged += HandleWorkFolderFileChangedEvent;
        }

        public IRuntimeFileInfo FileInfo { get; private set; }

        private void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            if (e.FullName == Instance.FileSystemState.PathInfo.FullName)
            {
                return;
            }
            Instance.SessionNotification.Notify(new SessionNotification(SessionNotificationType.WorkFolderChange, e.FullName));
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
            if (_workFolderWatcher != null)
            {
                _workFolderWatcher.Dispose();
                _workFolderWatcher = null;
            }
        }
    }
}