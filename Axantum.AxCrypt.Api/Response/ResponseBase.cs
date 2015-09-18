using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Response
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ResponseBase
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