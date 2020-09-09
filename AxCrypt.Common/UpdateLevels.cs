using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    [Flags]
    public enum UpdateLevels
    {
        None = 0,
        Reliability = 1,
        Security = 2,
    }
}