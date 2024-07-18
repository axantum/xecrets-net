using System.Text.Json.Serialization;
using System;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagActivityExportApiModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("utmtag")]
        public Guid UTMTag { get; set; }

        [JsonPropertyName("action")]
        public string Action { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("userhostaddress")]
        public string UserHostAddress { get; set; }

        [JsonPropertyName("useragent")]
        public string UserAgent { get; set; }

        [JsonPropertyName("createdutc")]
        public DateTime Createdutc { get; set; } = DateTime.MinValue;

        [JsonPropertyName("useremail")]
        public string UserEmail { get; set; }

        [JsonPropertyName("subslevel")]
        public string SubsLevel { get; set; }

        [JsonPropertyName("substype")]
        public string SubsType { get; set; }

        [JsonPropertyName("subsid")]
        public string SubsId { get; set; }

        [JsonPropertyName("transid")]
        public string TransId { get; set; }

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("urlreferrer")]
        public string UrlReferrer { get; set; }
    }
}
