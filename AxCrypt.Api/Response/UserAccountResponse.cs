using AxCrypt.Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace AxCrypt.Api.Response
{
    public class UserAccountResponse : ResponseBase
    {
        public UserAccountResponse(UserAccount summary)
        {
            UserAccount = summary;
        }

        [JsonPropertyName("S")]
        public UserAccount UserAccount { get; set; }
    }
}
