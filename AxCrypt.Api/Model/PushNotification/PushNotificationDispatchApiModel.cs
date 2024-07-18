using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.PushNotification
{
    public class PushNotificationDispatchApiModel: BaseApiModel
    {
        public PushNotificationDispatchApiModel()
        {
        }

        [JsonPropertyName("pushNotificationId")]
        public long PushNotificationId { get; set; }

        [JsonPropertyName("dispatchedBy")]
        public string? DispatchedBy { get; set; }
    }
}
