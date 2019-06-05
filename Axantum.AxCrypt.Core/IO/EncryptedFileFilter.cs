using Axantum.AxCrypt.Core.Extensions;
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

        public void InitializeUnopenableExtensions()
        {
            AddUnopenableExtension("zip");
            AddUnopenableExtension("rar");
            AddUnopenableExtension("gz");
            AddUnopenableExtension("7z");
        }

        public bool AddUnopenableExtension(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }
            pathFilters.Add(new Regex(@".*\-" + extension + "." + OS.Current.AxCryptExtension + "$"));
            return true;
        }

        public bool IsOpenable(IDataItem fileInfo)
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
            return true;
        }
    }
}