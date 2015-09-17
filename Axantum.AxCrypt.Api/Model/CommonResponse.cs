using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CommonResponse
    {
        /// <summary>
        /// Status. 0 Means success, everything else is an error.
        /// </summary>
        [JsonProperty("S")]
        public int Status { get; private set; }

        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty("M")]
        public string Message { get; private set; }
    }
}