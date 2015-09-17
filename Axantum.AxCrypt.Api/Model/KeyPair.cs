using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KeyPair
    {
        public KeyPair(string publicKey, string privateKey)
        {
            if (publicKey == null)
            {
                throw new ArgumentNullException("publicKey");
            }
            if (privateKey == null)
            {
                throw new ArgumentNullException("privateKey");
            }

            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        [JsonProperty("public")]
        public string PublicKey { get; private set; }

        [JsonProperty("private")]
        public string PrivateKey { get; private set; }
    }
}