using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class GenericPassphrase : PassphraseBase
    {
        public GenericPassphrase(SymmetricKey key)
        {
            SetDeriviationSalt(new byte[0]);
            DerivationIterations = 1;
            DerivedKey = key;
            Passphrase = String.Empty;
        }
    }
}