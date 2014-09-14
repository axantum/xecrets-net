using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Desktop
{
    public class DesktopFactory
    {
        public static void RegisterTypeFactories()
        {
            Factory.Instance.Register<string, IFileWatcher>((path) => new FileWatcher(path, new DelayedAction(Factory.New<IDelayTimer>(), Instance.UserSettings.SessionNotificationMinimumIdle)));
        }
    }
}
