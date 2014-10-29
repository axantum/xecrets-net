using Axantum.AxCrypt.Core.Portable;
using System;
using System.IO;
using System.Linq;

namespace Axantum.AxCrypt.Mono.Portable
{
    public class PortableFactory : IPortableFactory
    {
        public static Core.Portable.AxCryptHMACSHA1 AxCryptHMACSHA1()
        {
            return new Mono.Cryptography.AxCryptHMACSHA1Wrapper();
        }

        public static Core.Portable.HMACSHA512 HMACSHA512()
        {
            return new Mono.Cryptography.HMACSHA512Wrapper();
        }

        public static Core.Portable.AesManaged AesManaged()
        {
            return new Mono.Cryptography.AesManagedWrapper();
        }

        public static Core.Portable.CryptoStream CryptoStream()
        {
            return new Mono.Cryptography.CryptoStreamWrapper();
        }

        public static Core.Portable.Sha1 SHA1Managed()
        {
            return new Mono.Cryptography.Sha1Wrapper(new System.Security.Cryptography.SHA1Managed());
        }

        public static Core.Portable.Sha256 SHA256Managed()
        {
            return new Mono.Cryptography.Sha256Wrapper(new System.Security.Cryptography.SHA256Managed());
        }

        public RandomNumberGenerator RandomNumberGenerator()
        {
            return new PortableRandomNumberGeneratorWrapper(System.Security.Cryptography.RandomNumberGenerator.Create());
        }

        public ISemaphore Semaphore(int initialCount, int maximumCount)
        {
            return new PortableSemaphoreWrapper(new System.Threading.Semaphore(initialCount, maximumCount));
        }

        public IPath Path()
        {
            return new PortablePathImplementation();
        }

        public Core.Runtime.IThreadWorker ThreadWorker(Core.UI.IProgressContext progress, bool startSerializedOnUIThread)
        {
            return new ThreadWorker(progress, startSerializedOnUIThread);
        }

        public ISingleThread SingleThread()
        {
            return new SingleThread();
        }
    }
}