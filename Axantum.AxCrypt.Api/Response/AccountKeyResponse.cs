using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountKeyResponse : ResponseBase
    {
        public AccountKeyResponse()
        {
            KeyPair = new AccountKey[0];
        }

        [JsonProperty("keypair")]
        public IList<AccountKey> KeyPair { get; private set; }
    }
}