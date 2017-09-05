using Axantum.AxCrypt.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.IO
{
    public class FileFilter
    {
        private readonly List<Regex> pathFilters;
        private readonly List<string> forbiddenFolderFilters;

        public FileFilter()
        {
            pathFilters = new List<Regex>();
            forbiddenFolderFilters = new List<string>();
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
            foreach (string filter in forbiddenFolderFilters)
            {
                if (folder.NormalizeFolderPath().ToLower().StartsWith(filter.ToLower()))
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

        public bool AddForbiddenFolderFilters(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            forbiddenFolderFilters.Add(path);
            return true;
        }
    }
}