using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface IPortableFactory
    {
        ISemaphore Semaphore(int initialCount, int maximumCount);

        IPath Path();

        IThreadWorker ThreadWorker(IProgressContext progress, bool startSerializedOnUIThread);

        ISingleThread SingleThread();

        IBlockingBuffer BlockingBuffer();
    }
}