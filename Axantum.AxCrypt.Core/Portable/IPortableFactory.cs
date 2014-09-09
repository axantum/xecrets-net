using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Portable
{
    public interface IPortableFactory
    {
        HMAC AxCryptHMACSHA1(SymmetricKey key);

        HMAC HMACSHA512(byte[] key);

        SymmetricAlgorithm AesManaged();

        HashAlgorithm SHA1Managed();

        RandomNumberGenerator RandomNumberGenerator();

        Stream CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode);

        ISemaphore Semaphore(int initialCount, int maximumCoiunt);

        IPath Path();

        IThreadWorker ThreadWorker(IProgressContext progress, bool startSerializedOnUIThread);

        ISingleThread SingleThread();
    }
}