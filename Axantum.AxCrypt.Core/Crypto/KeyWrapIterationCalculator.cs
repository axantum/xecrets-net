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
    public class KeyWrapIterationCalculator
    {
        /// <summary>
        /// Get the number of key wrap iterations we use by default. This is a calculated value intended to cause the wrapping
        /// operation to take approximately 1/10th of a second in the system where the code is run.
        /// A minimum of 20000 iterations are always guaranteed.
        /// </summary>
        public virtual long Iterations()
        {
            AesKey dummyKey = new AesKey(128);
            KeyWrapSalt dummySalt = new KeyWrapSalt(dummyKey.Length);
            DateTime startTime = OS.Current.UtcNow;
            DateTime endTime;
            long totalIterations = 0;
            int iterationsIncrement = 1000;
            using (KeyWrap keyWrap = new KeyWrap(dummyKey, dummySalt, iterationsIncrement, KeyWrapMode.AxCrypt))
            {
                do
                {
                    keyWrap.Wrap(dummyKey);
                    totalIterations += iterationsIncrement;
                    endTime = OS.Current.UtcNow;
                } while ((endTime - startTime).TotalMilliseconds < 500);
            }
            long iterationsPerSecond = totalIterations * 1000 / (long)(endTime - startTime).TotalMilliseconds;
            long defaultIterations = iterationsPerSecond / 10;

            if (defaultIterations < 20000)
            {
                defaultIterations = 20000;
            }

            return defaultIterations;
        }
    }
}