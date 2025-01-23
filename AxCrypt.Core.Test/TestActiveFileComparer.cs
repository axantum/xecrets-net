﻿#region Xecrets Cli Copyright and GPL License notice

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
using AxCrypt.Core.Crypto;
using AxCrypt.Core.IO;
using AxCrypt.Core.Session;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFileComparer
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestDateComparer()
        {
            ((FakeNow)New<INow>()).TimeFunction = () => new DateTime(2013, 01, 01);
            ActiveFile activeFile1a = new ActiveFile(New<IDataStore>((@"C:\encrypted1.axx")), New<IDataStore>(@"C:\decrypted1.txt"), new LogOnIdentity("activeFile1a"), ActiveFileStatus.NotDecrypted, new V2Aes256CryptoFactory().CryptoId);

            ((FakeNow)New<INow>()).TimeFunction = () => new DateTime(2013, 01, 01);
            ActiveFile activeFile1b = new ActiveFile(New<IDataStore>((@"C:\encrypted2.axx")), New<IDataStore>(@"C:\decrypted2.txt"), new LogOnIdentity("activeFile1b"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);

            ActiveFileComparer comparer = ActiveFileComparer.DateComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            ((FakeNow)New<INow>()).TimeFunction = () => new DateTime(2013, 01, 02);
            ActiveFile activeFile2 = new ActiveFile(New<IDataStore>((@"C:\encrypted3.axx")), New<IDataStore>(@"C:\decrypted3.txt"), new LogOnIdentity("activeFile2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);

            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.LessThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.GreaterThan(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.LessThan(0));
            comparer.ReverseSort = false;
        }

        [Test]
        public static void TestEncryptedNameComparer()
        {
            ActiveFile activeFile1a = new ActiveFile(New<IDataStore>((@"C:\encrypted1.axx")), New<IDataStore>(@"C:\decrypted1a.txt"), new LogOnIdentity("activeFile1a"), ActiveFileStatus.NotDecrypted, new V2Aes256CryptoFactory().CryptoId);
            ActiveFile activeFile1b = new ActiveFile(New<IDataStore>((@"C:\encrypted1.axx")), New<IDataStore>(@"C:\decrypted1b.txt"), new LogOnIdentity("activeFile1b"), ActiveFileStatus.NotDecrypted, new V2Aes256CryptoFactory().CryptoId);

            ActiveFileComparer comparer = ActiveFileComparer.EncryptedNameComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            ActiveFile activeFile2 = new ActiveFile(New<IDataStore>((@"C:\encrypted2.axx")), New<IDataStore>(@"C:\decrypted1a.txt"), new LogOnIdentity("activeFile2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);

            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.LessThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.GreaterThan(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.LessThan(0));
            comparer.ReverseSort = false;
        }

        [Test]
        public static void TestDecryptedNameComparer()
        {
            ActiveFile activeFile1a = new ActiveFile(New<IDataStore>((@"C:\encrypted1a.axx")), New<IDataStore>(@"C:\decrypted1.txt"), new LogOnIdentity("activeFile1a"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            ActiveFile activeFile1b = new ActiveFile(New<IDataStore>((@"C:\encrypted1b.axx")), New<IDataStore>(@"C:\decrypted1.txt"), new LogOnIdentity("activeFile1b"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);

            ActiveFileComparer comparer = ActiveFileComparer.DecryptedNameComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            ActiveFile activeFile2 = new ActiveFile(New<IDataStore>((@"C:\encrypted1a.axx")), New<IDataStore>(@"C:\decrypted2.txt"), new LogOnIdentity("activeFile2"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);

            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.LessThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.GreaterThan(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.LessThan(0));
            comparer.ReverseSort = false;
        }
    }
}
