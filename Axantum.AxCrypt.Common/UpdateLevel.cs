using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common
{
    [Flags]
    public enum UpdateLevel
    {
        None = 0,
        Reliability = 1,
        Security = 2,
    }
}