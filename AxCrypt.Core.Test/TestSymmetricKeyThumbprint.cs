﻿#region Xecrets Cli Copyright and GPL License notice

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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Core.Crypto;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestSymmetricKeyThumbprint
    {
        private CryptoImplementation _cryptoImplementation;

        public TestSymmetricKeyThumbprint(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestInvalidArguments()
        {
            Passphrase nullKey = null;
            Salt nullSalt = null;
            Assert.Throws<ArgumentNullException>(() => { if (new SymmetricKeyThumbprint(nullKey, new Salt(128), 10) == null) { } });
            Assert.Throws<ArgumentNullException>(() => { if (new SymmetricKeyThumbprint(new Passphrase("passphrase"), nullSalt, 10) == null) { } });
        }

        [Test]
        public void TestAesKeyThumbprintMethods()
        {
            Passphrase key1 = new Passphrase("key");
            Passphrase key2 = new Passphrase("key");
            Salt salt1 = new Salt(512);
            Salt salt2 = new Salt(salt1.GetBytes());

            SymmetricKeyThumbprint thumbprint1 = new SymmetricKeyThumbprint(key1, salt1, 10);
            SymmetricKeyThumbprint thumbprint2 = new SymmetricKeyThumbprint(key2, salt2, 10);

            Assert.That(thumbprint1 == thumbprint2, "Two thumb prints made from the same key and salt bytes, although different AesKey instances should be equivalent.");

            SymmetricKeyThumbprint thumbprint3 = new SymmetricKeyThumbprint(new Passphrase("passphrase"), new Salt(512), 10);
            Assert.That(thumbprint2 != thumbprint3, "Two very different keys and salts should not be equivalent.");
        }

        [Test]
        public void TestComparisons()
        {
            Passphrase key1 = new Passphrase("samekey");
            Passphrase key2 = new Passphrase("samekey");
            Salt salt1 = new Salt(512);
            Salt salt2 = new Salt(512);

            SymmetricKeyThumbprint thumbprint1a = new SymmetricKeyThumbprint(key1, salt1, 13);
            SymmetricKeyThumbprint thumbprint1a_alias = thumbprint1a;
            SymmetricKeyThumbprint thumbprint1b = new SymmetricKeyThumbprint(key1, salt2, 25);
            SymmetricKeyThumbprint thumbprint2a = new SymmetricKeyThumbprint(key2, salt2, 25);
            SymmetricKeyThumbprint thumbprint2b = new SymmetricKeyThumbprint(key2, salt1, 13);
            SymmetricKeyThumbprint nullThumbprint = null;

            Assert.That(thumbprint1a == thumbprint1a_alias, "Same instance should of course compare equal.");
            Assert.That(nullThumbprint != thumbprint1a, "A null should not compare equal to any other instance.");
            Assert.That(thumbprint1a != nullThumbprint, "A null should not compare equal to any other instance.");
            Assert.That(thumbprint1a == thumbprint2b, "Same raw key and salt, but different instance, should compare equal.");
            Assert.That(thumbprint1b == thumbprint2a, "Same raw key and salt, but different instance, should compare equal.");
            Assert.That(thumbprint1a != thumbprint1b, "Same raw key but different salt, should compare inequal.");
            Assert.That(thumbprint2a != thumbprint2b, "Same raw key but different salt, should compare inequal.");

            object object1a = thumbprint1a;
            object object2b = thumbprint2b;
            Assert.That(object1a.Equals(nullThumbprint), Is.False, "An instance does not equals null.");
            Assert.That(object1a.Equals(object2b), Is.True, "The two instances are equivalent.");

            object badTypeObject = key1;
            Assert.That(object1a.Equals(badTypeObject), Is.False, "The object being compared to is of the wrong type.");
        }

        [Test]
        public void TestGetHashCode()
        {
            Passphrase key1 = new Passphrase("samekey");
            Passphrase key2 = new Passphrase("samekey");
            Salt salt1 = new Salt(512);
            Salt salt2 = new Salt(512);

            SymmetricKeyThumbprint thumbprint1a = new SymmetricKeyThumbprint(key1, salt1, 17);
            SymmetricKeyThumbprint thumbprint1b = new SymmetricKeyThumbprint(key1, salt2, 17);
            SymmetricKeyThumbprint thumbprint2a = new SymmetricKeyThumbprint(key2, salt2, 17);

            Assert.That(thumbprint1a.GetHashCode() != thumbprint1b.GetHashCode(), "The salt is different, so the hash code should be different.");
            Assert.That(thumbprint1b.GetHashCode() == thumbprint2a.GetHashCode(), "The keys are equivalent, and the salt the same, so the hash code should be different.");
        }
    }
}
