using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    public class WatchedFolderChangedEventArgs: EventArgs
    {
        public WatchedFolderChangedEventArgs(IEnumerable<WatchedFolder> added, IEnumerable<WatchedFolder> removed)
        {
            Added = new List<WatchedFolder>(added);
            Removed = new List<WatchedFolder>(removed);
        }

        public IEnumerable<WatchedFolder> Added { get; private set; }

        public IEnumerable<WatchedFolder> Removed { get; private set; }
    }
}
