using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Holds information about a folder that is watched for file changes, to enable
    /// automatic encryption of files for example. Instances of this class are
    /// immutable
    /// </summary>
    [DataContract(Namespace = "http://www.axantum.com/Serialization/")]
    public class WatchedFolder : IEquatable<WatchedFolder>, IDisposable
    {
        [DataMember(Name = "Path")]
        public string Path { get; private set; }

        private IFileWatcher _fileWatcher;

        public event EventHandler<FileWatcherEventArgs> Changed;

        public WatchedFolder(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            Path = path;
            Initialize(new StreamingContext());
        }

        public WatchedFolder(WatchedFolder watchedFolder)
        {
            Path = watchedFolder.Path;
            Initialize(new StreamingContext());
        }

        [OnDeserialized]
        private void Initialize(StreamingContext context)
        {
            _fileWatcher = OS.Current.CreateFileWatcher(Path);
            _fileWatcher.FileChanged += _fileWatcher_FileChanged;
        }

        private void _fileWatcher_FileChanged(object sender, FileWatcherEventArgs e)
        {
            OnChanged(e);
        }

        protected virtual void OnChanged(FileWatcherEventArgs e)
        {
            EventHandler<FileWatcherEventArgs> handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool Equals(WatchedFolder other)
        {
            if (other == null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            return Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            WatchedFolder watchedFolder = obj as WatchedFolder;
            if (watchedFolder == null)
            {
                return false;
            }

            return Equals(watchedFolder);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public static bool operator ==(WatchedFolder left, WatchedFolder right)
        {
            if ((object)left == null || ((object)right == null))
            {
                return Object.Equals(left, right);
            }

            return left.Equals(right);
        }

        public static bool operator !=(WatchedFolder left, WatchedFolder right)
        {
            return !(left == right);
        }

        #region IDisposable Members

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
            if (_fileWatcher != null)
            {
                _fileWatcher.Dispose();
                _fileWatcher = null;
            }
        }

        #endregion IDisposable Members
    }
}