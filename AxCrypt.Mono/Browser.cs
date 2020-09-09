using AxCrypt.Abstractions;
using System;
using System.Diagnostics;

namespace AxCrypt.Mono
{
    public class Browser : IBrowser
    {
        public void OpenUri(Uri url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            Process.Start(url.ToString());
        }
    }
}