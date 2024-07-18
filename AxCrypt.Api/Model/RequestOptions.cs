using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api
{
    public class RequestOptions
    {
        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public long Id { get; set; } = 0;

        [JsonPropertyName("queryString")]
        public string QueryString { get; set; } = string.Empty;

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; } = 0;

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; } = 0;

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }
    }
}
