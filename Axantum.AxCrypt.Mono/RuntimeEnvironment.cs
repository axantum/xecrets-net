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
using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Mono.Portable;
using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Mono
{
    public class RuntimeEnvironment : IRuntimeEnvironment, IDisposable
    {
        public static void RegisterTypeFactories()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<ILogging>(() => new Logging());
            TypeMap.Register.Singleton<CommandService>(() => new CommandService(new HttpRequestServer(), new HttpRequestClient()));
            TypeMap.Register.Singleton<IPlatform>(() => new MonoPlatform());

            TypeMap.Register.New<ISleep>(() => new Sleep());
            TypeMap.Register.New<IDelayTimer>(() => new DelayTimer());
            TypeMap.Register.New<string, IDataStore>((path) => new DataStore(path));
            TypeMap.Register.New<string, IDataContainer>((path) => new DataContainer(path));
            TypeMap.Register.New<string, IDataItem>((path) => DataItem.Create(path));
            TypeMap.Register.New<IRestCaller>(() => new RestCaller());
            TypeMap.Register.New<ISingleThread>(() => new SingleThread());
        }

        public RuntimeEnvironment(string extension)
        {
            AxCryptExtension = extension;
        }

        public bool IsLittleEndian
        {
            get
            {
                return BitConverter.IsLittleEndian;
            }
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
                return New<IPlatform>().Platform;
            }
        }

        public int StreamBufferSize
        {
            get { return 65536; }
        }

        public ITiming StartTiming()
        {
            return new Timing();
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
            if (_firstInstanceMutex != null)
            {
                _firstInstanceMutex.Close();
                _firstInstanceMutex = null;
            }
            if (_firstInstanceReady != null)
            {
                _firstInstanceReady.Close();
                _firstInstanceReady = null;
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
                return -1;
            }
        }

        private EventWaitHandle _firstInstanceReady;

        private EventWaitHandle FirstInstanceEvent
        {
            get
            {
                if (_firstInstanceReady == null)
                {
                    _firstInstanceReady = new EventWaitHandle(false, EventResetMode.ManualReset, @"Local\Axantum.AxCrypt.NET-FirstInstanceReady");
                }
                return _firstInstanceReady;
            }
        }

        public bool IsFirstInstanceReady(TimeSpan timeout)
        {
            return FirstInstanceEvent.WaitOne(timeout, false);
        }

        public void FirstInstanceIsReady()
        {
            FirstInstanceEvent.Set();
        }

        private Mutex _firstInstanceMutex;

        private bool _isFirstInstance;

        public bool IsFirstInstance
        {
            get
            {
                if (_firstInstanceMutex == null)
                {
                    _firstInstanceMutex = new Mutex(true, @"Local\Axantum.AxCrypt.NET-FirstInstance", out _isFirstInstance);
                }
                return _isFirstInstance;
            }
            set
            {
                if (!IsFirstInstance && value)
                {
                    throw new InvalidOperationException("Can't claim first instance when someone else already is.");
                }
                if (IsFirstInstance && !value)
                {
                    _firstInstanceMutex.Close();
                    _firstInstanceMutex.Dispose();
                    _firstInstanceMutex = null;
                }
            }
        }

        public void ExitApplication(int exitCode)
        {
            Environment.Exit(exitCode);
        }

        public void DebugMode(bool enable)
        {
            if (enable)
            {
                ServicePointManager.ServerCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    return true;
                };
            }
            else
            {
                ServicePointManager.ServerCertificateValidationCallback = null;
            }
        }

        public SynchronizationContext SynchronizationContext
        {
            get
            {
                return SynchronizationContext.Current ?? new SynchronizationContext();
            }
        }
    }
}