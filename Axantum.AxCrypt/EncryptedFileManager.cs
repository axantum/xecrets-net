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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Properties;

namespace Axantum.AxCrypt
{
    /// <summary>
    /// Wrap IDisposable background processing resources in a Component and support ISupportInitialize, thus
    /// serving as wrapper for those resources and allowing them to work well with
    /// the designer.
    /// </summary>
    internal class EncryptedFileManager : Component, ISupportInitialize
    {
        public event EventHandler<EventArgs> Changed;

        public event EventHandler<VersionEventArgs> VersionChecked;

        public FileSystemState FileSystemState { get; private set; }

        private IFileWatcher _fileWatcher;

        private UpdateCheck _updateCheck;

        private bool _disposed = false;

        public EncryptedFileManager()
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

            string fileSystemStateFullName = Path.Combine(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName, "FileSystemState.xml"); //MLHIDE
            FileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(fileSystemStateFullName));
            FileSystemState.Changed += new EventHandler<EventArgs>(File_Changed);

            _fileWatcher = AxCryptEnvironment.Current.FileWatcher(AxCryptEnvironment.Current.TemporaryDirectoryInfo.FullName);
            _fileWatcher.FileChanged += new EventHandler<FileWatcherEventArgs>(File_Changed);

            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            Version newestVersion;
            if (!Version.TryParse(Settings.Default.NewestKnownVersion, out newestVersion))
            {
                newestVersion = UpdateCheck.VersionUnknown;
            }
            _updateCheck = new UpdateCheck(myVersion, newestVersion, Settings.Default.AxCrypt2VersionCheckUrl, Settings.Default.UpdateUrl);
            _updateCheck.VersionUpdate += new EventHandler<VersionEventArgs>(UpdateCheck_VersionUpdate);

            AxCryptEnvironment.Current.Changed += new EventHandler<EventArgs>(File_Changed);
        }

        private void UpdateCheck_VersionUpdate(object sender, VersionEventArgs e)
        {
            EventHandler<VersionEventArgs> handler = VersionChecked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void VersionCheckInBackground(DateTime lastUpdateCheckUtc)
        {
            _updateCheck.CheckInBackground(lastUpdateCheckUtc);
        }

        private void File_Changed(object sender, EventArgs e)
        {
            OnChanged(e);
        }

        public FileOperationStatus Open(string file, IEnumerable<AesKey> keys, ProgressContext progress)
        {
            return FileSystemState.OpenAndLaunchApplication(file, keys, progress);
        }

        public void RemoveRecentFile(string encryptedPath)
        {
            ActiveFile activeFile = FileSystemState.FindEncryptedPath(encryptedPath);
            FileSystemState.Remove(activeFile);
            FileSystemState.Save();
        }

        private void OnChanged(EventArgs eventArgs)
        {
            EventHandler<EventArgs> changed = Changed;
            if (changed != null)
            {
                changed(null, eventArgs);
            }
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
                if (_updateCheck != null)
                {
                    _updateCheck.Dispose();
                    _updateCheck = null;
                }
            }
            _disposed = true;
            base.Dispose(disposing);
        }
    }
}