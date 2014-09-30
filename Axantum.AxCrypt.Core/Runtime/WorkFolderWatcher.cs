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
using Axantum.AxCrypt.Core.Session;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class WorkFolderWatcher : IDisposable
    {
        private IFileWatcher _workFolderWatcher;

        public WorkFolderWatcher()
        {
            _workFolderWatcher = TypeMap.Resolve.New<IFileWatcher>(TypeMap.Resolve.Singleton<WorkFolder>().FileInfo.FullName);
            _workFolderWatcher.IncludeSubdirectories = true;
            _workFolderWatcher.FileChanged += HandleWorkFolderFileChangedEvent;
        }

        private void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            if (e.FullName == Resolve.FileSystemState.PathInfo.FullName)
            {
                return;
            }
            Resolve.SessionNotify.Notify(new SessionNotification(SessionNotificationType.WorkFolderChange, e.FullName));
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