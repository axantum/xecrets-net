#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using System;
using System.Linq;
using System.Runtime.Serialization;

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

        public WatchedFolder(string path, AesKeyThumbprint thumbprint)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (thumbprint == null)
            {
                throw new ArgumentNullException("thumbprint");
            }

            Path = path;
            Thumbprint = thumbprint;
            Initialize(new StreamingContext());
        }

        public WatchedFolder(string fullName)
            : this(fullName, AesKeyThumbprint.Zero)
        {
        }

        public WatchedFolder(WatchedFolder watchedFolder)
        {
            Path = watchedFolder.Path;
            Thumbprint = watchedFolder.Thumbprint;
            Initialize(new StreamingContext());
        }

        [DataMember(Name = "Thumbprint")]
        public AesKeyThumbprint Thumbprint
        {
            get;
            private set;
        }

        [OnDeserialized]
        private void Initialize(StreamingContext context)
        {
            if (Factory.New<IRuntimeFileInfo>(Path).IsFolder)
            {
                _fileWatcher = Factory.New<IFileWatcher>(Path);
                _fileWatcher.FileChanged += _fileWatcher_FileChanged;
            }
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

            return String.Compare(Path, other.Path, StringComparison.OrdinalIgnoreCase) == 0;
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