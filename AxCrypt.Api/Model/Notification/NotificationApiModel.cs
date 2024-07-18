using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Notification
{
    public class NotificationApiModel : BaseApiModel
    {
        public static NotificationApiModel Empty = new NotificationApiModel(0, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public NotificationApiModel(long id, string receiver, string actor, string content, string eventType, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc)
        {
            Id = id;
            Receiver = receiver;
            Actor = actor;
            Content = content;
            EventType = eventType;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("receiver")]
        public string Receiver { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; } = string.Empty;

        [JsonPropertyName("eventType")]
        public string EventType { get; } = string.Empty;

        [JsonPropertyName("actor")]
        public string Actor { get; } = string.Empty;

        [JsonPropertyName("actionData")]
        public string ActionData { get; set; } = string.Empty;

        [JsonPropertyName("pushNotify")]
        public bool PushNotify { get; set; }

        [JsonPropertyName("pushNotified")]
        public DateTime? PushNotified { get; set; }
    }
}
