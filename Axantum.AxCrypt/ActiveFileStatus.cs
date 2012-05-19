using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt
{
    [Flags]
    public enum ActiveFileStatus
    {
        AssumedOpenAndDecrypted = 1,
        NotDecrypted = 2,
        Error = 4,
        DecryptedIsPendingDelete = 8,
        NotShareable = 16,
    }
}