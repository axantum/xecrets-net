using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.UI.ViewModel
{
    public enum ValidationError
    {
        None = 0,
        VerificationPassphraseWrong,
        IdentityExistsAlready,
    }
}