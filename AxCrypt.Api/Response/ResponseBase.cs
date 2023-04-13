using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace AxCrypt.Api.Response
{
    public abstract class ResponseBase
    {
        /// <summary>
        /// Status. 0 Means success, everything else is an error.
        /// </summary>
        [JsonPropertyName("S")]
        public int Status { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        [JsonPropertyName("M")]
        public string Message { get; set; } = string.Empty;
    }
}
