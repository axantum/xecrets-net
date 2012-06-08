using System;

namespace Axantum.AxCrypt.Core
{
    [Flags]
    public enum AxCryptOptions
    {
        None = 0,
        SetFileTimes = 1,
        EncryptWithCompression = 2,
        EncryptWithoutCompression = 4,
    }
}