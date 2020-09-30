using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Fake
{
    public static class Extensions
    {
        public static string PathCombine(this string path, params string[] parts)
        {
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
            }
            return path;
        }
    }
}