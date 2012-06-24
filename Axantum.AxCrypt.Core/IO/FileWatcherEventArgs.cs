using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileWatcherEventArgs : EventArgs
    {
        public string FullName { get; private set; }

        public FileWatcherEventArgs(string fullName)
        {
            FullName = fullName;
        }
    }
}