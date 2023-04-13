using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
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

        [JsonPropertyName("receipt-data")]
        public string ReceiptData { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("paid_by")]
        public string PaidBy { get; set; }

        [JsonPropertyName("paid_for")]
        public string PaidFor { get; set; }

        [JsonPropertyName("txn_id")]
        public string TransactionId { get; set; }

        [JsonPropertyName("currency_paid")]
        public string CurrencyPaid { get; set; }

        [JsonPropertyName("amount_paid")]
        public decimal AmountPaid { get; set; }

        [JsonPropertyName("datetime")]
        public DateTime DateTimeUtc { get; set; }

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonPropertyName("item_name")]
        public string? ItemName { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("amount_fee")]
        public decimal AmountFee { get; set; }

        [JsonPropertyName("amount_vat")]
        public decimal AmountVat { get; set; }

        [JsonPropertyName("discount_code")]
        public string AppliedDiscountCode { get; set; } = string.Empty;
    }
}
