using Axantum.AxCrypt.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Crypto.Asymmetric;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Fake
{
    public class FakeLicenseAuthority : ILicenseAuthority
    {
        private IAsymmetricKeyPair _keyPair;

        public FakeLicenseAuthority()
        {
            _keyPair = New<IAsymmetricFactory>().CreateKeyPair(512);
        }

        public Task<IAsymmetricPrivateKey> PrivateKeyAsync()
        {
            return Task.FromResult(_keyPair.PrivateKey);
        }

        public Task<IAsymmetricPublicKey> PublicKeyAsync()
        {
            return Task.FromResult(_keyPair.PublicKey);
        }
    }
}