using System.Text.Json.Serialization;

using AxCrypt.Common;

namespace AxCrypt.Api.Model
{
    public class AccountTip
    {
        public AccountTip()
        {
            Message = string.Empty;
            Level = StartupTipLevel.Unknown;
            ButtonStyle = StartupTipButtonStyle.Unknown;
        }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("level")]
        public StartupTipLevel Level { get; set; }

        [JsonPropertyName("button_style")]
        public StartupTipButtonStyle ButtonStyle { get; set; }

        [JsonPropertyName("url")]
        public Uri? Url { get; set; }
    }
}
