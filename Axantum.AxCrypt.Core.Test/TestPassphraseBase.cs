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
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestPassphraseBase
    {
        [SetUp]
        public static void Setup()
        {
            Factory.Instance.Singleton<IRuntimeEnvironment>(() => new FakeRuntimeEnvironment());
            Factory.Instance.Singleton<IRandomGenerator>(() => new FakeRandomGenerator());
        }

        [TearDown]
        public static void Teardown()
        {
            Factory.Instance.Clear();
        }

        private class TestingPassphraseBase : PassphraseBase
        {
            public TestingPassphraseBase(Guid cryptoId, SymmetricKey derivedKey, string passphrase)
            {
                CryptoId = cryptoId;
                DerivedKey = derivedKey;
                Passphrase = passphrase;
            }
        }

        [Test]
        public static void TestEquals()
        {
            SymmetricKey key1 = new SymmetricKey(128);
            Guid id = new Guid();
            TestingPassphraseBase p1a = new TestingPassphraseBase(id, key1, "passphrase");
            TestingPassphraseBase p1b = new TestingPassphraseBase(id, key1, "passphrase");
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