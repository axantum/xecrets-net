using AxCrypt.Abstractions;
using AxCrypt.Core;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Desktop
{
    public static class DesktopFactory
    {
        public static void RegisterTypeFactories()
        {
            TypeMap.Register.New<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(New<IDelayTimer>(), TimeSpan.FromMilliseconds(500))));
            TypeMap.Register.Singleton<ISettingsStore>(() => new SettingsStore(Resolve.WorkFolder.FileInfo.FileItemInfo("UserSettings.txt")));
        }
    }
}