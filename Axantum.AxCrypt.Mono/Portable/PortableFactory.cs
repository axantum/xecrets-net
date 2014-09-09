using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Portable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Mono.Portable
{
    public class PortableFactory : IPortableFactory
    {
        public HMAC AxCryptHMACSHA1(SymmetricKey key)
        {
            return new PortableHmacWrapper(Axantum.AxCrypt.Mono.Portable.AxCryptHMACSHA1.Create(key));
        }

        public HMAC HMACSHA512(byte[] key)
        {
            return new PortableHmacWrapper(new System.Security.Cryptography.HMACSHA512(key));
        }

        public SymmetricAlgorithm AesManaged()
        {
            return new PortableSymmetricAlgorithmWrapper(new System.Security.Cryptography.AesManaged());
        }

        public HashAlgorithm SHA1Managed()
        {
            return new PortableHashAlgorithmWrapper(new System.Security.Cryptography.SHA1Managed());
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