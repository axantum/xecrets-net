using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model
{
    public class BaseApiModel
    {
        [JsonPropertyName("createdUtc")]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("updatedUtc")]
        public DateTime UpdatedUtc { get; set; }

        [JsonPropertyName("deletedUtc")]
        public DateTime? DeletedUtc { get; set; }
    }
}
