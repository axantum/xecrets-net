using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupMemberApiModel : BaseApiModel
    {
        public GroupMemberApiModel()
        {
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("group")]
        public long Group { get; set; }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("role")]
        public long Role { get; set; }

        [JsonProperty("groups")]
        public IEnumerable<string> Groups { get; set; }
    }
}