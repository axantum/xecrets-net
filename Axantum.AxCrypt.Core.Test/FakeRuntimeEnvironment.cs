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
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Axantum.AxCrypt.Core.Test
{
    public class FakeRuntimeEnvironment : IRuntimeEnvironment, IDisposable
    {
        private bool _isLittleEndian = BitConverter.IsLittleEndian;

        public Func<DateTime> TimeFunction { get; set; }

        public FakeRuntimeEnvironment()
        {
            AxCryptExtension = ".axx";
            TimeFunction = StandardTimeFunction;
            Platform = Platform.WindowsDesktop;
            CurrentTiming = new FakeTiming();
            EnvironmentVariables = new Dictionary<string, string>();
            MaxConcurrency = 2;
            IsFirstInstance = true;
            ExitCode = Int32.MinValue;
        }

        public FakeRuntimeEnvironment(Endian endianness)
            : this()
        {
            _isLittleEndian = endianness == Endian.Reverse ? !_isLittleEndian : _isLittleEndian;
        }

        public static FakeRuntimeEnvironment Instance
        {
            get { return (FakeRuntimeEnvironment)TypeMap.Resolve.Singleton<IRuntimeEnvironment>(); }
        }

        private static DateTime StandardTimeFunction()
        {
            return DateTime.UtcNow;
        }

        public bool IsLittleEndian
        {
            get { return _isLittleEndian; }
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

        public DateTime UtcNow
        {
            get { return TimeFunction(); }
        }

        public Func<string, ILauncher> Launcher { get; set; }

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

        public bool CanTrackProcess
        {
            get { return false; }
        }

        public void Dispose()
        {
        }

        public IDictionary<string, string> EnvironmentVariables { get; private set; }

        public string EnvironmentVariable(string name)
        {
            string variable;
            if (!EnvironmentVariables.TryGetValue(name, out variable))
            {
                return String.Empty;
            }
            return variable;
        }

        public int MaxConcurrency { get; set; }

        public bool IsFirstInstance { get; set; }

        public bool IsFirstInstanceRunning { get; set; }

        public bool FirstInstanceRunning(TimeSpan timeout)
        {
            return IsFirstInstanceRunning;
        }

        public int ExitCode { get; set; }

        public void ExitApplication(int exitCode)
        {
            if (ExitCode == Int32.MinValue)
            {
                ExitCode = exitCode;
            }
        }

        public bool IsDebugModeEnabled { get; private set; }

        public void DebugMode(bool enable)
        {
            IsDebugModeEnabled = enable;
        }

        private class FakeSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state)
            {
                callback(state);
            }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return new FakeSynchronizationContext(); }
        }
    }
}