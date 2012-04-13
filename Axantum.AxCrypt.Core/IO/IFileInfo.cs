using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.IO
{
    /// <summary>
    /// Abstraction for FileInfo-related operations
    /// </summary>
    public interface IFileInfo
    {
        FileStream OpenRead();

        FileStream OpenWrite();

        string Name { get; }

        DateTime CreationTimeUtc { get; }

        DateTime LastAccessTimeUtc { get; }

        DateTime LastWriteTimeUtc { get; }
    }
}