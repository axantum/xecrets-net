using System;

namespace AxCrypt.Abstractions
{
    public interface IBrowser
    {
        void OpenUri(Uri url);
    }
}