using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Core.UI
{
    public interface IVerifySignInPassword
    {
        bool Verify(string description);
    }
}