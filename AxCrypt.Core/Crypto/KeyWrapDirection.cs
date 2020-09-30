using System;
using System.Linq;

namespace AxCrypt.Core.Crypto
{
    /// <summary>
    /// Control the direction of the NIST Key Wrap
    /// </summary>
    public enum KeyWrapDirection
    {
        /// <summary>
        /// Reserved
        /// </summary>
        Unknown,

        /// <summary>
        /// Use when wrapping
        /// </summary>
        Encrypt,

        /// <summary>
        /// Use when unwrapping
        /// </summary>
        Decrypt,
    }
}