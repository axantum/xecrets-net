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

using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Axantum.AxCrypt.Mono
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    internal class FileWatcher : IFileWatcher
    {
        private FileSystemWatcher _fileSystemWatcher;

        public FileWatcher(string path)
        {
            _fileSystemWatcher = new FileSystemWatcher(path);
            _fileSystemWatcher.Changed += TemporaryDirectoryWatcher_Changed;
            _fileSystemWatcher.Created += TemporaryDirectoryWatcher_Changed;
            _fileSystemWatcher.Deleted += TemporaryDirectoryWatcher_Changed;
            _fileSystemWatcher.Renamed += new RenamedEventHandler(_temporaryDirectoryWatcher_Renamed);
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.Filter = String.Empty;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            _fileSystemWatcher.EnableRaisingEvents = true;
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
                if (_fileSystemWatcher != null)
                {
                    _fileSystemWatcher.Dispose();
                }
                _fileSystemWatcher = null;
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