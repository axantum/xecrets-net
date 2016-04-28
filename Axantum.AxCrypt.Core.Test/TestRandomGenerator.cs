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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestRandomGenerator
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestGenerate()
        {
            RandomGenerator generator = new RandomGenerator();

            byte[] randomBytes = generator.Generate(100);
            Assert.That(randomBytes.Length, Is.EqualTo(100), "Ensuring we really got the right number of bytes.");
            Assert.That(randomBytes, Is.Not.EquivalentTo(new byte[100]), "It is not in practice possible that all zero bytes are returned by GetRandomBytes().");

            randomBytes = generator.Generate(1000);
            double average = randomBytes.Average(b => b);
            Assert.That(average >= 115 && average <= 140, "Unscientific, but the sample sequence should not vary much from a mean of 127.5, but was {0}".InvariantFormat(average));
        }
    }
}