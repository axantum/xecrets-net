using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    public class SlackPayload
    {
        [JsonPropertyName("channel")]
        public string? Channel { get; set; }

        [JsonPropertyName("username")]
        public string? UserName { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
