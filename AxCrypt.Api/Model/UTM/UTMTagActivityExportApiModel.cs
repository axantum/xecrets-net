using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagActivityExportApiModel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("utmtag")]
        public Guid UTMTag { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("userhostaddress")]
        public string UserHostAddress { get; set; }

        [JsonProperty("useragent")]
        public string UserAgent { get; set; }

        [JsonProperty("createdutc")]
        public DateTime Createdutc { get; set; } = DateTime.MinValue;

        [JsonProperty("useremail")]
        public string UserEmail { get; set; }

        [JsonProperty("subslevel")]
        public string SubsLevel { get; set; }

        [JsonProperty("substype")]
        public string SubsType { get; set; }

        [JsonProperty("subsid")]
        public string SubsId { get; set; }

        [JsonProperty("transid")]
        public string TransId { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("urlreferrer")]
        public string UrlReferrer { get; set; }
    }
}