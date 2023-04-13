using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    public class FeedbackData
    {
        public FeedbackData(string subject, string message)
        {
            Subject = subject ?? string.Empty;
            Message = message ?? string.Empty;
        }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
