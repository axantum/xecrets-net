using Axantum.AxCrypt.Abstractions.Rest;
using Axantum.AxCrypt.Api;
using Axantum.AxCrypt.Api.Model;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Sdk
{
    public class SdkPublicKeyDirectory
    {
        private EmailAddress _email;

        private Passphrase _passphrase;

        private AxCryptApiClient _client;

        public SdkPublicKeyDirectory(string email, string passphrase, SdkConfiguration configuration)
        {
            _email = EmailAddress.Parse(email);
            _passphrase = new Passphrase(passphrase);

            _client = new AxCryptApiClient(new RestIdentity(email, passphrase), configuration.ApiBaseUrl, configuration.ApiTimeout);
        }

        public async Task<string> PublicKeyAsync(string email)
        {
            AccountKey accountKey = await _client.GetAllAccountsOtherUserPublicKeyAsync(email);

            return accountKey.KeyPair.PublicPem;
        }
    }
}
