using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public class WhatIPResponse : ResponseBase
    {
        public WhatIPResponse()
            : this(string.Empty)
        {
        }

        public WhatIPResponse(string address)
        {
            Message = "OK";
            IPAddress = address;
        }

        [JsonProperty("Ip")]
        public string IPAddress { get; }
    }
}