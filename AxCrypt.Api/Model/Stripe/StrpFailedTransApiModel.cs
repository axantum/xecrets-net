using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Stripe
{
    public class StrpFailedTransApiModel : BaseApiModel
    {
        public static StrpFailedTransApiModel Empty = new StrpFailedTransApiModel(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty, string.Empty, null, DateTime.MinValue, DateTime.MinValue, null, null);

        public StrpFailedTransApiModel()
        {
        }

        public StrpFailedTransApiModel(string chargeid, string failurecode, string failuremessage, string reason, string sellermessage, string customeremail, string hostedinvoiceurlcharacter, long attemptCount, string subsMetadata, string subsLevel, DateTime? paidOnUtc, DateTime createdUtc, DateTime updatedUtc, DateTime? viewedUtc, DateTime? deletedUtc)
        {
            ChargeId = chargeid;
            FailureCode = failurecode;
            FailureMessage = failuremessage;
            Reason = reason;
            SellerMessage = sellermessage;
            CustomerEmail = customeremail;
            HostedInvoiceUrl = hostedinvoiceurlcharacter;
            AttemptCount = attemptCount;
            SubsMetadata = subsMetadata;
            SubsLevel = subsLevel;
            PaidOnUtc = paidOnUtc;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            ViewedUtc = viewedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonPropertyName("chargeid")]
        public string ChargeId { get; set; }

        [JsonPropertyName("failurecode")]
        public string FailureCode { get; set; }

        [JsonPropertyName("failuremessage")]
        public string FailureMessage { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("sellermessage")]
        public string SellerMessage { get; set; }

        [JsonPropertyName("customeremail")]
        public string CustomerEmail { get; set; }

        [JsonPropertyName("hostedinvoiceurl")]
        public string HostedInvoiceUrl { get; set; }

        [JsonPropertyName("attemptcount")]
        public long AttemptCount { get; set; }

        [JsonPropertyName("subsmetadata")]
        public string SubsMetadata { get; set; }

        [JsonPropertyName("subslevel")]
        public string SubsLevel { get; set; }

        [JsonPropertyName("paidonutc")]
        public DateTime? PaidOnUtc { get; set; }

        [JsonPropertyName("viewedutc")]
        public DateTime? ViewedUtc { get; set; }
    }
}
