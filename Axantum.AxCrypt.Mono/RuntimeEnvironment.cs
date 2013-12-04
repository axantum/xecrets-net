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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace Axantum.AxCrypt.Mono
{
    public class RuntimeEnvironment : IRuntimeEnvironment, IDisposable
    {
        private static readonly TimeSpan _defaultWorkFolderStateMinimumIdle = TimeSpan.FromMilliseconds(500);

        private IFileWatcher _workFolderWatcher;

        private DelayedAction _delayedWorkFolderStateChanged;

        public RuntimeEnvironment()
            : this(_defaultWorkFolderStateMinimumIdle)
        {
        }

        public RuntimeEnvironment(TimeSpan workFolderStateMinimumIdle)
            : this(".axx", workFolderStateMinimumIdle)
        {
        }

        public RuntimeEnvironment(string extension, TimeSpan workFolderStateMinimumIdle)
        {
            AxCryptExtension = extension;

            _workFolderWatcher = CreateFileWatcher(WorkFolder.FullName);
            _workFolderWatcher.FileChanged += HandleWorkFolderFileChangedEvent;

            _delayedWorkFolderStateChanged = new DelayedAction(OnWorkFolderStateChanged, workFolderStateMinimumIdle);
        }

        private void HandleWorkFolderFileChangedEvent(object sender, FileWatcherEventArgs e)
        {
            if (e.FullName == FileSystemState.DefaultPathInfo.FullName)
            {
                return;
            }
            NotifySessionChanged(new SessionEvent(SessionEventType.WorkFolderChange, e.FullName));
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
                    string temporaryFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"AxCrypt" + Path.DirectorySeparatorChar);
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

        private HashSet<SessionEvent> _sessionEvents = new HashSet<SessionEvent>();

        public void NotifySessionChanged(SessionEvent sessionEvent)
        {
            lock (_sessionEvents)
            {
                _sessionEvents.Add(sessionEvent);
            }
            _delayedWorkFolderStateChanged.RestartIdleTimer();
        }

        protected virtual void OnWorkFolderStateChanged()
        {
            EventHandler<SessionEventArgs> handler = SessionChanged;
            if (handler != null)
            {
                IEnumerable<SessionEvent> events;
                lock (_sessionEvents)
                {
                    events = new List<SessionEvent>(_sessionEvents);
                    _sessionEvents.Clear();
                }
                handler(this, new SessionEventArgs(events));
            }
        }

        public event EventHandler<SessionEventArgs> SessionChanged;

        public IDataProtection DataProtection
        {
            get { return new DataProtection(); }
        }

        public bool CanTrackProcess
        {
            get { return Platform == Platform.WindowsDesktop; }
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
            if (_delayedWorkFolderStateChanged != null)
            {
                _delayedWorkFolderStateChanged.Dispose();
                _delayedWorkFolderStateChanged = null;
            }
            if (_firstInstanceMutex != null)
            {
                _firstInstanceMutex.Close();
                _firstInstanceMutex = null;
            }
            if (_firstInstanceRunning != null)
            {
                _firstInstanceRunning.Close();
                _firstInstanceRunning = null;
            }
        }

        public string EnvironmentVariable(string name)
        {
            string variable = Environment.GetEnvironmentVariable(name);

            return variable;
        }

        public int MaxConcurrency
        {
            get
            {
                return Environment.ProcessorCount > 2 ? Environment.ProcessorCount - 1 : 2;
            }
        }

        private EventWaitHandle _firstInstanceRunning = new EventWaitHandle(false, EventResetMode.ManualReset, "Axantum.AxCrypt.NET-FirstInstanceRunning");

        private Mutex _firstInstanceMutex;

        private bool _isFirstInstance;

        public bool IsFirstInstance
        {
            get
            {
                if (_firstInstanceMutex == null)
                {
                    _firstInstanceMutex = new Mutex(true, "Axantum.AxCrypt.NET-FirstInstance", out _isFirstInstance);
                    if (_isFirstInstance)
                    {
                        _firstInstanceRunning.Set();
                    }
                }
                return _isFirstInstance;
            }
        }

        public bool FirstInstanceRunning(TimeSpan timeout)
        {
            return _firstInstanceRunning.WaitOne(timeout, false);
        }

        public void ExitApplication(int exitCode)
        {
            Environment.Exit(exitCode);
        }
    }
}