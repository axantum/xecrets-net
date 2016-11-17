using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static bool IsFileOrDirectoryNotFound(this Exception ex)
        {
            if (ex is FileNotFoundException)
            {
                return true;
            }

            return IsDirectoryNotFound(ex);
        }

        private static bool IsDirectoryNotFound(this Exception ex)
        {
            string typeName = ex.GetType().FullName;
            if (typeName == "System.IO.DirectoryNotFoundException")
            {
                return true;
            }

            return false;
        }
    }
}
