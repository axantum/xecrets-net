using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Api.Model
{
    public enum SubscriptionLevel
    {
        /// <summary>
        /// Illegal value
        /// </summary>
        None = 0,

        /// <summary>
        /// The level is unknown because the authentication failed.
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// The free level, may come with various restrictions decided upon by the client application.
        /// </summary>
        Free,

        /// <summary>
        /// The premium level, adds some for-pay features decided upon by the client application.
        /// </summary>
        Premium,
    }
}