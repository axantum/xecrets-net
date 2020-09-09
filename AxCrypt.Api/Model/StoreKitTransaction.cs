using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class StoreKitTransaction
    {
        public StoreKitTransaction(string productId, string receiptData, string paidBy, string paidFor, string transactionId, string currencyPaid, decimal amountPaid, DateTime dateTimeUtc, string paymentStatus)
        {
            ProductId = productId;
            ReceiptData = receiptData;
            PaidBy = paidBy;
            PaidFor = paidFor;
            TransactionId = transactionId;
            CurrencyPaid = currencyPaid;
            AmountPaid = amountPaid;
            DateTimeUtc = dateTimeUtc;
            PaymentStatus = paymentStatus;
        }

        [JsonProperty("receipt-data")]
        public string ReceiptData { get; }

        [JsonProperty("product_id")]
        public string ProductId { get; }

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

        [JsonProperty("payment_status")]
        public string PaymentStatus { get; }

        [JsonProperty("item_name")]
        public string ItemName { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("amount_fee")]
        public decimal AmountFee { get; set; }

        [JsonProperty("amount_vat")]
        public decimal AmountVat { get; set; }

        [JsonProperty("discount_code")]
        public string AppliedDiscountCode { get; set; } = string.Empty;
    }
}