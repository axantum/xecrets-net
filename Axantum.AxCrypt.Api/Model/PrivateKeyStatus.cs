using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Api.Model
{
    public enum PrivateKeyStatus
    {
        None,

        /// <summary>
        /// The passphrase was konwn at the last sign in for this key pair
        /// </summary>
        PassphraseKnown,

        /// <summary>
        /// The passphrase was unknown at the last sign in for this key pair
        /// </summary>
        PassphraseUnknown,

        /// <summary>
        /// There is no private key
        /// </summary>
        Empty,
    }
}
