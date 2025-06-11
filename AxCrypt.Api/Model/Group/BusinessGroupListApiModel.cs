using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Groups
{
    public class BusinessGroupListApiModel
    {
        [JsonPropertyName("busSubsId")]
        public string? BusSubsId { get; set; }

        [JsonPropertyName("Groups")]
        public IList<GroupApiModel>? Groups { get; set; }
    }
}
