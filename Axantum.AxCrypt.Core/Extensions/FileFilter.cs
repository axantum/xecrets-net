using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;

namespace Axantum.AxCrypt.Core.Extensions
{
    public static class FileFilter
    {
        private static readonly List<Regex> pathFilters = new List<Regex>();

        public static bool IsEncryptable(this IDataItem fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException("fileInfo");
            }

            foreach (Regex filter in pathFilters)
            {
                if (filter.IsMatch(fileInfo.FullName))
                {
                    return false;
                }
            }
            return !fileInfo.IsEncrypted();
        }

        public static bool AddUnencryptable(Regex regex)
        {
            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }
            pathFilters.Add(regex);
            return true;
        }

        public static bool AddUnencryptableExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            pathFilters.Add(new Regex(@".*\."+ extension  + "$"));
            return true;
        }

        public static void Clear()
        {
            pathFilters.Clear();
        }
    }
}
