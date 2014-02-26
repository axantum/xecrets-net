using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class GenericPassphrase : PassphraseBase
    {
        public GenericPassphrase(SymmetricKey key)
            : this((string)null)
        {
            DerivedKey = key;
        }

        public GenericPassphrase(string passphrase)
        {
            SetDerivationSalt(new byte[0]);
            DerivationIterations = 1;
            DerivedKey = SymmetricKey.Zero128;
            Passphrase = passphrase;
            CryptoName = CryptoName.Unknown;
        }
    }
}