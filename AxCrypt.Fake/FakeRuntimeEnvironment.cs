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
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Fake
{
    public class FakeRuntimeEnvironment : IRuntimeEnvironment, IDisposable
    {
        private readonly bool _isLittleEndian = BitConverter.IsLittleEndian;

        public FakeRuntimeEnvironment()
        {
            AxCryptExtension = ".axx";
            Platform = Platform.WindowsDesktop;
            CurrentTiming = new FakeTiming();
            EnvironmentVariables = new Dictionary<string, string>();
            MaxConcurrency = -1;
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
            get { return (FakeRuntimeEnvironment)New<IRuntimeEnvironment>(); }
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

        public Func<string, ILauncher> Launcher { get; set; } = (path) => null!;

        public ITiming StartTiming()
        {
            return CurrentTiming;
        }

        public FakeTiming CurrentTiming { get; set; }

        public bool CanTrackProcess
        {
            get { return false; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public IDictionary<string, string> EnvironmentVariables { get; private set; }

        public string EnvironmentVariable(string name)
        {
            if (!EnvironmentVariables.TryGetValue(name, out string? variable))
            {
                return string.Empty;
            }
            return variable;
        }

        public int MaxConcurrency { get; set; }

        private bool _isFirstInstanceReady = false;

        public bool IsFirstInstanceReady(TimeSpan timeout)
        {
            return _isFirstInstanceReady;
        }

        public void FirstInstanceIsReady()
        {
            _isFirstInstanceReady = true;
        }

        public bool IsFirstInstance { get; set; }

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

        public void RunApp(string arguments)
        {
            throw new NotImplementedException();
        }

        private class FakeSynchronizationContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object? state)
            {
                callback(state);
            }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return new FakeSynchronizationContext(); }
        }

        public string AppPath { get; set; } = string.Empty;
    }
}
