using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.IO;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeFileWatcher : IFileWatcher
    {
        internal string Path { get; set; }

        public FakeFileWatcher(string path)
        {
            Path = path;
        }

        internal virtual void OnChanged(FileWatcherEventArgs eventArgs)
        {
            EventHandler<FileWatcherEventArgs> fileChanged = FileChanged;
            if (fileChanged != null)
            {
                fileChanged(null, eventArgs);
            }
        }

        #region IFileWatcher Members

        public event EventHandler<FileWatcherEventArgs> FileChanged;

        #endregion IFileWatcher Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
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