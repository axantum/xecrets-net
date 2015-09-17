using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPublicKey
    {
        public UserPublicKey()
        {
            PublicKey = String.Empty;
            Thumbprint = String.Empty;
            Timestamp = DateTime.MinValue;
            User = String.Empty;
        }

        [JsonProperty("public")]
        public string PublicKey { get; private set; }

        [JsonProperty("thumbprint")]
        public string Thumbprint { get; private set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonProperty("user")]
        public string User { get; private set; }
    }
}