using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Stripe
{
    [JsonObject(MemberSerialization.OptIn)]
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

        [JsonProperty("chargeid")]
        public string ChargeId { get; set; }

        [JsonProperty("failurecode")]
        public string FailureCode { get; set; }

        [JsonProperty("failuremessage")]
        public string FailureMessage { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("sellermessage")]
        public string SellerMessage { get; set; }

        [JsonProperty("customeremail")]
        public string CustomerEmail { get; set; }

        [JsonProperty("hostedinvoiceurl")]
        public string HostedInvoiceUrl { get; set; }

        [JsonProperty("attemptcount")]
        public long AttemptCount { get; set; }

        [JsonProperty("subsmetadata")]
        public string SubsMetadata { get; set; }

        [JsonProperty("subslevel")]
        public string SubsLevel { get; set; }

        [JsonProperty("paidonutc")]
        public DateTime? PaidOnUtc { get; set; }

        [JsonProperty("viewedutc")]
        public DateTime? ViewedUtc { get; set; }
    }
}