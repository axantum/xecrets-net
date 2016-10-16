using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WhatIpResponse : ResponseBase
    {
        public WhatIpResponse()
            : this(string.Empty)
        {
        }

        public WhatIpResponse(string ipAddress)
        {
            Message = "OK";
            IpAddress = ipAddress;
        }

        [JsonProperty("Ip")]
        public string IpAddress { get; }
    }
}