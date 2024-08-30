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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Service;
using AxCrypt.Core.Session;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestIdentityPublicTag
    {
        private CryptoImplementation _cryptoImplementation;

        public TestIdentityPublicTag(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void SetUp()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void TearDown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestSimpleThumbprintMatches()
        {
            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new Passphrase("allan")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestSimpleAsymmetricIdentityMatches()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("allan")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same asymmetric key and passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same asymmetric key and passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestAsymmetricIdentityButDifferentPassphraseMatches()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante@axantum.com"), 1024);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("niklas")));

            Assert.That(tag1.Matches(tag2), "tag1 should match tag2 since they are based on the same asymmetric user email and passphrase.");
            Assert.That(tag2.Matches(tag1), "tag2 should match tag1 since they are based on the same asymmetric user email and passphrase.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }

        [Test]
        public void TestDifferentAsymmetricIdentityAndSamePassphraseDoesNotMatch()
        {
            UserKeyPair key1 = new UserKeyPair(EmailAddress.Parse("svante1@axantum.com"), 1024);
            UserKeyPair key2 = new UserKeyPair(EmailAddress.Parse("svante2@axantum.com"), 1024);

            IdentityPublicTag tag1 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key1 }, new Passphrase("allan")));
            IdentityPublicTag tag2 = new IdentityPublicTag(new LogOnIdentity(new UserKeyPair[] { key2 }, new Passphrase("allan")));

            Assert.That(!tag1.Matches(tag2), "tag1 should not match tag2 since they are based on different asymmetric user email even if passphrase is the same.");
            Assert.That(!tag2.Matches(tag1), "tag2 should not match tag1 since they are based on different asymmetric user email even if passphrase is the same.");
            Assert.That(tag1.Matches(tag1), "tag1 should match tag1 since they are the same instance.");
            Assert.That(tag2.Matches(tag2), "tag2 should match tag2 since they are the same instance.");
        }
    }
}
