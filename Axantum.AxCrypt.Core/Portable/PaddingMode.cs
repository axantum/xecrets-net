using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
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