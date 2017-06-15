using System;

namespace Axantum.AxCrypt.Abstractions
{
    public interface IBrowser
    {
        void OpenUri(Uri url);
    }
}