using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    public class SecretApiModel
    {
        public static SecretApiModel Empty = new SecretApiModel(0, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public SecretApiModel(int id, string type, string userEmail, string secret, DateTime createdUtc, DateTime updatedUtc, DateTime deletedUtc)
        {
            Id = id;
            Type = type;
            UserEmail = userEmail;
            TheSecret = secret;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("user_email")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("secret")]
        public string TheSecret { get; set; } = string.Empty;

        [JsonPropertyName("created_utc")]
        public DateTime CreatedUtc { get; set; }

        [JsonPropertyName("updated_utc")]
        public DateTime UpdatedUtc { get; set; }

        [JsonPropertyName("deleted_utc")]
        public DateTime? DeletedUtc { get; set; }
    }
}
