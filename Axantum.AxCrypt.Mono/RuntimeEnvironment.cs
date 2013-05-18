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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace Axantum.AxCrypt.Mono
{
    public class RuntimeEnvironment : IRuntimeEnvironment, IDisposable
    {
        private static readonly TimeSpan _defaultWorkFolderStateMinimumIdle = new TimeSpan(0, 0, 1);

        private IFileWatcher _workFolderWatcher;

        private DelayedAction _delayedWorkFolderStateChanged;

        public RuntimeEnvironment(TimeSpan workFolderStateMinimumIdle)
            : this(null, workFolderStateMinimumIdle)
        {
        }

        public RuntimeEnvironment(ISynchronizeInvoke synchronizingObject)
            : this(synchronizingObject, _defaultWorkFolderStateMinimumIdle)
        {
        }

        public RuntimeEnvironment(ISynchronizeInvoke synchronizingObject, TimeSpan workFolderStateMinimumIdle)
            : this(".axx", synchronizingObject, workFolderStateMinimumIdle)
        {
        }

        public RuntimeEnvironment(string extension, ISynchronizeInvoke synchronizingObject, TimeSpan workFolderStateMinimumIdle)
        {
            AxCryptExtension = extension;

            _workFolderWatcher = CreateFileWatcher(WorkFolder.FullName);
            _workFolderWatcher.FileChanged += HandleWorkFolderFileChangedEvent;

            _delayedWorkFolderStateChanged = new DelayedAction(OnWorkFolderStateChanged, workFolderStateMinimumIdle, synchronizingObject);
        }

        private void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            NotifyWorkFolderStateChanged();
        }

        public bool IsLittleEndian
        {
            get
            {
                return BitConverter.IsLittleEndian;
            }
        }

        private RandomNumberGenerator _rng;

        public byte[] GetRandomBytes(int count)
        {
            if (_rng == null)
            {
                _rng = RandomNumberGenerator.Create();
            }

            byte[] data = new byte[count];
            _rng.GetBytes(data);
            return data;
        }

        public IRuntimeFileInfo FileInfo(string path)
        {
            return new RuntimeFileInfo(path);
        }

        public string AxCryptExtension
        {
            get;
            set;
        }

        public Platform Platform
        {
            get
            {
                OperatingSystem os = global::System.Environment.OSVersion;
                PlatformID pid = os.Platform;
                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                        return Platform.WindowsDesktop;
                    case PlatformID.MacOSX:
                        return Platform.MacOsx;
                    case PlatformID.Unix:
                        return Platform.Linux;
                    case PlatformID.WinCE:
                        return Platform.WindowsMobile;
                    case PlatformID.Xbox:
                        return Platform.Xbox;
                    default:
                        return Platform.Unknown;
                }
            }
        }

        public int StreamBufferSize
        {
            get { return 65536; }
        }

        public IFileWatcher CreateFileWatcher(string path)
        {
            return new FileWatcher(path);
        }

        private IRuntimeFileInfo _temporaryDirectoryInfo;

        public IRuntimeFileInfo WorkFolder
        {
            get
            {
                if (_temporaryDirectoryInfo == null)
                {
                    string temporaryFolderPath = Path.Combine(Path.GetTempPath(), @"AxCrypt" + Path.DirectorySeparatorChar);
                    IRuntimeFileInfo temporaryFolderInfo = FileInfo(temporaryFolderPath);
                    temporaryFolderInfo.CreateFolder();
                    _temporaryDirectoryInfo = temporaryFolderInfo;
                }

                return _temporaryDirectoryInfo;
            }
        }

        public DateTime UtcNow
        {
            get { return DateTime.UtcNow; }
        }

        public ILauncher Launch(string path)
        {
            return new Launcher(path);
        }

        public ITiming StartTiming()
        {
            return new Timing();
        }

        public IWebCaller CreateWebCaller()
        {
            return new WebCaller();
        }

        private ILogging _logging = null;

        public ILogging Log
        {
            get
            {
                if (_logging == null)
                {
                    _logging = new Logging();
                }
                return _logging;
            }
        }

        public void NotifyWorkFolderStateChanged()
        {
            _delayedWorkFolderStateChanged.RestartIdleTimer();
        }

        protected virtual void OnWorkFolderStateChanged()
        {
            EventHandler<EventArgs> handler = WorkFolderStateChanged;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        public event EventHandler<EventArgs> WorkFolderStateChanged;

        public IDataProtection DataProtection
        {
            get { return new DataProtection(); }
        }

        public bool CanTrackProcess
        {
            get { return Platform == Platform.WindowsDesktop; }
        }

        private long _keyWrapIterations = 0;

        public long KeyWrapIterations
        {
            get
            {
                if (_keyWrapIterations == 0)
                {
                    _keyWrapIterations = KeyWrapIterationCalculator.CalculatedKeyWrapIterations;
                }
                return _keyWrapIterations;
            }
            set
            {
                _keyWrapIterations = value;
            }
        }

        public KeyWrapSalt ThumbprintSalt
        {
            get;
            set;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_workFolderWatcher != null)
            {
                _workFolderWatcher.Dispose();
                _workFolderWatcher = null;
            }
            if (_delayedWorkFolderStateChanged != null)
            {
                _delayedWorkFolderStateChanged.Dispose();
                _delayedWorkFolderStateChanged = null;
            }
        }
    }
}