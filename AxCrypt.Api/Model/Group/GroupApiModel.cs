using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    public class GroupApiModel : BaseApiModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("busSubsId")]
        public string? BusSubsId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("creator")]
        public string? Creator { get; set; }

        [JsonPropertyName("masterKeyEnabled")]
        public bool MasterKeyEnabled { get; set; }

        [JsonPropertyName("groupKey")]
        public GroupKeyApiModel? GroupKey { get; set; }

        [JsonPropertyName("members")]
        public IEnumerable<GroupMemberApiModel>? Members { get; set; }
    }
}
