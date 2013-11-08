using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
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

        public static IUIThread IUIThread
        {
            get { return FactoryRegistry.Instance.Singleton<IUIThread>(); }
        }

        public static IBackgroundWork IBackgroundWork
        {
            get { return FactoryRegistry.Instance.Singleton<IBackgroundWork>(); }
        }

        public static Background Background
        {
            get { return FactoryRegistry.Instance.Singleton<Background>(); }
        }

        public static IRuntimeEnvironment Environment
        {
            get { return FactoryRegistry.Instance.Singleton<IRuntimeEnvironment>(); }
        }
    }
}