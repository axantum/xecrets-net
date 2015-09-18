using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class KeyPairResponse : ResponseBase
    {
        public KeyPairResponse()
        {
            KeyPair = new KeyPair();
        }

        [JsonProperty("keypair")]
        public KeyPair KeyPair { get; private set; }
    }
}