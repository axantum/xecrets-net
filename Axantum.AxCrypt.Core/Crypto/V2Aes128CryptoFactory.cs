using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V2Aes128CryptoFactory : ICryptoFactory
    {
        public static readonly Guid CryptoId = new Guid("2B0CCBB0-B978-4BC3-A293-F97585F06557");

        public IDerivedKey CreateDerivedKey(Passphrase passphrase)
        {
            return new V2DerivedKey(passphrase, 128, Id);
        }

        public IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations)
        {
            return new V2DerivedKey(passphrase, salt, derivationIterations, 128, Id);
        }

        public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V2AesCrypto(this, key, iv, keyStreamOffset);
        }

        public int Priority
        {
            get { return 200000; }
        }

        public Guid Id
        {
            get { return CryptoId; }
        }

        public string Name
        {
            get { return "AES-128"; }
        }
    }
}