using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class GenericPassphrase : PassphraseBase
    {
        public GenericPassphrase(SymmetricKey key)
            : this(Passphrase.Empty)
        {
            DerivedKey = key;
        }

        public GenericPassphrase(Passphrase passphrase)
        {
            DerivationSalt = Salt.Zero;
            DerivationIterations = 1;
            DerivedKey = SymmetricKey.Zero128;
            Passphrase = passphrase;
        }

        public override SymmetricKeyThumbprint Thumbprint
        {
            get
            {
                throw new NotImplementedException("The Thumprint property cannot be used on a GenericPassphrase instance.");
            }
        }
    }
}