using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Core.UI
{
    public interface IVerifySignInPassword
    {
        bool Verify(string description);
    }
}