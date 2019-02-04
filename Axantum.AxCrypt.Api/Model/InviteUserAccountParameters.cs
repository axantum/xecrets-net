using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// Information for a invite user request.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InviteUserAccountParameters
    {
        public InviteUserAccountParameters(string userName, CultureInfo messageCulture, string personalizedMessage)
        {
            UserName = userName;
            MessageCulture = messageCulture;
            PersonalizedMessage = personalizedMessage;
        }

        /// <summary>
        /// Gets the invited user email address
        /// </summary>
        /// <value>
        /// The userName.
        /// </value>
        [JsonProperty("userName")]
        public string UserName { get; }

        /// <summary>
        /// Gets the user choose culture for sending the invite mail
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        [JsonProperty("messageCulture")]
        public CultureInfo MessageCulture { get; }

        /// <summary>
        /// Gets the invite persionalized message for add the caller information to invite mail.
        /// </summary>
        /// <value>
        /// The invitation Personalized Message .
        /// </value>
        [JsonProperty("personalizedMessage")]
        public string PersonalizedMessage { get; }
    }
}