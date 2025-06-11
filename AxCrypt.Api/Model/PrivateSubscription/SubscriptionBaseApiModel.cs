using System.Text.Json.Serialization;
using System;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    public class SubscriptionBaseApiModel : BaseApiModel
    {
        public SubscriptionBaseApiModel()
        { }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("level")]
        public string? Level { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("expirationtimeutc")]
        public DateTime ExpirationTimeUTC { get; set; }

        [JsonPropertyName("offerused")]
        public string? OfferUsed { get; set; }
    }
}
