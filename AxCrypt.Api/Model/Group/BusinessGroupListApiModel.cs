using Newtonsoft.Json;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BusinessGroupListApiModel
    {
        [JsonProperty("busSubsId")]
        public string BusSubsId { get; set; }

        [JsonProperty("Groups")]
        public IList<GroupApiModel> Groups { get; set; }
    }
}