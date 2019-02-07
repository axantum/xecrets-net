using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// Information for the invitation message.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class InvitationMessageParameters
    {
        public InvitationMessageParameters(CultureInfo messageCulture, string personalizedMessage)
        {
            MessageCulture = messageCulture;
            PersonalizedMessage = personalizedMessage;
        }

        public InvitationMessageParameters()
            : this(CultureInfo.CurrentCulture, string.Empty)
        {
        }

        /// <summary>
        /// Gets the language culture to send the invitation message in the recipient's preferred language/culture.
        /// </summary>
        /// <value>
        /// The language culture.
        /// </value>
        [JsonProperty("messageCulture")]
        public CultureInfo MessageCulture { get; }

        /// <summary>
        /// Gets the inviter's personalized message which will exist in invitation message.
        /// </summary>
        /// <value>
        /// The invitation personalized message.
        /// </value>
        [JsonProperty("personalizedMessage")]
        public string PersonalizedMessage { get; }
    }
}