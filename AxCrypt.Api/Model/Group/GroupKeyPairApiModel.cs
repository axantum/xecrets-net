using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GroupKeyPairApiModel : GroupKeyApiModel
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("private")]
        public string Private { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("memberEmail")]
        public string MemberEmail { get; set; }
    }
}