using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.UI;
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
        ListView WatchedFolders { get; }

        ListView RecentFiles { get; }

        TabControl Tabs { get; }

        IContainer Components { get; }

        Control Control { get; }

        DragDropEffects GetDragDropEffects(object sender, DragEventArgs e);
    }
}