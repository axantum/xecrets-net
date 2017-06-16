using Axantum.AxCrypt.Abstractions;
using System;
using System.Diagnostics;

namespace Axantum.AxCrypt.Mono
{
    public class Browser : IBrowser
    {
        public void OpenUri(Uri url)
        {
            Process.Start(url.ToString());
        }
    }
}