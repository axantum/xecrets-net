#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestAesIV
    {
        [Test]
        public static void TestInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new AesIV(null);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                new AesIV(new byte[0]);
            });

            Assert.Throws<InternalErrorException>(() =>
            {
                new AesIV(new byte[32]);
            });
        }

        [Test]
        public static void TestMethods()
        {
            AesIV zeroIV = AesIV.Zero;
            Assert.That(zeroIV.GetBytes(), Is.EquivalentTo(new byte[16]), "The IV 'zero' should consist of all zeros.");

            AesIV iv = new AesIV(new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            Assert.That(iv.GetBytes(), Is.EquivalentTo(new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }), "An IV specified should consist of just those bytes.");

            iv = new AesIV();
            Assert.That(iv.GetBytes(), Is.Not.EquivalentTo(new byte[16]), "A random iv will in practice never be all zeros.");
        }
    }
}