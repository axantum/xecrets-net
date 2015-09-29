using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAccountResponse : ResponseBase
    {
        public UserAccountResponse(UserAccount summary)
        {
            UserAccount = summary;
        }

        [JsonProperty("S")]
        public UserAccount UserAccount { get; private set; }
    }
}