using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    [Flags]
    public enum SecureFolderLevels
    {
        None = 0,
        SingleFolder = 1,
        IncludeSubFolders = 2,
    }
}