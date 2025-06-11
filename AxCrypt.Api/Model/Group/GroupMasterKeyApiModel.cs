using AxCrypt.Api.Model.Masterkey;
using System.Text.Json.Serialization;
using System;

namespace AxCrypt.Api.Model.Groups
{
    public class GroupMasterKeyApiModel
    {
        public static GroupMasterKeyApiModel Empty { get; } = new GroupMasterKeyApiModel();

        [JsonPropertyName("groupId")]
        public long GroupId { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupName { get; set; }

        [JsonPropertyName("keyPair")]
        public MasterKeyPairInfo? KeyPair { get; set; }
    }
}
