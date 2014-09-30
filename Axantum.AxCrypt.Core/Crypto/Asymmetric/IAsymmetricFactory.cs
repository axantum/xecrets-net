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

        IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv);

        IPaddingHash CreatePaddingHash();
    }
}