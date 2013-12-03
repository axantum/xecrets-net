using Axantum.AxCrypt.Core.Ipc;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core
{
    public static class Instance
    {
        public static KnownKeys KnownKeys
        {
            get { return FactoryRegistry.Instance.Singleton<KnownKeys>(); }
        }

        public static IUIThread UIThread
        {
            get { return FactoryRegistry.Instance.Singleton<IUIThread>(); }
        }

        public static IProgressBackground BackgroundWork
        {
            get { return FactoryRegistry.Instance.Singleton<IProgressBackground>(); }
        }

        public static ParallelBackground ParallelBackground
        {
            get { return FactoryRegistry.Instance.Singleton<ParallelBackground>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(); }
        }

        public static FileSystemState FileSystemState
        {
            get { return FactoryRegistry.Instance.Singleton<FileSystemState>(); }
        }

        public static ProcessState ProcessState
        {
            get { return FactoryRegistry.Instance.Singleton<ProcessState>(); }
        }

        public static CommandService CommandService
        {
            get { return FactoryRegistry.Instance.Singleton<CommandService>(); }
        }

        public static IStatusChecker StatusChecker
        {
            get { return FactoryRegistry.Instance.Singleton<IStatusChecker>(); }
        }

        public static IUserSettings UserSettings
        {
            get { return FactoryRegistry.Instance.Singleton<IUserSettings>(); }
        }
    }
}