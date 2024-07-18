using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.PushNotification
{
    public class PushNotificationApiModel : BaseApiModel
    {
        public PushNotificationApiModel()
        {
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;

        [JsonPropertyName("subsLevel")]
        public string? SubsLevel { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("expirationUtc")]
        public DateTime? ExpirationUtc { get; set; }

        [JsonPropertyName("dispatchInfo")]
        public PushNotificationDispatchApiModel DispatchInfo { get; set; } = new PushNotificationDispatchApiModel();

        [JsonPropertyName("languages")]
        public string? Languages { get; set; }
    }
}
