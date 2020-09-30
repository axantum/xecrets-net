using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PasswordSuggestion
    {
        [JsonConstructor]
        public PasswordSuggestion()
        {
        }

        public PasswordSuggestion(string suggestion, int bits)
        {
            Suggestion = suggestion;
            EstimatedBits = bits;
        }

        [JsonProperty("suggestion")]
        public string Suggestion { get; private set; }

        [JsonProperty("bits")]
        public int EstimatedBits { get; private set; }
    }
}