using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.PushNotification
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationApiModel : BaseApiModel
    {
        public PushNotificationApiModel()
        {
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("body")]
        public string Body { get; set; } = string.Empty;

        [JsonProperty("subsLevel")]
        public string SubsLevel { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; } = string.Empty;

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonProperty("expirationUtc")]
        public DateTime? ExpirationUtc { get; set; }

        [JsonProperty("dispatchInfo")]
        public PushNotificationDispatchApiModel DispatchInfo { get; set; } = new PushNotificationDispatchApiModel();

        [JsonProperty("languages")]
        public string Languages { get; set; }
    }
}