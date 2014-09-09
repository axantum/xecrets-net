using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface IPath
    {
        string GetFileName(string path);

        string GetDirectoryName(string path);

        string GetExtension(string path);

        string Combine(string left, string right);

        string GetFileNameWithoutExtension(string path);

        char DirectorySeparatorChar { get; }

        string GetPathRoot(string path);

        string GetRandomFileName();
    }
}