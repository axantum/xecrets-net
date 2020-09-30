using AxCrypt.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.UI
{
    public abstract class VerifySignInPasswordBase : IVerifySignInPassword
    {
        public bool Verify(string description)
        {
            if (!New<KnownIdentities>().IsLoggedOn)
            {
                return false;
            }

            bool isVerified = false;
            New<IUIThread>().SendTo(() => isVerified = VerifyDialog(description));
            return isVerified;
        }

        protected abstract bool VerifyDialog(string description);
    }
}