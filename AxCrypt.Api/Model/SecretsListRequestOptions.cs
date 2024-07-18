using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AxCrypt.Api
{
    public class SecretsListRequestOptions : RequestOptions
    {
        public SecretsListRequestOptions(string userName, Guid secretId)
        {
            UserName = userName;
            SecretId = secretId;
        }

        public SecretsListRequestOptions(string userName) : this(userName, Guid.Empty)
        {
        }

        [JsonProperty("secretId")]
        public Guid SecretId { get; set; } = Guid.Empty;
    }
}