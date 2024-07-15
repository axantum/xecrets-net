using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Notification
{
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

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; } = string.Empty;

        [JsonPropertyName("eventType")]
        public string EventType { get; } = string.Empty;

        [JsonPropertyName("created_utc")]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("updated_utc")]
        public DateTime UpdatedUtc { get; set; }

        [JsonPropertyName("deleted_utc")]
        public DateTime? DeletedUtc { get; set; }
    }
}
