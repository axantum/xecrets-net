using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Common
{
    [Flags]
    public enum AppTypes
    {
        None = 0,
        AxCryptIos2 = 1,
        AxCryptAndroid2 = 2,
        AxCryptWindowsPhone2 = 4,
        AxCryptWindows2 = 8,
        AxCryptMac2 = 16,
        AxCryptLinux2 = 32,
        AxCryptAccountWeb = 64,
    }
}