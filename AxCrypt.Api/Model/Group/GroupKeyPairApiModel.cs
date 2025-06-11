using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    public class GroupKeyPairApiModel : GroupKeyApiModel
    {
        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("private")]
        public string? Private { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("memberEmail")]
        public string? MemberEmail { get; set; }
    }
}
