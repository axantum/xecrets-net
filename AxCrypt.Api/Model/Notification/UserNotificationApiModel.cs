using AxCrypt.Api.Model;
using AxCrypt.Api.Model.PushNotification;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model.Notification
{
    public class UserNotificationApiModel : NotificationApiModel
    {
        public UserNotificationApiModel(long id, string receiver, string actor, string content, string eventType, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc) 
            : base(id, receiver, actor, content, eventType, createdUtc, updatedUtc, deletedUtc)
        {
        }

        [JsonPropertyName("pushnotif_title")]
        public string PushNotif_Title { get; set; } = string.Empty;

        [JsonPropertyName("pushnotif_subsLevel")]
        public string? PushNotif_SubsLevel { get; set; }

        [JsonPropertyName("pushnotif_category")]
        public string? PushNotif_Category { get; set; }

        [JsonPropertyName("pushnotif_imageUrl")]
        public string PushNotif_ImageUrl { get; set; } = string.Empty;
    }
}
