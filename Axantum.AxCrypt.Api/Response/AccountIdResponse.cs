using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountIdResponse : ResponseBase
    {
        public AccountIdResponse()
        {
            AccountId = AccountKey.Empty;
        }

        [JsonProperty("id")]
        public AccountKey AccountId { get; private set; }
    }
}