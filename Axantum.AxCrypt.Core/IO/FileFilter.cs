using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Newtonsoft.Json;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileFilter
    {
        private readonly List<Regex> pathFilters;

        public FileFilter()
        {
            pathFilters = new List<Regex>();
        }

        public bool IsEncryptable(IDataItem fileInfo)
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

        public bool AddUnencryptable(Regex regex)
        {
            if (regex == null)
            {
                throw new ArgumentNullException(nameof(regex));
            }
            pathFilters.Add(regex);
            return true;
        }

        public bool AddUnencryptableExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            pathFilters.Add(new Regex(@".*\."+ extension  + "$"));
            return true;
        }

        public void Clear()
        {
            pathFilters.Clear();
        }
    }
}
