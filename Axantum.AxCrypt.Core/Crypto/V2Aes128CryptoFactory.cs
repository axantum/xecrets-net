using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class V2Aes128CryptoFactory : ICryptoFactory
    {
        private static readonly Guid _id = CryptoFactory.Aes128Id;

        public IPassphrase CreatePassphrase(string passphrase)
        {
            return new V2Passphrase(passphrase, 128, Id);
        }

        public IPassphrase CreatePassphrase(string passphrase, Salt salt, int iterations)
        {
            return new V2Passphrase(passphrase, salt, iterations, 128, Id);
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