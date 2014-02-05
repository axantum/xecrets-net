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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFileComparer
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup();
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestDateComparer()
        {
            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2013, 01, 01);
            ActiveFile activeFile1a = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2013, 01, 01);
            ActiveFile activeFile1b = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted2.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted2.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

            ActiveFileComparer comparer = ActiveFileComparer.DateComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            FakeRuntimeEnvironment.Instance.TimeFunction = () => new DateTime(2013, 01, 02);
            ActiveFile activeFile2 = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted3.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted3.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

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
            ActiveFile activeFile1a = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1a.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);
            ActiveFile activeFile1b = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1b.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

            ActiveFileComparer comparer = ActiveFileComparer.EncryptedNameComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            ActiveFile activeFile2 = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted2.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1a.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

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
            ActiveFile activeFile1a = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1a.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);
            ActiveFile activeFile1b = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1b.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted1.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

            ActiveFileComparer comparer = ActiveFileComparer.DecryptedNameComparer;
            Assert.That(comparer.ReverseSort, Is.False);

            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile1b), Is.EqualTo(0));
            Assert.That(comparer.Compare(activeFile1b, activeFile1a), Is.EqualTo(0));
            comparer.ReverseSort = false;

            ActiveFile activeFile2 = new ActiveFile(Factory.New<IRuntimeFileInfo>((@"C:\encrypted1a.axx")), Factory.New<IRuntimeFileInfo>(@"C:\decrypted2.txt"), new AesKey(128), ActiveFileStatus.NotDecrypted);

            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.LessThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.GreaterThan(0));
            comparer.ReverseSort = true;
            Assert.That(comparer.Compare(activeFile1a, activeFile2), Is.GreaterThan(0));
            Assert.That(comparer.Compare(activeFile2, activeFile1a), Is.LessThan(0));
            comparer.ReverseSort = false;
        }
    }
}