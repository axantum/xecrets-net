using System;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    public class V2Aes128CryptoFactory : ICryptoFactory
    {
        private static readonly Guid CRYPTOID = new Guid("2B0CCBB0-B978-4BC3-A293-F97585F06557");

        public IDerivedKey CreateDerivedKey(Passphrase passphrase)
        {
            return new V2DerivedKey(passphrase, 128);
        }

        public IDerivedKey RestoreDerivedKey(Passphrase passphrase, Salt salt, int derivationIterations)
        {
            return new V2DerivedKey(passphrase, salt, derivationIterations, 128);
        }

        public ICrypto CreateCrypto(SymmetricKey key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V2AesCrypto(key, iv, keyStreamOffset);
        }

        public int Priority
        {
            get { return 200000; }
        }

        public Guid CryptoId
        {
            get { return CRYPTOID; }
        }

        public string Name
        {
            get { return "AES-128"; }
        }

        public int KeySize
        {
            get { return 128; }
        }

        public int BlockSize
        {
            get { return 128; }
        }
    }
}