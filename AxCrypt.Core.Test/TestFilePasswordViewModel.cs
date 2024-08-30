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
using AxCrypt.Core.Session;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Core.UI;
using AxCrypt.Core.UI.ViewModel;
using AxCrypt.Fake;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestFilePasswordViewModel
    {
        private static IList<Passphrase> _identities;

        private CryptoImplementation _cryptoImplementation;

        public TestFilePasswordViewModel(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);

            _identities = new List<Passphrase>();
            Mock<FileSystemState> fileSystemStateMock = new Mock<FileSystemState>();
            fileSystemStateMock.Setup<IList<Passphrase>>(f => f.KnownPassphrases).Returns(_identities);
            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);

            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestConstructor()
        {
            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);

            Assert.That(lovm.PasswordText, Is.EqualTo(""));
        }

        [Test]
        public void TestShowPassphrase()
        {
            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);

            Assert.That(lovm.ShowPassword, Is.False);

            lovm.ShowPassword = true;

            Assert.That(lovm.ShowPassword, Is.True);
        }

        [Test]
        public void TestValidatePropertyThatCannotBeValidated()
        {
            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm[nameof(FilePasswordViewModel.ShowPassword)]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public void TestValidatePassphraseOk()
        {
            New<UserSettings>().ThumbprintSalt = Salt.Zero;
            New<UserSettings>().SetKeyWrapIterations(new V1Aes128CryptoFactory().CryptoId, 10);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);

            _identities.Add(new Passphrase("abc1234"));

            lovm.PasswordText = "abc1234";

            Assert.That(lovm[nameof(FilePasswordViewModel.PasswordText)], Is.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public void TestValidatePassphraseNotOk()
        {
            New<UserSettings>().ThumbprintSalt = Salt.Zero;
            New<UserSettings>().SetKeyWrapIterations(new V1Aes128CryptoFactory().CryptoId, 10);

            Mock<IRuntimeEnvironment> environmentMock = new Mock<IRuntimeEnvironment>();
            environmentMock.Setup<bool>(f => f.IsLittleEndian).Returns(true);
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => environmentMock.Object);

            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);

            _identities.Add(new Passphrase("abc1234"));

            lovm.PasswordText = "abc12345";

            Assert.That(lovm[nameof(FilePasswordViewModel.PasswordText)], Is.Not.EqualTo(""));
            Assert.That(lovm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }

        [Test]
        public void TestValidateNonExistingPropertyName()
        {
            FilePasswordViewModel lovm = new FilePasswordViewModel(String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = lovm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public void TestValidateWrongPassphraseWithRealFile()
        {
            _identities.Add(new Passphrase("a"));

            FakeDataStore.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FilePasswordViewModel npvm = new FilePasswordViewModel(@"C:\My Folder\MyFile-txt.axx");
            npvm.PasswordText = "b";

            Assert.That(npvm[nameof(FilePasswordViewModel.PasswordText)], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }

        [Test]
        public void TestValidateCorrectPassphraseWithRealFile()
        {
            _identities.Add(new Passphrase("a"));

            FakeDataStore.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FilePasswordViewModel npvm = new FilePasswordViewModel(@"C:\My Folder\MyFile-txt.axx");
            npvm.PasswordText = "a";

            Assert.That(npvm[nameof(FilePasswordViewModel.PasswordText)], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public void TestValidateWrongButKnownPassphraseWithRealFile()
        {
            _identities.Add(new Passphrase("b"));

            FakeDataStore.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            FilePasswordViewModel npvm = new FilePasswordViewModel(@"C:\My Folder\MyFile-txt.axx");
            npvm.PasswordText = "b";

            Assert.That(npvm[nameof(FilePasswordViewModel.PasswordText)], Is.Not.EqualTo(String.Empty));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }
    }
}
