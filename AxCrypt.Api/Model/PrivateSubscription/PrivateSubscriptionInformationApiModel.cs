using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    public class PrivateSubscriptionInformationApiModel : PrivateSubscriptionApiModel
    {
        [JsonPropertyName("amountPaid")]
        public decimal AmountPaid { get; set; }

        [JsonPropertyName("amountVat")]
        public decimal AmountVat { get; set; }

        [JsonPropertyName("currencyPaid")]
        public string? CurrencyPaid { get; set; }

        [JsonPropertyName("accountingCurrencyRate")]
        public decimal AccountingCurrencyRate { get; set; }

        [JsonPropertyName("systemcomment")]
        public string? SystemComment { get; set; }

        [JsonPropertyName("usercomment")]
        public string? UserComment { get; set; }

        [JsonPropertyName("payeeid")]
        public string? PayeeId { get; set; }

        [JsonPropertyName("beneficiaryid")]
        public string? BeneficiaryId { get; set; }

        [JsonPropertyName("paymentreference")]
        public string? PaymentReference { get; set; }

        [JsonPropertyName("paymenttype")]
        public string? PaymentType { get; set; }
    }
}
