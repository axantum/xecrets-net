using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V2Aes128CryptoFactory : ICryptoFactory
    {
        private static readonly Guid _id = new Guid("2B0CCBB0-B978-4BC3-A293-F97585F06557");

        public IPassphrase CreatePassphrase(string passphrase)
        {
            return new V2Passphrase(passphrase, 128);
        }

        public IPassphrase CreatePassphrase(string passphrase, Salt salt, int iterations)
        {
            return new V2Passphrase(passphrase, salt, iterations, 128);
        }

        public ICrypto CreateCrypto(IPassphrase key)
        {
            return new V2AesCrypto(key, SymmetricIV.Zero128, 0);
        }

        public ICrypto CreateCrypto(IPassphrase key, SymmetricIV iv, long keyStreamOffset)
        {
            return new V2AesCrypto(key, iv, keyStreamOffset);
        }

        public int Priority
        {
            get { return 200000; }
        }

        public Guid Id
        {
            get { return _id; }
        }

        public string Name
        {
            get { return "AES-128"; }
        }
    }
}