using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserInformationResponse : ResponseBase
    {
        public UserInformationResponse(UserInformation summary)
        {
            Summary = summary;
        }

        [JsonProperty("S")]
        public UserInformation Summary { get; private set; }
    }
}