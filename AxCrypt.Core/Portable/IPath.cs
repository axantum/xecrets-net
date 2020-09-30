using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Portable
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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This is not a property, and the .NET analogue is also a method.")]
        string GetRandomFileName();
    }
}