#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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

using Xecrets.Net.Implementation.Cryptography;

using AxCryptHMACSHA1 = AxCrypt.Abstractions.Algorithm.AxCryptHMACSHA1;

namespace AxCrypt.Mono.Portable
{
    public class PortableFactory : IPortableFactory
    {
        public static Abstractions.Algorithm.AxCryptHMACSHA1 AxCryptHMACSHA1()
        {
            AxCryptHMACSHA1 hmac = new AxCrypt1HmacSha1Wrapper(new AxCrypt1HmacSha1CryptoServiceProvider());
            return hmac;
        }

        public static HMACSHA512 HMACSHA512()
        {
            return new Mono.Cryptography.HMACSHA512Wrapper(new System.Security.Cryptography.HMACSHA512());
        }

        public static Aes AesManaged()
        {
            return new Mono.Cryptography.AesWrapper(System.Security.Cryptography.Aes.Create());
        }

        public static CryptoStreamBase CryptoStream()
        {
            return new Mono.Cryptography.CryptoStreamWrapper();
        }

        public static Sha1 SHA1Managed()
        {
            return new Mono.Cryptography.Sha1Wrapper(System.Security.Cryptography.SHA1.Create());
        }

        public static Sha256 SHA256Managed()
        {
            return new Mono.Cryptography.Sha256Wrapper(System.Security.Cryptography.SHA256.Create());
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
