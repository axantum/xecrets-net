using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    public enum FileInfoType
    {
        Folder,
        EncryptedFile,

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Encryptable")]
        EncryptableFile,

        NonExisting,
        OtherFile,
    }
}