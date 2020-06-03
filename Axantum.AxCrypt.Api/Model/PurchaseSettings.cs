using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class PurchaseSettings
    {
        public PurchaseSettings(string provider)
        {
            Provider = provider;
        }

        [JsonConstructor]
        private PurchaseSettings()
        {
        }

        public static PurchaseSettings Empty { get; } = new PurchaseSettings();

        [JsonProperty("provider")]
        public string Provider { get; private set; }

        [JsonProperty("premium_product_ids")]
        public string PremiumProductIdsWithAmount { get; set; }

        [JsonProperty("business_product_ids")]
        public string BusinessProductIdsWithAmount { get; set; }

        [JsonProperty("yearly_discount_percent")]
        public int YearlyDiscountPercentage { get; set; }

        [JsonProperty("tax_rates")]
        public string TaxRates { get; set; }

        public IEnumerable<string> PremiumProductIdList
        {
            get
            {
                if (PremiumProductIdsWithAmount == null)
                {
                    return new List<string>();
                }

                string[] productIdsWithAmount = PremiumProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => product.Split("=".ToCharArray())[0]);
            }
        }

        public IEnumerable<SubscriptionProduct> PremiumProducts
        {
            get
            {
                if (PremiumProductIdsWithAmount == null)
                {
                    return new List<SubscriptionProduct>();
                }

                string[] productIdsWithAmount = PremiumProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => GetSubscriptionProduct(product));
            }
        }

        public IEnumerable<SubscriptionProduct> BusinessSubscriptionProducts
        {
            get
            {
                if (BusinessProductIdsWithAmount == null)
                {
                    return new List<SubscriptionProduct>();
                }

                string[] productIdsWithAmount = BusinessProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => GetSubscriptionProduct(product));
            }
        }

        private static SubscriptionProduct GetSubscriptionProduct(string productIdsWithAmounString)
        {
            if (productIdsWithAmounString == null)
            {
                return new SubscriptionProduct();
            }

            string[] productIdsWithAmount = productIdsWithAmounString.Split("=".ToCharArray());
            decimal amount = 0m;
            if (productIdsWithAmount.Length > 1 && productIdsWithAmount[1].Length > 0)
            {
                amount = decimal.Parse(productIdsWithAmount[1]);
            }

            return new SubscriptionProduct() { Id = productIdsWithAmount[0], AmountExcludingVat = amount, };
        }
    }
}