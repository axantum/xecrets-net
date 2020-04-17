using System;
using Newtonsoft.Json;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class InAppPurchaseSettings
    {
        [JsonConstructor]
        public InAppPurchaseSettings(string productIds, string yearlyDiscountPercentage)
        {
            ProductIds = productIds;
            YearlyDiscountPercentage = yearlyDiscountPercentage.ToString();
        }

        public InAppPurchaseSettings() : this(string.Empty, "0")
        {
        }

        [JsonProperty("product_ids")]
        public string ProductIds { get; }

        [JsonProperty("yearly_discount_percent")]
        public string YearlyDiscountPercentage { get; }
    }
}
