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

using Axantum.AxCrypt.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeFileWatcher : IFileWatcher
    {
        private bool disposed = false;

        private static List<KeyValuePair<string, FakeFileWatcher>> _fileWatchers = new List<KeyValuePair<string, FakeFileWatcher>>();

        internal string Path { get; set; }

        public FakeFileWatcher(string path)
        {
            Path = path;
            lock (_fileWatchers)
            {
                _fileWatchers.Add(new KeyValuePair<string, FakeFileWatcher>(path, this));
            }
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

        public static void HandleFileChanged(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            lock (_fileWatchers)
            {
                foreach (KeyValuePair<string, FakeFileWatcher> fileWatcher in _fileWatchers)
                {
                    if (fileWatcher.Value.disposed)
                    {
                        continue;
                    }
                    string key = fileWatcher.Key;
                    if (!key.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                    {
                        key += System.IO.Path.DirectorySeparatorChar;
                    }
                    if (((path.Substring(0, path.Length - System.IO.Path.GetFileName(path).Length))).StartsWith(key, StringComparison.Ordinal))
                    {
                        fileWatcher.Value.OnChanged(new FileWatcherEventArgs(path));
                    }
                }
            }
        }

        public bool IncludeSubdirectories
        {
            get;
            set;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            disposed = true;
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