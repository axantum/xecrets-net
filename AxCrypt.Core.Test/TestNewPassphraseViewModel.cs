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

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestNewPassphraseViewModel
    {
        private static IList<Passphrase> _identities;

        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);

            _identities = new List<Passphrase>();
            Mock<FileSystemState> fileSystemStateMock = new Mock<FileSystemState>();
            fileSystemStateMock.Setup<IList<Passphrase>>(f => f.KnownPassphrases).Returns(_identities);
            TypeMap.Register.Singleton<FileSystemState>(() => fileSystemStateMock.Object);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestConstructorWithoutKnownDefaultIdentity()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            Assert.That(npvm.PasswordText, Is.EqualTo(""));
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestConstructorWithKnownDefaultIdentity(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            _identities.Add(Passphrase.Empty);
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            Assert.That(npvm.PasswordText, Is.EqualTo(String.Empty));
        }

        [Test]
        public static void TestShowPassphrase()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel("Identity", String.Empty);

            Assert.That(npvm.ShowPassword, Is.False);

            npvm.ShowPassword = true;

            Assert.That(npvm.ShowPassword, Is.True);
        }

        [Test]
        public static void TestValidateIdentity()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel("Identity", String.Empty);

            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePropertyThatCannotBeValidated()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm[nameof(NewPasswordViewModel.ShowPassword)]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [Test]
        public static void TestValidatePassphraseOk()
        {
            TypeMap.Register.New<AxCryptFactory>(() => new AxCryptFactory());
            TypeMap.Register.Singleton<PasswordStrengthEvaluator>(() => new PasswordStrengthEvaluator(64, 0));

            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            npvm.PasswordText = "Loadiney4aropRout[";
            npvm.Verification = "Loadiney4aropRout[";

            Assert.That(npvm[nameof(NewPasswordViewModel.PasswordText)], Is.EqualTo(""));
            Assert.That(npvm[nameof(NewPasswordViewModel.Verification)], Is.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.None));
        }

        [Test]
        public static void TestValidatePassphraseNotOk()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);

            npvm.PasswordText = "abc1234";
            npvm.Verification = "abc12345";

            Assert.That(npvm[nameof(NewPasswordViewModel.Verification)], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.VerificationPassphraseWrong));
        }

        [Test]
        public static void TestValidateNonExistingPropertyName()
        {
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, String.Empty);
            string s = null;
            Assert.Throws<ArgumentException>(() => { s = npvm["NonExisting"]; });
            Assert.That(s, Is.Null, "Not a real assertion, only to make the variable used for FxCop.");
        }

        [TestCase(CryptoImplementation.Mono)]
        [TestCase(CryptoImplementation.WindowsDesktop)]
        [TestCase(CryptoImplementation.BouncyCastle)]
        public static void TestValidateWrongPassphraseWithRealFile(CryptoImplementation cryptoImplementation)
        {
            SetupAssembly.AssemblySetup(cryptoImplementation);

            FakeDataStore.AddFile(@"C:\My Folder\MyFile-txt.axx", new MemoryStream(Resources.helloworld_key_a_txt));
            NewPasswordViewModel npvm = new NewPasswordViewModel(String.Empty, @"C:\My Folder\MyFile-txt.axx");
            npvm.PasswordText = "b";
            npvm.Verification = "b";

            Assert.That(npvm[nameof(NewPasswordViewModel.PasswordText)], Is.Not.EqualTo(""));
            Assert.That(npvm.ValidationError, Is.EqualTo((int)ValidationError.WrongPassphrase));
        }
    }
}
