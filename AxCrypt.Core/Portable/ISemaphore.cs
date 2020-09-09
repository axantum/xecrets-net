using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxCrypt.Core.Portable
{
    public interface ISemaphore : IDisposable
    {
        void WaitOne();

        void Release();
    }
}