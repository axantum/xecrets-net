using System.Text.Json.Serialization;

namespace AxCrypt.Api.Model
{
    /// <summary>
    /// Information for a password reset request.
    /// </summary>
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
        [JsonPropertyName("password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets the verification code sent to the user via a side-channel, most likely e-mail. This
        /// must match the code on the server in order to effectuate the change.
        /// </summary>
        /// <value>
        /// The verification code.
        /// </value>
        [JsonPropertyName("verification")]
        public string Verification { get; set; }
    }
}
