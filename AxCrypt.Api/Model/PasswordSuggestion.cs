using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    public class PasswordSuggestion
    {
        public PasswordSuggestion()
        {
        }

        public PasswordSuggestion(string suggestion, int bits)
        {
            Suggestion = suggestion;
            EstimatedBits = bits;
        }

        [JsonPropertyName("suggestion")]
        public string? Suggestion { get; set; }

        [JsonPropertyName("bits")]
        public int EstimatedBits { get; set; }
    }
}
