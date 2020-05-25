using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StoreKitTransaction
    {
        public StoreKitTransaction(string productId, string receiptData, int subscriptionDays, string paidBy, string paidFor, string transactionId, string currencyPaid, decimal amountPaid, DateTime dateTimeUtc)
        {
            ProductId = productId;
            ReceiptData = receiptData;
            SubscriptionDays = subscriptionDays;
            PaidBy = paidBy;
            PaidFor = paidFor;
            TransactionId = transactionId;
            CurrencyPaid = currencyPaid;
            AmountPaid = amountPaid;
            DateTimeUtc = dateTimeUtc;
        }

        [JsonProperty("receipt-data")]
        public string ReceiptData { get; }

        [JsonProperty("product_id")]
        public string ProductId { get; }

        [JsonProperty("days")]
        public int SubscriptionDays { get; }

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

        [JsonProperty("datetime")]
        public DateTime DateTimeUtc { get; }

        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("amount_fee")]
        public decimal AmountFee { get; set; }

        [JsonProperty("amount_vat")]
        public decimal AmountVat { get; set; }

        [JsonProperty("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonProperty("discount_code")]
        public string AppliedDiscountCode { get; set; } = string.Empty;
    }
}