using System;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class InAppPurchaseSettings
    {
        public InAppPurchaseSettings(string productIds, int yearlyDiscountPercentage, string provider)
        {
            ProductIdsWithAmount = productIds;
            YearlyDiscountPercentage = yearlyDiscountPercentage;
            Provider = provider;
        }

        [JsonConstructor]
        private InAppPurchaseSettings()
        {
        }

        public static InAppPurchaseSettings Empty { get; } = new InAppPurchaseSettings();

        [JsonProperty("product_ids")]
        public string ProductIdsWithAmount { get; set; }

        [JsonProperty("yearly_discount_percent")]
        public int YearlyDiscountPercentage { get; set; }

        [JsonProperty("provider")]
        public string Provider { get; private set; }

        public IEnumerable<string> ProductIdList
        {
            get
            {
                if (ProductIdsWithAmount == null)
                {
                    return new List<string>();
                }

                string[] productIdsWithAmount = ProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => product.Split("|".ToCharArray())[0]);
            }
        }

        public IEnumerable<SubscriptionProduct> SubscriptionProducts
        {
            get
            {
                if (ProductIdsWithAmount == null)
                {
                    return new List<SubscriptionProduct>();
                }

                string[] productIdsWithAmount = ProductIdsWithAmount.Split(",".ToCharArray());
                return productIdsWithAmount.Select(product => GetSubscriptionProduct(product));
            }
        }

        private static SubscriptionProduct GetSubscriptionProduct(string productIdsWithAmounString)
        {
            if (productIdsWithAmounString == null)
            {
                return new SubscriptionProduct();
            }

            string[] productIdsWithAmount = productIdsWithAmounString.Split("|".ToCharArray());
            decimal amount = 0m;
            if (productIdsWithAmount.Length > 1 && productIdsWithAmount[1].Length > 0)
            {
                amount = decimal.Parse(productIdsWithAmount[1]);
            }

            return new SubscriptionProduct() { Id = productIdsWithAmount[0], AmountExcludingVat = amount };
        }
    }
}