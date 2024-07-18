using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EncryptedSecretApiModel
    {
        public static EncryptedSecretApiModel Empty = new EncryptedSecretApiModel();

        public EncryptedSecretApiModel()
        {
        }

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; } = string.Empty;

        [JsonProperty("cipher")]
        public byte[] Cipher { get; set; }

        [JsonProperty("created")]
        public DateTime CreatedUtc { get; set; }
    }
}