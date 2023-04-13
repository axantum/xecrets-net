using System.Text.Json.Serialization;

using AxCrypt.Api.Model;

namespace AxCrypt.Api.Response
{
    public class AccountIdResponse : ResponseBase
    {
        public AccountIdResponse()
        {
            AccountId = AccountKey.Empty;
        }

        [JsonPropertyName("id")]
        public AccountKey AccountId { get; set; }
    }
}
