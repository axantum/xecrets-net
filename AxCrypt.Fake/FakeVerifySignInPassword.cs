using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Fake
{
    public class FakeVerifySignInPassword : IVerifySignInPassword
    {
        public bool Verify(string description)
        {
            return true;
        }
    }
}