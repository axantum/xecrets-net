using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FeedbackData
    {
        [JsonConstructor]
        public FeedbackData()
        {
        }

        public FeedbackData(string subject, string message)
        {
            Subject = subject;
            Message = message;
        }

        [JsonProperty("subject")]
        public string Subject { get; private set; }

        [JsonProperty("message")]
        public string Message { get; private set; }
    }
}
