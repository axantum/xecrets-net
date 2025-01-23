#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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
using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Algorithm;
using AxCrypt.Core.Crypto;
using AxCrypt.Core.Crypto.Asymmetric;
using AxCrypt.Core.Header;
using AxCrypt.Core.Portable;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using AxCrypt.Mono;
using AxCrypt.Mono.Portable;

using NUnit.Framework;

using Xecrets.Net.Api.Implementation;
using Xecrets.Net.Core;
using Xecrets.Net.Core.Crypto.Asymmetric;
using Xecrets.Net.Core.Test.LegacyImplementation;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestV2AsymmetricRecipientsEncryptedHeaderBlock
    {
        private CryptoImplementation _cryptoImplementation;

        public TestV2AsymmetricRecipientsEncryptedHeaderBlock(CryptoImplementation cryptoImplementation)
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
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestGetSetRecipientsAndClone()
        {
            V2AsymmetricRecipientsEncryptedHeaderBlock headerBlock = new V2AsymmetricRecipientsEncryptedHeaderBlock(new V2AesCrypto(SymmetricKey.Zero256, SymmetricIV.Zero128, 0));
            IAsymmetricKeyPair aliceKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);
            IAsymmetricKeyPair bobKeyPair = New<IAsymmetricFactory>().CreateKeyPair(1024);

            List<UserPublicKey> publicKeys = new List<UserPublicKey>();
            publicKeys.Add(new UserPublicKey(EmailAddress.Parse("alice@email.com"), aliceKeyPair.PublicKey));
            publicKeys.Add(new UserPublicKey(EmailAddress.Parse("bob@email.com"), bobKeyPair.PublicKey));
            Recipients recipients = new Recipients(publicKeys);
            headerBlock.Recipients = recipients;
            Assert.That(headerBlock.Recipients.PublicKeys.ToList()[0].Email, Is.EqualTo(EmailAddress.Parse("alice@email.com")));
            Assert.That(headerBlock.Recipients.PublicKeys.ToList()[1].Email, Is.EqualTo(EmailAddress.Parse("bob@email.com")));

            V2AsymmetricRecipientsEncryptedHeaderBlock clone = (V2AsymmetricRecipientsEncryptedHeaderBlock)headerBlock.Clone();
            Assert.That(clone.Recipients.PublicKeys.ToList()[0].Email, Is.EqualTo(EmailAddress.Parse("alice@email.com")));
            Assert.That(clone.Recipients.PublicKeys.ToList()[0].PublicKey.ToString(), Is.EqualTo(aliceKeyPair.PublicKey.ToString()));
            Assert.That(clone.Recipients.PublicKeys.ToList()[1].Email, Is.EqualTo(EmailAddress.Parse("bob@email.com")));
            Assert.That(clone.Recipients.PublicKeys.ToList()[1].PublicKey.ToString(), Is.EqualTo(bobKeyPair.PublicKey.ToString()));
        }
    }
}
