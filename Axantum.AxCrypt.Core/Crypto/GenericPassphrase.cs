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
            DerivationSalt = Salt.Zero;
            DerivationIterations = 1;
            DerivedKey = SymmetricKey.Zero128;
            Passphrase = passphrase;
            CryptoId = CryptoId.Unknown;
        }
    }
}