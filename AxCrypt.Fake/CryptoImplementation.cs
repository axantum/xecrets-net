using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public enum CryptoImplementation
    {
        Unknown,
        Mono,
        WindowsDesktop,
        BouncyCastle
    }
}