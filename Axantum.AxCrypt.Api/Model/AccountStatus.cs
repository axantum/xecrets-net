using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    public enum AccountStatus
    {
        Unknown,
        Unverified,
        Verified,
        Offline,
        Unauthenticated,
        DefinedByServer,
        NotFound,
    }
}