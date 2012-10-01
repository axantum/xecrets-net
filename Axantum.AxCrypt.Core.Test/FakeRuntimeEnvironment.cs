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
using System.Collections.ObjectModel;
using System.IO;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.System;

namespace Axantum.AxCrypt.Core.Test
{
    internal class FakeRuntimeEnvironment : IRuntimeEnvironment
    {
        private byte _randomForTest = 0;

        private bool _isLittleEndian = BitConverter.IsLittleEndian;

        private Dictionary<string, FakeFileWatcher> _fileWatchers = new Dictionary<string, FakeFileWatcher>();

        public Func<DateTime> TimeFunction { get; set; }

        public FakeRuntimeEnvironment()
        {
            AxCryptExtension = ".axx";
            TimeFunction = StandardTimeFunction;
            Platform = Platform.WindowsDesktop;
            CurrentTiming = new FakeTiming();
        }

        public FakeRuntimeEnvironment(Endian endianness)
            : this()
        {
            _isLittleEndian = endianness == Endian.Reverse ? !_isLittleEndian : _isLittleEndian;
        }

        private static DateTime StandardTimeFunction()
        {
            return DateTime.UtcNow;
        }

        public bool IsLittleEndian
        {
            get { return _isLittleEndian; }
        }

        public byte[] GetRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; ++i)
            {
                bytes[i] = _randomForTest++;
            }
            return bytes;
        }

        public IRuntimeFileInfo FileInfo(string path)
        {
            return new FakeRuntimeFileInfo(path);
        }

        public string AxCryptExtension
        {
            get;
            set;
        }

        public Platform Platform
        {
            get;
            set;
        }

        public int StreamBufferSize
        {
            get { return 512; }
        }

        public IFileWatcher FileWatcher(string path)
        {
            FakeFileWatcher fileWatcher;
            if (_fileWatchers.TryGetValue(path, out fileWatcher))
            {
                return fileWatcher;
            }

            fileWatcher = new FakeFileWatcher(path);
            _fileWatchers.Add(path, fileWatcher);
            return fileWatcher;
        }

        private IRuntimeFileInfo _temporaryDirectoryInfo;

        public IRuntimeFileInfo TemporaryDirectoryInfo
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), "AxCrypt" + Path.DirectorySeparatorChar);
                    IRuntimeFileInfo temporaryFolderInfo = FileInfo(temporaryFolderPath);
                    temporaryFolderInfo.CreateDirectory();
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }

                return _temporaryDirectoryInfo;
            }
        }

        internal void FileCreated(string path)
        {
            HandleFileChanged(path);
        }

        internal void FileDeleted(string path)
        {
            HandleFileChanged(path);
        }

        internal void FileMoved(string path)
        {
            HandleFileChanged(path);
        }

        private void HandleFileChanged(string path)
        {
            foreach (FakeFileWatcher fileWatcher in _fileWatchers.Values)
            {
                if (Path.GetDirectoryName(path).StartsWith(fileWatcher.Path, StringComparison.Ordinal))
                {
                    fileWatcher.OnChanged(new FileWatcherEventArgs(path));
                }
            }
        }

        public DateTime UtcNow
        {
            get { return TimeFunction(); }
        }

        public Func<string, ILauncher> Launcher { get; set; }

        public ILauncher Launch(string path)
        {
            if (Launcher != null)
            {
                return Launcher(path);
            }
            return new FakeLauncher(path);
        }

        public ITiming StartTiming()
        {
            return CurrentTiming;
        }

        public FakeTiming CurrentTiming { get; set; }

        public Func<IWebCaller> WebCallerCreator { get; set; }

        public IWebCaller CreateWebCaller()
        {
            return WebCallerCreator();
        }

        public void NotifyFileChanged()
        {
            OnChanged();
        }

        protected virtual void OnChanged()
        {
            EventHandler<EventArgs> handler = FileChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<EventArgs> FileChanged;
    }
}