using AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Api.Response
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