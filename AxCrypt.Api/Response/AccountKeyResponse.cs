using AxCrypt.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace AxCrypt.Api.Response
{
    public class AccountKeyResponse : ResponseBase
    {
        public AccountKeyResponse()
        {
            KeyPair = new AccountKey[0];
        }

        [JsonPropertyName("keypair")]
        public IList<AccountKey> KeyPair { get; set; }
    }
}
