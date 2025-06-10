using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model.PrivateSubscription
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PrivateSubscriptionInformationApiModel : PrivateSubscriptionApiModel
    {
        [JsonProperty("amountPaid")]
        public decimal AmountPaid { get; set; }

        [JsonProperty("amountVat")]
        public decimal AmountVat { get; set; }

        [JsonProperty("currencyPaid")]
        public string CurrencyPaid { get; set; }

        [JsonProperty("accountingCurrencyRate")]
        public decimal AccountingCurrencyRate { get; set; }

        [JsonProperty("systemcomment")]
        public string SystemComment { get; set; }

        [JsonProperty("usercomment")]
        public string UserComment { get; set; }

        [JsonProperty("payeeid")]
        public string PayeeId { get; set; }

        [JsonProperty("beneficiaryid")]
        public string BeneficiaryId { get; set; }

        [JsonProperty("paymentreference")]
        public string PaymentReference { get; set; }

        [JsonProperty("paymenttype")]
        public string PaymentType { get; set; }
    }
}