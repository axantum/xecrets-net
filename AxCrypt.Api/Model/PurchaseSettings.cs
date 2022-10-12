using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model
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

        [JsonProperty("micro_payment_product_id")]
        public string MicroPaymentProductId { get; set; }

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

        public IEnumerable<string> BusinessProductIdList
        {
            get
            {
                if (BusinessProductIdsWithAmount == null)
                {
                    return new List<string>();
                }

                string[] productIdsWithAmount = BusinessProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => product.Split("=".ToCharArray())[0]);
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

        public string TaxRateID(string countryOrRegion)
        {
            if (countryOrRegion == null)
            {
                return string.Empty;
            }

            if (TaxRates == null)
            {
                return string.Empty;
            }

            string[] taxRatesWithCultures = TaxRates.Split(",".ToCharArray());
            foreach (string tax in taxRatesWithCultures)
            {
                if (tax == null)
                {
                    continue;
                }

                string[] taxRate = tax.Split("=".ToCharArray());
                if (taxRate[0].Trim() == countryOrRegion || taxRate[0].Trim().Contains(countryOrRegion))
                {
                    return taxRate[1];
                }
            }

            return string.Empty;
        }

        [JsonProperty("appstore_sandbox_url")]
        public string AppStoreSandboxUrl { get; set; }
    }
}