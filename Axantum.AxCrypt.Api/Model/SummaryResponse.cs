using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SummaryResponse : CommonResponse
    {
        public SummaryResponse(UserSummary summary)
        {
            Summary = summary;
        }

        [JsonProperty("S")]
        public UserSummary Summary { get; private set; }
    }
}