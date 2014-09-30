using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Crypto.Asymmetric
{
    public class BouncyCastleAsymmetricFactory : IAsymmetricFactory
    {
        public JsonConverter[] GetConverters()
        {
            JsonConverter[] jsonConverters = new JsonConverter[]
            {
                new AbstractJsonConverter<IAsymmetricPublicKey, BouncyCastlePublicKey>(),
                new AbstractJsonConverter<IAsymmetricPrivateKey, BouncyCastlePrivateKey>(),
                new AbstractJsonConverter<IAsymmetricKeyPair, BouncyCastleKeyPair>(),
            };
            return jsonConverters;
        }

        public IAsymmetricPrivateKey CreatePrivateKey(string privateKeyPem)
        {
            return new BouncyCastlePrivateKey(privateKeyPem);
        }

        public IAsymmetricPublicKey CreatePublicKey(string publicKeyPem)
        {
            return new BouncyCastlePublicKey(publicKeyPem);
        }

        public IAsymmetricKeyPair CreateKeyPair(int bits)
        {
            return new BouncyCastleKeyPair(bits);
        }

        public IAsymmetricKeyPair CreateKeyPair(byte[] n, byte[] e, byte[] d, byte[] p, byte[] q, byte[] dp, byte[] dq, byte[] qinv)
        {
            return new BouncyCastleKeyPair(n, e, d, p, q, dp, dq, qinv);
        }

        public IPaddingHash CreatePaddingHash()
        {
            return new BouncyCastlePaddingHash();
        }
    }
}