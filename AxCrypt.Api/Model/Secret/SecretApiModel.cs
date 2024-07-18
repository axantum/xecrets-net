using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    public class SecretApiModel : BaseApiModel
    {
        public static SecretApiModel Empty = new SecretApiModel(0, string.Empty, string.Empty, string.Empty, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

        public SecretApiModel(long id, string type, string userEmail, string secretBody, DateTime createdUtc, DateTime updatedUtc, DateTime? deletedUtc)
        {
            Id = id;
            Type = type;
            UserEmail = userEmail;
            SecretBody = secretBody;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            DeletedUtc = deletedUtc;
        }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("secretBody")]
        public string SecretBody { get; set; } = string.Empty;
    }
}
