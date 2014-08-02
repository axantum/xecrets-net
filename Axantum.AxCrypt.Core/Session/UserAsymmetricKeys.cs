using Axantum.AxCrypt.Core.Crypto.Asymmetric;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// A respository for a single user e-mail. A user has a single active key pair, with both a public
    /// key for encryption and the matching private key for decryption. There may also be a list of
    /// previously used private keys, in order to be able to decrypt files encrypted with older key pairs.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserAsymmetricKeys
    {
        private UserAsymmetricKeys()
        {
            RecalledPrivateKeys = new List<IAsymmetricKey>();
        }

        public UserAsymmetricKeys(string userEmail)
        {
            UserEmail = userEmail;
            RecalledPrivateKeys = new List<IAsymmetricKey>();
            KeyPair  = AsymmetricKeyPair.Create();
        }

        [JsonProperty("useremail")]
        public string UserEmail { get; private set; }

        [JsonProperty("recalledprivatekeys")]
        public IList<IAsymmetricKey> RecalledPrivateKeys { get; private set; }

        [JsonProperty("keypair")]
        public AsymmetricKeyPair KeyPair { get; private set; }
    }
}
