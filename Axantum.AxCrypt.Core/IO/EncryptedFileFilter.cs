using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.UI;
using AxCrypt.Content;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

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

        public bool IsOpenable(string decryptedFileName)
        {
            if (decryptedFileName == null)
            {
                throw new ArgumentNullException("decryptedFileName");
            }

            foreach (Regex filter in pathFilters)
            {
                if (filter.IsMatch(decryptedFileName))
                {
                    New<Abstractions.IUIThread>().PostTo(async () => await New<IPopup>().ShowAsync(PopupButtons.Ok, Texts.WarningTitle, Texts.IgnoreFileOpenWarningText.InvariantFormat(decryptedFileName), Common.DoNotShowAgainOptions.IgnoreFileWarning));
                    return false;
                }
            }
            return true;
        }
    }
}