using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupKeyApiModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("group")]
        public long Group { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("thumbprint")]
        public string Thumbprint { get; set; }

        [JsonProperty("public")]
        public string Public { get; set; }
    }
}