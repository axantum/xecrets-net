using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Algorithm
{
    public enum PaddingMode
    {
        None = 1,
        PKCS7,
        Zeros,
        ANSIX923,
        ISO10126
    }
}