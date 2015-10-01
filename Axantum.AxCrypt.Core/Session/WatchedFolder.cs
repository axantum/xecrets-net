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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;
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
    [JsonObject(MemberSerialization.OptIn)]
    public class WatchedFolder : IDisposable
    {
        [JsonProperty("path")]
        public string Path { get; private set; }

        private IFileWatcher _fileWatcher;

        public event EventHandler<FileWatcherEventArgs> Changed;

        [JsonConstructor]
        private WatchedFolder()
        {
            Tag = IdentityPublicTag.Empty;
        }

        public WatchedFolder(string path, IdentityPublicTag publicTag)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (publicTag == null)
            {
                throw new ArgumentNullException("publicTag");
            }

            Path = path.NormalizeFolderPath();
            Tag = publicTag;
            Initialize(new StreamingContext());
        }

        [JsonProperty("publicTag")]
        public IdentityPublicTag Tag
        {
            get;
            private set;
        }

        [OnDeserialized]
        private void Initialize(StreamingContext context)
        {
            if (TypeMap.Resolve.New<IDataContainer>(Path).IsAvailable)
            {
                _fileWatcher = TypeMap.Resolve.New<IFileWatcher>(Path);
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

        public bool Matches(string path)
        {
            return String.Compare(Path, path, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public bool Matches(WatchedFolder watchedFolder)
        {
            if (watchedFolder == null)
            {
                throw new ArgumentNullException("watchedFolder");
            }

            return Matches(watchedFolder.Path);
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