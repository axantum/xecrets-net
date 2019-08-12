using System.Globalization;
using Newtonsoft.Json;

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
            _messageCultureStringWorkAroundMsCorlibVsSystemGlobalizationPCLSerializationProblem = messageCulture?.ToString();
            CustomMessage = customMessage;
        }

        /// <summary>
        /// It is not 100% clear what the problem is - but, when an attempt is made to serialize the CultureInfo object
        /// returned by 'MessageCulture' (which is defined as a TypeForwardedTo in System.Globalization.dll in this portable profile)
        /// serialization fails with an exception caused by loopoing of the Parent property. Apparently, in this situation Newtonsoft.Json
        /// will not recognize the CultureInfo type, and instead serialize all public properties. Normally, it is serialized as it's .ToString()
        /// representation, i.e. just "en-US" etc. We have not been able to reproduce the problem in a unit test, but for now we're working
        /// around it by not using the CultureInfo for Newtonsoft serialization.
        ///
        /// This problem will probably go away if we standardize on .NET Standard 2.0 instead of the current PCL profile mess. This requires
        /// pretty extensive testing and verification to ensure that all builds work as expected on all platforms, and it's likely to give
        /// less problems by waiting for the dust to settle around PCL, .NET Core, .NET Standard etc.
        /// </summary>
        [JsonProperty("messageCulture")]
        private readonly string _messageCultureStringWorkAroundMsCorlibVsSystemGlobalizationPCLSerializationProblem;

        private CultureInfo _cultureInfo;

        /// <summary>
        /// Gets the language culture to send the message in the recipient's preferred language/culture.
        /// </summary>
        /// <value>
        /// The language culture or null if none was provided
        /// </value>
        public CultureInfo MessageCulture
        {
            get
            {
                if (_messageCultureStringWorkAroundMsCorlibVsSystemGlobalizationPCLSerializationProblem == null)
                {
                    return null;
                }

                if (_cultureInfo == null)
                {
                    _cultureInfo = new CultureInfo(_messageCultureStringWorkAroundMsCorlibVsSystemGlobalizationPCLSerializationProblem);
                }

                return _cultureInfo;
            }
        }

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