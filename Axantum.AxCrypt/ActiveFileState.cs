using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    public static class ActiveFileState
    {
        private static IList<ActiveFile> _activeFiles = new List<ActiveFile>();

        public static IList<ActiveFile> ActiveFiles
        {
            get { return _activeFiles; }
        }
    }
}