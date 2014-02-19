using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class GenericPassphrase : PassphraseBase
    {
        public GenericPassphrase(SymmetricKey key)
        {
            DerivedKey = key;
            Passphrase = String.Empty;
        }
    }
}