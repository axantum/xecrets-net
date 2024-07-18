using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace AxCrypt.Api.Model
{
    public class EncryptedSecretApiModel
    {
        public static EncryptedSecretApiModel Empty = new EncryptedSecretApiModel();

        public EncryptedSecretApiModel()
        {
        }

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonPropertyName("cipher")]
        public byte[]? Cipher { get; set; }

        [JsonPropertyName("created")]
        public DateTime CreatedUtc { get; set; }
    }
}
