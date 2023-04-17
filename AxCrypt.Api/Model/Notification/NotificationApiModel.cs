using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Notification
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NotificationApiModel
    {
        public static NotificationApiModel Empty = new NotificationApiModel(0, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public NotificationApiModel(long id, string userEmail, string content, string eventType, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
        {
            Id = id;
            UserEmail = userEmail;
            Content = content;
            EventType = eventType;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_email")]
        public string UserEmail { get; set; }

        [JsonProperty("content")]
        public string Content { get; } = string.Empty;

        [JsonProperty("eventType")]
        public string EventType { get; } = string.Empty;

        [JsonProperty("created_utc")]
        public DateTime CreatedUtc { get; set; }

        [JsonProperty("updated_utc")]
        public DateTime UpdatedUtc { get; set; }

        [JsonProperty("deleted_utc")]
        public DateTime? DeletedUtc { get; set; }
    }
}