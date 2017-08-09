using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Fake
{
    public class FakeVerifySignInPassword : IVerifySignInPassword
    {
        public bool Verify(string description)
        {
            return true;
        }
    }
}