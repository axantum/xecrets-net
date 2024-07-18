using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    public class GroupMemberApiModel : BaseApiModel
    {
        public GroupMemberApiModel()
        {
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("group")]
        public long Group { get; set; }

        [JsonPropertyName("userEmail")]
        public string? UserEmail { get; set; }

        [JsonPropertyName("role")]
        public long Role { get; set; }

        [JsonPropertyName("groups")]
        public IEnumerable<string>? Groups { get; set; }
    }
}
