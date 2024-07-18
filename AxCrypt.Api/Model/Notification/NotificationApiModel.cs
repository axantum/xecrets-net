using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.Notification
{
    [JsonObject(MemberSerialization.OptIn)]
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

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("receiver")]
        public string Receiver { get; set; }

        [JsonProperty("content")]
        public string Content { get; } = string.Empty;

        [JsonProperty("eventType")]
        public string EventType { get; } = string.Empty;

        [JsonProperty("actor")]
        public string Actor { get; } = string.Empty;

        [JsonProperty("actionData")]
        public string ActionData { get; set; } = string.Empty;

        [JsonProperty("pushNotify")]
        public bool PushNotify { get; set; }

        [JsonProperty("pushNotified")]
        public DateTime? PushNotified { get; set; }
    }
}