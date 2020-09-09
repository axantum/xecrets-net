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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Runtime;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestPassphraseBase
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        private class TestingPassphraseBase : DerivedKeyBase
        {
            public TestingPassphraseBase(SymmetricKey derivedKey)
            {
                DerivedKey = derivedKey;
            }
        }

        [Test]
        public static void TestEquals()
        {
            SymmetricKey key1 = new SymmetricKey(128);
            TestingPassphraseBase p1a = new TestingPassphraseBase(key1);
            TestingPassphraseBase p1b = new TestingPassphraseBase(key1);
            TestingPassphraseBase nullPassphraseBase = null;

            Assert.That(p1a.Equals(p1b));
            Assert.That(p1b.Equals(p1a));
            Assert.That(p1a.Equals(p1a));
            Assert.That(p1b.Equals(p1b));
            Assert.That(!p1a.Equals(nullPassphraseBase));

            object p1aObject = p1a;
            object p1bObject = p1b;

            Assert.That(p1aObject.Equals(p1bObject));
            Assert.That(p1bObject.Equals(p1aObject));
            Assert.That(p1aObject.Equals(p1aObject));
            Assert.That(p1bObject.Equals(p1bObject));
            Assert.That(!p1aObject.Equals((object)nullPassphraseBase));
        }
    }
}