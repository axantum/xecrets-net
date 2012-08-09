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
using System.IO;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestFileOperation
    {
        private static IRuntimeEnvironment _environment;

        private static FakeRuntimeEnvironment _fakeRuntimeEnvironment;

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", FakeRuntimeFileInfo.TestDate1Utc, FakeRuntimeFileInfo.TestDate2Utc, FakeRuntimeFileInfo.TestDate1Utc, new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
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
            FileSystemState fileSystemState = FileSystemState.Load(AxCryptEnvironment.Current.FileInfo(@"c:\Temp\FileSystemState.xml"));
            FileSystemState nullFileSystemState = null;

            string file = @"c:\Documents\HelloWorld.axx";
            string nullFile = null;

            IEnumerable<AesKey> keys = new AesKey[] { new Passphrase("a").DerivedPassphrase };
            IEnumerable<AesKey> nullKeys = null;

            ProgressContext context = new ProgressContext();
            ProgressContext nullContext = null;

            Assert.Throws<ArgumentNullException>(() => { nullFileSystemState.OpenAndLaunchApplication(file, keys, context); }, "The FileSystemState is null.");
            Assert.Throws<ArgumentNullException>(() => { fileSystemState.OpenAndLaunchApplication(nullFile, keys, context); }, "The file string is null.");
            Assert.Throws<ArgumentNullException>(() => { fileSystemState.OpenAndLaunchApplication(file, nullKeys, context); }, "The keys are null.");
            Assert.Throws<ArgumentNullException>(() => { fileSystemState.OpenAndLaunchApplication(file, keys, nullContext); }, "The context is null.");
        }
    }
}