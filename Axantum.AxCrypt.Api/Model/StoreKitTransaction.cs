using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StoreKitTransaction
    {
        public StoreKitTransaction(string productId, int subscriptionMonths, string paidBy, string paidFor, string transactionId, string currencyPaid, decimal amountPaid, DateTime paymentDate)
        {
            ProductId = productId;
            SubscriptionMonths = subscriptionMonths;
            PaidBy = paidBy;
            PaidFor = paidFor;
            TransactionId = transactionId;
            CurrencyPaid = currencyPaid;
            AmountPaid = amountPaid;
            PaymentDate = paymentDate;
        }

        public StoreKitTransaction()
            : this(string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty, 0m, DateTime.UtcNow)
        {
        }

        [JsonProperty("product_id")]
        public string ProductId { get; }

        [JsonProperty("months")]
        public int SubscriptionMonths { get; }

        [JsonProperty("paid_by")]
        public string PaidBy { get; }

        [JsonProperty("paid_for")]
        public string PaidFor { get; }

        [JsonProperty("txn_id")]
        public string TransactionId { get; }

        [JsonProperty("currency_paid")]
        public string CurrencyPaid { get; }

        [JsonProperty("amount_paid")]
        public decimal AmountPaid { get; }

        [JsonProperty("payment_date")]
        public DateTime PaymentDate { get; }

        [JsonProperty("datetime")]
        public DateTime DateTimeUtc { get; set; }

        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("amount_fee")]
        public decimal AmountFee { get; set; }

        [JsonProperty("amount_vat")]
        public decimal AmountVat { get; set; }

        [JsonProperty("is_production")]
        public bool IsProduction { get; set; }

        [JsonProperty("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonProperty("discount_code")]
        public string AppliedDiscountCode { get; set; } = string.Empty;
    }
}