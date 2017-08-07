using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WhatIPResponse : ResponseBase
    {
        public WhatIPResponse()
            : this(string.Empty)
        {
        }

        public WhatIPResponse(string ipAddress)
        {
            Message = "OK";
            IPAddress = ipAddress;
        }

        [JsonProperty("Ip")]
        public string IPAddress { get; }
    }
}