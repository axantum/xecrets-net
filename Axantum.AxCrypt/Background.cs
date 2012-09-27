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
using System.ComponentModel;
using System.Reflection;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// Wrap IDisposable background processing resources in a Component and support ISupportInitialize, thus
    /// serving as wrapper for those resources and allowing them to work well with the designer.
    /// </summary>
    internal class Background : Component, ISupportInitialize
    {
        private IFileWatcher _fileWatcher;

        public UpdateCheck UpdateCheck { get; private set; }

        private bool _disposed = false;

        public Background()
        {
        }

        public void BeginInit()
        {
        }

        public void EndInit()
        {
            if (DesignMode)
            {
                return;
            }
            _fileWatcher = Os.Current.FileWatcher(Os.Current.TemporaryDirectoryInfo.FullName);
            _fileWatcher.FileChanged += new EventHandler<FileWatcherEventArgs>(File_Changed);

            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            UpdateCheck = new UpdateCheck(myVersion);
        }

        private void File_Changed(object sender, EventArgs e)
        {
            Os.Current.NotifyFileChanged();
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (_fileWatcher != null)
                {
                    _fileWatcher.Dispose();
                    _fileWatcher = null;
                }
                if (UpdateCheck != null)
                {
                    UpdateCheck.Dispose();
                    UpdateCheck = null;
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}