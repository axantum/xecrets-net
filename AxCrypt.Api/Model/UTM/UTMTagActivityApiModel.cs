using Newtonsoft.Json;
using System;

namespace AxCrypt.Api.Model.UTM
{
    public class UTMTagActivityApiModel
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

        [JsonProperty("urlreferrer")]
        public string UrlReferrer { get; set; }
    }
}