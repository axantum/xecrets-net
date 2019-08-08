using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Axantum.AxCrypt.Core.IO
{
    public class EncryptedFileFilter : IEncryptedFileFilter
    {
        private readonly List<Regex> pathFilters;

        public EncryptedFileFilter()
        {
            pathFilters = new List<Regex>();
            InitializeUnopenableExtensions();
        }

        private void InitializeUnopenableExtensions()
        {
            AddUnopenableExtension("zip");
            AddUnopenableExtension("rar");
            AddUnopenableExtension("gz");
            AddUnopenableExtension("7z");
            AddUnopenableExtension("cpgz");
            AddUnopenableExtension("cpio");
            AddUnopenableExtension("cpgz");
        }

        private bool AddUnopenableExtension(string extension)
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