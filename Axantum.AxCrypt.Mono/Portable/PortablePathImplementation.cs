using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Mono.Portable
{
    internal class PortablePathImplementation : Axantum.AxCrypt.Core.Portable.IPath
    {
        public string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public string Combine(string left, string right)
        {
            return Path.Combine(left, right);
        }

        public string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public char DirectorySeparatorChar
        {
            get { return Path.DirectorySeparatorChar; }
        }

        public string GetPathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }

        public string GetRandomFileName()
        {
            return Path.GetRandomFileName();
        }
    }
}