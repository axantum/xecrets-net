using Axantum.AxCrypt.Core.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Axantum.AxCrypt.Presentation
{
    public interface IMainView
    {
        FileSystemState FileSystemState { get; }

        ListView WatchedFolders { get; }

        ListView RecentFiles { get; }

        TabControl Tabs { get; }

        IContainer Components { get; }
    }
}