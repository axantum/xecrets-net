#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.Test.Properties;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFile
    {
        private static IRuntimeEnvironment _environment;

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate3Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\Users\AxCrypt\David Copperfield.txt", FakeRuntimeFileInfo.TestDate4Utc, FakeRuntimeFileInfo.TestDate5Utc, FakeRuntimeFileInfo.TestDate6Utc, new MemoryStream(Resources.David_Copperfield));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\Uncompressed.axx", new MemoryStream(Resources.Uncompressable_zip));
            FakeRuntimeFileInfo.AddFile(@"c:\Documents\HelloWorld.axx", new MemoryStream(Resources.HelloWorld_Key_a_txt));
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestInvalidArguments()
        {
            string nullString = null;
            Process nullProcess = null;
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(nullString, @"c:\dir\file.exe", new AesKey(), ActiveFileStatus.None, nullProcess); });
            Assert.Throws<ArgumentNullException>(() => { new ActiveFile(@"c:\dir\file.exe", nullString, new AesKey(), ActiveFileStatus.None, nullProcess); });
        }

        [Test]
        public static void TestConstructor()
        {
            AesKey key = new AesKey();
            Process process = new Process();
            ActiveFile activeFile = new ActiveFile(@"c:\Documents\HelloWorld.axx", @"c:\test.txt", key, ActiveFileStatus.None, process);

            IRuntimeFileInfo decryptedFileInfo = activeFile.DecryptedFileInfo;
            Assert.That(decryptedFileInfo.Exists, Is.True, "The file should exist in the fake file system.");
            Assert.That(decryptedFileInfo.FullName, Is.EqualTo(@"c:\test.txt"), "The file should be named as it was in the constructor");
        }
    }
}