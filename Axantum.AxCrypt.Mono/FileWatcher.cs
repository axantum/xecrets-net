#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;

namespace Axantum.AxCrypt.Mono
{
    internal class FileWatcher : IFileWatcher
    {
        private FileSystemWatcher _temporaryDirectoryWatcher;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "The code has full trust anyway.")]
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
            if (OS.Log.IsInfoEnabled)
            {
                OS.Log.LogInfo("Watcher says '{0}' changed.".InvariantFormat(fullPath));
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