using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AxCrypt.Api.Response
{
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

        [JsonPropertyName("Ip")]
        public string IPAddress { get; set; }
    }
}
