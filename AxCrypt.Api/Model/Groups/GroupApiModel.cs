using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupApiModel : BaseApiModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("busSubsId")]
        public string BusSubsId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("masterKeyEnabled")]
        public bool MasterKeyEnabled { get; set; }

        [JsonProperty("members")]
        public IEnumerable<GroupMemberApiModel> Members { get; set; }
    }
}