using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Axantum.AxCrypt.Core.Session;

namespace Axantum.AxCrypt
{
    interface IMainView
    {
        FileSystemState FileSystemState { get; }

        ListView WatchedFolders { get; }

        TabControl Tabs { get; }
    }
}
