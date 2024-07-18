using AxCrypt.Abstractions;
using Newtonsoft.Json;
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

        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }

        [JsonProperty("visibilityType")]
        public string VisibilityType { get; set; }

        [JsonProperty("visibility")]
        public DateTime? Visibility { get; set; }
        
        [JsonProperty("deleted")]
        public DateTime? Deleted { get; set; }
    }
}
