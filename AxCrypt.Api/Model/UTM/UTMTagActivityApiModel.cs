using System.Text.Json.Serialization;
using System;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagActivityApiModel
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

        [JsonPropertyName("urlreferrer")]
        public string UrlReferrer { get; set; }
    }
}
