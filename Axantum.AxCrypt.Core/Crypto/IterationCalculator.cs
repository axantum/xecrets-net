#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axantum.com for more information about the author.
*/

#endregion Coypright and License

using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Crypto
{
    public class IterationCalculator
    {
        /// <summary>
        /// Get the number of key wrap iterations we use by default. This is a calculated value intended to cause the wrapping
        /// operation to take approximately 1/10th of a second in the system where the code is run.
        /// A minimum of 20000 iterations are always guaranteed.
        /// </summary>
        public virtual long V1KeyWrapIterations()
        {
            long iterationsPerSecond = IterationsPerSecond(V1KeyWrapIterate);
            long defaultIterations = iterationsPerSecond / 10;

            if (defaultIterations < 20000)
            {
                defaultIterations = 20000;
            }

            return defaultIterations;
        }

        public virtual long V2KeyWrapIterations()
        {
            long iterationsPerSecond = IterationsPerSecond(V2KeyWrapIterate);
            long defaultIterations = iterationsPerSecond / 20;

            if (defaultIterations < 10000)
            {
                defaultIterations = 10000;
            }

            return defaultIterations;
        }

        private static long IterationsPerSecond(Func<int, object> iterate)
        {
            int iterationsIncrement = 1000;
            long totalIterations = 0;
            DateTime startTime = OS.Current.UtcNow;
            DateTime endTime;
            do
            {
                iterate(iterationsIncrement);
                totalIterations += iterationsIncrement;
                endTime = OS.Current.UtcNow;
            } while ((endTime - startTime).TotalMilliseconds < 500);
            long iterationsPerSecond = totalIterations * 1000 / (long)(endTime - startTime).TotalMilliseconds;
            return iterationsPerSecond;
        }

        private static object V1KeyWrapIterate(int iterations)
        {
            ICrypto dummyCrypto = new V1AesCrypto(new GenericPassphrase("A dummy passphrase"));
            KeyWrapSalt dummySalt = new KeyWrapSalt(16);
            KeyWrap keyWrap = new KeyWrap(dummySalt, iterations, KeyWrapMode.AxCrypt);
            return keyWrap.Wrap(dummyCrypto, new SymmetricKey(128));
        }

        private static object V2KeyWrapIterate(int iterations)
        {
            ICrypto dummyCrypto = new V2AesCrypto(new GenericPassphrase(new SymmetricKey(256)));
            KeyWrapSalt dummySalt = new KeyWrapSalt(32);
            KeyWrap keyWrap = new KeyWrap(dummySalt, iterations, KeyWrapMode.AxCrypt);
            return keyWrap.Wrap(dummyCrypto, new SymmetricKey(256));
        }
    }
}