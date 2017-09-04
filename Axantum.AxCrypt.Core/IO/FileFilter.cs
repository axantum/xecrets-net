using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public bool IsForbiddenFolder(string folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException("folder");
            }
            foreach (Regex filter in pathFilters)
            {
                if (filter.IsMatch(folder))
                {
                    return true;
                }
            }
            return false;
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
            pathFilters.Add(new Regex(@".*\." + extension + "$"));
            return true;
        }
    }
}