using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    [Flags]
    public enum FileInfoTypes
    {
        None = 0,

        Folder = 1,

        EncryptedFile = 2,

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable")]
        EncryptableFile = 4,

        NonExisting = 8,

        OtherFile = 16,
    }
}