using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Session
{
    /// <summary>
    /// Specifies how the ActiveFile should be visualized in a GUI, depending
    /// on what operations are possible with it.
    /// </summary>
    public enum ActiveFileVisualState
    {
        DecryptedWithKnownKey,
        DecryptedWithoutKnownKey,
        EncryptedNeverBeenDecrypted,
        EncryptedWithKnownKey,
        EncryptedWithoutKnownKey,
    }
}