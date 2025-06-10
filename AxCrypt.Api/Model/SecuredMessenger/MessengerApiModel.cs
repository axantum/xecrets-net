using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.SecuredMessenger
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SecuredMessengerRootApiModel
    {
        [JsonProperty("message")]
        public MessengerApiModel Message { get; set; }

        [JsonProperty("replies")]
        public IEnumerable<MessengerApiModel> Replies { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MessengerApiModel : BaseApiModel
    {
        public MessengerApiModel()
        {
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("messageId")]
        public Guid MessageId { get; set; }

        [JsonProperty("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonProperty("receiver")]
        public IEnumerable<MessengerReceiverApiModel> Receiver { get; set; } = new List<MessengerReceiverApiModel>();

        [JsonProperty("visibility")]
        public string Visibility { get; set; } = string.Empty;

        [JsonProperty("visibleuntil")]
        public DateTime VisibleUntil { get; set; }

        [JsonProperty("encryptedMessage")]
        public string EncryptedMessage { get; set; } = string.Empty;

        [JsonProperty("parentid")]
        public Guid ParentId { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class MessengerReceiverApiModel
    {
        [JsonProperty("user")]
        public string User { get; set; } = "";

        [JsonProperty("read")]
        public DateTime Read { get; set; } = DateTime.MinValue;
    }
}