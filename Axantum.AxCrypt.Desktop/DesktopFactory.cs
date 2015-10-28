using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Desktop
{
    public static class DesktopFactory
    {
        public static void RegisterTypeFactories()
        {
            TypeMap.Register.New<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(New<IDelayTimer>(), Resolve.UserSettings.SessionNotificationMinimumIdle)));
        }
    }
}