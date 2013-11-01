using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt
{
    internal interface IMainView
    {
        FileSystemState FileSystemState { get; }

        ListView WatchedFolders { get; }

        TabControl Tabs { get; }
    }
}