using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public interface IFileWatcher : IDisposable
    {
        event EventHandler<FileWatcherEventArgs> FileChanged;
    }
}