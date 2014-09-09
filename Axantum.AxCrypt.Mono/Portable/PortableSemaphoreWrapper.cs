using Axantum.AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Mono.Portable
{
    internal class PortableSemaphoreWrapper : ISemaphore
    {
        private System.Threading.Semaphore _semaphore;

        public PortableSemaphoreWrapper(System.Threading.Semaphore semaphore)
        {
            _semaphore = semaphore;
        }

        public void WaitOne()
        {
            _semaphore.WaitOne();
        }

        public void Release()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}