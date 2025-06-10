using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SubscriptionBaseApiModel : BaseApiModel
    {
        public SubscriptionBaseApiModel()
        { }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("expirationtimeutc")]
        public DateTime ExpirationTimeUTC { get; set; }

        [JsonProperty("offerused")]
        public string OfferUsed { get; set; }
    }
}