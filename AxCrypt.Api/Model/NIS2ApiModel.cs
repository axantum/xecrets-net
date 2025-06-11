using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    public class NIS2ApiModel : BaseApiModel
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("fullname")]
        public string? FullName { get; set; }

        [JsonPropertyName("workemail")]
        public string? WorkEmail { get; set; }

        [JsonPropertyName("companyname")]
        public string? CompanyName { get; set; }

        [JsonPropertyName("companysize")]
        public string? CompanySize { get; set; }

        [JsonPropertyName("jobtitle")]
        public string? JobTitle { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("grecaptcharesponse")]
        public string? GreCaptchaResponse { get; set; }
    }
}
