using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AxCrypt.Core.IO
{
    public class CanOpenEncryptedFile
    {
        private readonly List<Regex> pathFilters;

        public CanOpenEncryptedFile()
        {
            pathFilters = new List<Regex>();
        }

        public bool AddUnopenableExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            pathFilters.Add(new Regex(@".*\." + extension + "$"));
            return true;
        }

        public bool IsOpenable(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            foreach (Regex filter in pathFilters)
            {
                if (filter.IsMatch(fileName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}