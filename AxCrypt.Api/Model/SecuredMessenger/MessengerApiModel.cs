using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;

namespace AxCrypt.Api.Model.SecuredMessenger
{
    public class SecuredMessengerRootApiModel
    {
        [JsonPropertyName("message")]
        public MessengerApiModel? Message { get; set; }

        [JsonPropertyName("replies")]
        public IEnumerable<MessengerApiModel>? Replies { get; set; }
    }

    public class MessengerApiModel : BaseApiModel
    {
        public MessengerApiModel()
        {
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("messageId")]
        public Guid MessageId { get; set; }

        [JsonPropertyName("sender")]
        public string Sender { get; set; } = string.Empty;

        [JsonPropertyName("receiver")]
        public IEnumerable<MessengerReceiverApiModel> Receiver { get; set; } = new List<MessengerReceiverApiModel>();

        [JsonPropertyName("visibility")]
        public string Visibility { get; set; } = string.Empty;

        [JsonPropertyName("visibleuntil")]
        public DateTime VisibleUntil { get; set; }

        [JsonPropertyName("encryptedMessage")]
        public string EncryptedMessage { get; set; } = string.Empty;

        [JsonPropertyName("parentid")]
        public Guid ParentId { get; set; }
    }

    public class MessengerReceiverApiModel
    {
        [JsonPropertyName("user")]
        public string User { get; set; } = "";

        [JsonPropertyName("read")]
        public DateTime Read { get; set; } = DateTime.MinValue;
    }
}
