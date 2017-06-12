#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Runtime
{
    public class WorkFolderWatcher : IDisposable
    {
        private IFileWatcher _workFolderWatcher;

        public WorkFolderWatcher()
        {
            _workFolderWatcher = New<IFileWatcher>(New<WorkFolder>().FileInfo.FullName);
            _workFolderWatcher.IncludeSubdirectories = true;
            _workFolderWatcher.FileChanged += HandleWorkFolderFileChangedEvent;
        }

        private async void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            foreach (string fullName in e.FullNames)
            {
                if (fullName == Resolve.FileSystemState.PathInfo.FullName)
                {
                    continue;
                }

                await Resolve.SessionNotify.NotifyAsync(new SessionNotification(SessionNotificationType.WorkFolderChange, fullName));
            }
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