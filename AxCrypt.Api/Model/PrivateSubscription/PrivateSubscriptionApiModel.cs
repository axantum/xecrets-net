using System.Text.Json.Serialization;
using System;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    public class PrivateSubscriptionApiModel : SubscriptionBaseApiModel
    {
        public PrivateSubscriptionApiModel()
        {
        }

        [JsonPropertyName("eventId")]
        public Guid EventId { get; set; }

        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("purchaseOriginalTransactionId")]
        public string? PurchaseOriginalTransactionId { get; set; }

        [JsonPropertyName("accountingCurrency")]
        public string? AccountingCurrency { get; set; }

        [JsonPropertyName("starttimeutc")]
        public DateTime StartTimeUTC { get; set; }
    }
}
