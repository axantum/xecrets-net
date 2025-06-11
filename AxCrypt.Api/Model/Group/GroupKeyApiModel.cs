using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    public class GroupKeyApiModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("group")]
        public long Group { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("thumbprint")]
        public string? Thumbprint { get; set; }

        [JsonPropertyName("public")]
        public string? Public { get; set; }
    }
}
