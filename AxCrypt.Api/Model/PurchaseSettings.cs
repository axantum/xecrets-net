using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model
{
    public class PurchaseSettings : BaseApiModel
    {
        public PurchaseSettings(string provider)
        {
            Provider = provider;
        }

        private PurchaseSettings()
        {
        }

        public static PurchaseSettings Empty { get; } = new PurchaseSettings();

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("premiumproductids")]
        public string? PremiumProductIdsWithAmount { get; set; }

        [JsonPropertyName("businessproductids")]
        public string? BusinessProductIdsWithAmount { get; set; }

        [JsonPropertyName("passwordmanagerproductid")]
        public string? PasswordManagerProductId { get; set; }

        [JsonPropertyName("yearlydiscountpercentage")]
        public int YearlyDiscountPercentage { get; set; }

        [JsonPropertyName("taxrates")]
        public string? TaxRates { get; set; }

        [JsonPropertyName("micropaymentproductid")]
        public string? MicroPaymentProductId { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonPropertyName("appstoresandboxurl")]
        public string? AppStoreSandboxUrl { get; set; }
    }
}
