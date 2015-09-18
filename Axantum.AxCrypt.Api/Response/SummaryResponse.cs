using Axantum.AxCrypt.Api.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SummaryResponse : ResponseBase
    {
        public SummaryResponse(UserSummary summary)
        {
            Summary = summary;
        }

        [JsonProperty("S")]
        public UserSummary Summary { get; private set; }
    }
}