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

        [JsonPropertyName("masterKeyStatus")]
        public string? MasterKeyStatus { get; set; }

        [JsonPropertyName("disableMasterKeyAccess")]
        public bool DisableMasterKeyAccess { get; set; }

        [JsonPropertyName("groupPrivateKey")]
        public string? GroupPrivateKey { get; set; }

        [JsonPropertyName("disableAccess")]
        public bool DisableAccess { get; set; }

        [JsonPropertyName("groups")]
        public IEnumerable<string>? Groups { get; set; }
    }
}
