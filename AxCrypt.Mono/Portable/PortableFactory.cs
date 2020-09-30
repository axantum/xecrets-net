#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
 *
 * This file is part of AxCrypt.
 *
 * AxCrypt is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AxCrypt is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AxCrypt.  If not, see <http://www.gnu.org/licenses/>.
 *
 * The source is maintained at http://bitbucket.org/AxCrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Portable;
using AxCrypt.Mono.Cryptography;
using System;
using System.Linq;

namespace AxCrypt.Mono.Portable
{
    public class PortableFactory : IPortableFactory
    {
        public static Abstractions.Algorithm.AxCryptHMACSHA1 AxCryptHMACSHA1()
        {
            return new AxCryptHMACSHA1Wrapper();
        }

        public static HMACSHA512 HMACSHA512()
        {
            return new Mono.Cryptography.HMACSHA512Wrapper(new System.Security.Cryptography.HMACSHA512());
        }

        public static Aes AesManaged()
        {
            return new Mono.Cryptography.AesWrapper(new System.Security.Cryptography.AesManaged());
        }

        public static CryptoStreamBase CryptoStream()
        {
            return new Mono.Cryptography.CryptoStreamWrapper();
        }

        public static Sha1 SHA1Managed()
        {
            return new Mono.Cryptography.Sha1Wrapper(new System.Security.Cryptography.SHA1Managed());
        }

        public static Sha256 SHA256Managed()
        {
            return new Mono.Cryptography.Sha256Wrapper(new System.Security.Cryptography.SHA256Managed());
        }

        public static RandomNumberGenerator RandomNumberGenerator()
        {
            return new RandomNumberGeneratorWrapper(System.Security.Cryptography.RandomNumberGenerator.Create());
        }

        public ISemaphore Semaphore(int initialCount, int maximumCount)
        {
            return new PortableSemaphoreWrapper(new System.Threading.Semaphore(initialCount, maximumCount));
        }

        public IPath Path()
        {
            return new PortablePath();
        }

        public Core.Runtime.IThreadWorker ThreadWorker(string name, Core.UI.IProgressContext progress, bool startSerializedOnUIThread)
        {
            return new ThreadWorker(name, progress, startSerializedOnUIThread);
        }

        public ISingleThread SingleThread()
        {
            return new SingleThread();
        }

        public IBlockingBuffer BlockingBuffer()
        {
            return new BlockingBuffer();
        }
    }
}