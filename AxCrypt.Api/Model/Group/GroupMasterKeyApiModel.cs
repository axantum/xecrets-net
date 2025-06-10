using AxCrypt.Api.Model.Masterkey;
using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class GroupMasterKeyApiModel
    {
        public static GroupMasterKeyApiModel Empty { get; } = new GroupMasterKeyApiModel();

        [JsonProperty("groupId")]
        public long GroupId { get; set; }

        [JsonProperty("groupName")]
        public string GroupName { get; set; }

        [JsonProperty("keyPair")]
        public MasterKeyPairInfo KeyPair { get; set; }
    }
}
