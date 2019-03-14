using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Axantum.AxCrypt.Api.Model
{
    /// <summary>
    /// Information for custom messages.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CustomMessageParameters
    {
        public CustomMessageParameters(CultureInfo messageCulture, string customMessage)
        {
            MessageCulture = messageCulture;
            CustomMessage = customMessage;
        }

        /// <summary>
        /// Gets the language culture to send the message in the recipient's preferred language/culture.
        /// </summary>
        /// <value>
        /// The language culture.
        /// </value>
        [JsonProperty("messageCulture")]
        public CultureInfo MessageCulture { get; }

        /// <summary>
        /// Gets a custom message which will used in the final message.
        /// </summary>
        /// <value>
        /// The custom message.
        /// </value>
        [JsonProperty("customMessage")]
        public string CustomMessage { get; }
    }
}