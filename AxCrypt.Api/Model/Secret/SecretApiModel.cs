using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api.Model.Secret
{
    [JsonObject(MemberSerialization.OptIn)]
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

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("user_email")]
        public string UserEmail { get; } = string.Empty;

        [JsonProperty("secret")]
        public string TheSecret { get; } = string.Empty;

        [JsonProperty("created_utc")]
        public DateTime CreatedUtc { get; }

        [JsonProperty("updated_utc")]
        public DateTime UpdatedUtc { get; }

        [JsonProperty("deleted_utc")]
        public DateTime? DeletedUtc { get; }
    }
}