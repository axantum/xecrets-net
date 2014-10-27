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

        public SymmetricAlgorithm AesManaged()
        {
            return new PortableSymmetricAlgorithmWrapper(new System.Security.Cryptography.AesManaged());
        }

        public HashAlgorithm SHA1Managed()
        {
            return new PortableHashAlgorithmWrapper(new System.Security.Cryptography.SHA1Managed());
        }

        public HashAlgorithm SHA256Managed()
        {
            return new PortableHashAlgorithmWrapper(new System.Security.Cryptography.SHA256Managed());
        }

        public RandomNumberGenerator RandomNumberGenerator()
        {
            return new PortableRandomNumberGeneratorWrapper(System.Security.Cryptography.RandomNumberGenerator.Create());
        }

        public Stream CryptoStream(Stream stream, ICryptoTransform transform, CryptoStreamMode mode)
        {
            System.Security.Cryptography.CryptoStreamMode streamMode;
            switch (mode)
            {
                case CryptoStreamMode.Read:
                    streamMode = System.Security.Cryptography.CryptoStreamMode.Read;
                    break;

                case CryptoStreamMode.Write:
                    streamMode = System.Security.Cryptography.CryptoStreamMode.Write;
                    break;

                default:
                    streamMode = (System.Security.Cryptography.CryptoStreamMode)mode;
                    break;
            }
            return new PortableCryptoStreamWrapper(new System.Security.Cryptography.CryptoStream(stream, new CryptographyCryptoTransformWrapper(transform), streamMode));
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