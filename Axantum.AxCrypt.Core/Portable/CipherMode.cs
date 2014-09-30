using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public enum CipherMode
    {
        None = 0,
        CBC,
        ECB,
        OFB,
        CFB,
        CTS,
    }
}