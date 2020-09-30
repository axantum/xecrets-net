using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using System;
using System.Linq;

namespace AxCrypt.Core.Portable
{
    public interface IPortableFactory
    {
        ISemaphore Semaphore(int initialCount, int maximumCount);

        IPath Path();

        IThreadWorker ThreadWorker(string name, IProgressContext progress, bool startSerializedOnUIThread);

        ISingleThread SingleThread();

        IBlockingBuffer BlockingBuffer();
    }
}