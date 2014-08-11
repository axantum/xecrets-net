using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    public interface IAsymmetricFactory
    {
        JsonConverter[] GetConverters();

        IAsymmetricPrivateKey CreatePrivateKey(string privateKeyPem);

        IAsymmetricPublicKey CreatePublicKey(string publicKeyPem);

        IAsymmetricKeyPair CreateKeyPair(int bits);

        IPaddingHash CreatePaddingHash();
    }
}