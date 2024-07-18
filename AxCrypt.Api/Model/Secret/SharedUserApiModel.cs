using AxCrypt.Abstractions;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Secrets
{
    public class SharedUserApiModel
    {
        public SharedUserApiModel(string userEmail, string visibilityType, DateTime? visibility, DateTime? deleted = null)
        {
            UserEmail = userEmail;
            VisibilityType = visibilityType;
            Visibility = visibility;
            Deleted = deleted;
        }

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; }

        [JsonPropertyName("visibilityType")]
        public string VisibilityType { get; set; }

        [JsonPropertyName("visibility")]
        public DateTime? Visibility { get; set; }
        
        [JsonPropertyName("deleted")]
        public DateTime? Deleted { get; set; }
    }
}
