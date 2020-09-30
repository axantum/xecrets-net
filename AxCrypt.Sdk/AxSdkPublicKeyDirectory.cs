using AxCrypt.Abstractions.Rest;
using AxCrypt.Api;
using AxCrypt.Api.Model;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AxCrypt.Sdk
{
    public class AxSdkPublicKeyDirectory
    {
        private EmailAddress _email;

        private Passphrase _passphrase;

        private AxCryptApiClient _client;

        public AxSdkPublicKeyDirectory(string email, Guid apiKey, AxSdkConfiguration configuration)
        {
            _email = EmailAddress.Parse(email);
            _passphrase = new Passphrase(apiKey.ToString());

            _client = new AxCryptApiClient(new RestIdentity(email, _passphrase.Text), configuration.ApiBaseUrl, configuration.ApiTimeout);
        }

        public async Task<string> PublicKeyAsync(string email)
        {
            AccountKey accountKey = await _client.GetPublicApiKeyAllAccountsOtherUserPublicKeyAsync(email);

            return accountKey.KeyPair.PublicPem;
        }
    }
}