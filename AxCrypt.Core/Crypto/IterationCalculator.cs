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

using AxCrypt.Abstractions;
using System;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Crypto
{
    public class IterationCalculator
    {
        private class WrapIterator
        {
            private ICrypto _dummyCrypto;

            private Salt _dummySalt;

            private SymmetricKey _dummyKey;

            public WrapIterator(Guid cryptoId)
            {
                ICryptoFactory factory = Resolve.CryptoFactory.Create(cryptoId);
                _dummyCrypto = factory.CreateCrypto(factory.CreateDerivedKey(new Passphrase("A dummy passphrase")).DerivedKey, null, 0);
                _dummySalt = new Salt(_dummyCrypto.Key.Size);
                _dummyKey = new SymmetricKey(_dummyCrypto.Key.Size);
            }

            public void Iterate(long keyWrapIterations)
            {
                KeyWrap keyWrap = new KeyWrap(_dummySalt, keyWrapIterations, KeyWrapMode.Specification);
                keyWrap.Wrap(_dummyCrypto, _dummyKey);
            }
        }

        /// <summary>
        /// Get the number of key wrap iterations we use by default. This is a calculated value intended to cause the wrapping
        /// operation to take approximately 1/20th of a second in the system where the code is run.
        /// A minimum of 5000 iterations are always guaranteed.
        /// </summary>
        /// <param name="cryptoId">The id of the crypto to use for the wrap.</param>
        public virtual long KeyWrapIterations(Guid cryptoId)
        {
            DateTime startTime = New<INow>().Utc;
            WrapIterator wrapIterator = new WrapIterator(cryptoId);

            long iterationsPerSecond = IterationsPerSecond(startTime, wrapIterator.Iterate);
            long defaultIterations = iterationsPerSecond / 20;

            if (defaultIterations < 5000)
            {
                defaultIterations = 5000;
            }

            return defaultIterations;
        }

        private static long IterationsPerSecond(DateTime startTime, Action<long> iterate)
        {
            long iterationsIncrement = 1000;
            long totalIterations = 0;
            DateTime endTime;
            do
            {
                iterate(iterationsIncrement);
                totalIterations += iterationsIncrement;
                endTime = New<INow>().Utc;
            } while ((endTime - startTime).TotalMilliseconds < 500);
            long iterationsPerSecond = totalIterations * 1000 / (long)(endTime - startTime).TotalMilliseconds;
            return iterationsPerSecond;
        }
    }
}