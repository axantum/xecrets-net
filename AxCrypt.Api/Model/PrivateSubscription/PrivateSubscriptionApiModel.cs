using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PrivateSubscriptionApiModel : SubscriptionBaseApiModel
    {
        public PrivateSubscriptionApiModel()
        {
        }

        [JsonProperty("eventId")]
        public Guid EventId { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("purchaseOriginalTransactionId")]
        public string PurchaseOriginalTransactionId { get; set; }

        [JsonProperty("accountingCurrency")]
        public string AccountingCurrency { get; set; }

        [JsonProperty("starttimeutc")]
        public DateTime StartTimeUTC { get; set; }
    }
}