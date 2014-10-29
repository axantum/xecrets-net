using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
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