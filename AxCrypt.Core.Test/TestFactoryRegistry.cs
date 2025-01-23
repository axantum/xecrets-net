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
using AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFactoryRegistry
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(Fake.CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        private class MyClass { };

        [Test]
        public static void TestRegisterParameterless()
        {
            bool wasCalled = false;
            MyClass theObject = new MyClass();
            TypeMap.Register.New<MyClass>(() => { wasCalled = true; return theObject; });

            object value = New<MyClass>();
            Assert.That(value, Is.EqualTo(theObject));
            Assert.That(wasCalled, Is.True);
        }

        [Test]
        public static void TestRegisterObjectThrows()
        {
            Assert.Throws<ArgumentException>(() => TypeMap.Register.New<object>(() => { return new object(); }));
        }

        [Test]
        public static void TestRegisterWithParameter()
        {
            bool wasCalled = false;
            TypeMap.Register.New<int, int>((argument) => { wasCalled = true; return argument; });

            int value = New<int, int>(27);
            Assert.That(value, Is.EqualTo(27));
            Assert.That(wasCalled, Is.True);
        }

        [Test]
        public static void TestDefault()
        {
            AxCryptFile axCryptFile = New<AxCryptFile>();
            Assert.That(axCryptFile is AxCryptFile, Is.True);

            TypeMap.Register.New<FileSystemState>(() => new FileSystemState());
            ActiveFileAction actions = New<ActiveFileAction>();
            Assert.That(actions is ActiveFileAction, Is.True);
        }

        [Test]
        public static void TestNotRegistered()
        {
            Assert.Throws<ArgumentException>(() => New<object>());
            Assert.Throws<ArgumentException>(() => New<int, int>(13));
        }

        [Test]
        public static void TestNotRegisteredSingleton()
        {
            Assert.Throws<ArgumentException>(() => New<object>());
        }

        private class MyDisposable : IDisposable
        {
            public bool IsDisposed { get; set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        [Test]
        public static void TestClearDisposable()
        {
            TypeMap.Register.Singleton(() => new MyDisposable());

            MyDisposable md = New<MyDisposable>();
            Assert.That(md.IsDisposed, Is.False);

            TypeMap.Register.Clear();
            Assert.That(md.IsDisposed, Is.True);
        }

        [Test]
        public static void TestSetDisposableSingletonTwice()
        {
            TypeMap.Register.Singleton(() => new MyDisposable());

            MyDisposable md = New<MyDisposable>();
            Assert.That(md.IsDisposed, Is.False);

            TypeMap.Register.Singleton(() => new MyDisposable());
            Assert.That(md.IsDisposed, Is.True);
        }
    }
}
