using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.PushNotification
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PushNotificationDispatchApiModel: BaseApiModel
    {
        public PushNotificationDispatchApiModel()
        {
        }

        [JsonProperty("pushNotificationId")]
        public long PushNotificationId { get; set; }

        [JsonProperty("dispatchedBy")]
        public string DispatchedBy { get; set; }
    }
}