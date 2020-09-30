using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Model
{
    /// <summary>
    /// Information for a password reset request.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PasswordResetParameters
    {
        public PasswordResetParameters(string password, string verification)
        {
            Password = password;
            Verification = verification;
        }

        /// <summary>
        /// Gets the password to reset to. Note that resetting the password does not gain access
        /// to previously encrypted data. It just means the user can access the account.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [JsonProperty("password")]
        public string Password { get; }

        /// <summary>
        /// Gets the verification code sent to the user via a side-channel, most likely e-mail. This
        /// must match the code on the server in order to effectuate the change.
        /// </summary>
        /// <value>
        /// The verification code.
        /// </value>
        [JsonProperty("verification")]
        public string Verification { get; }
    }
}