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
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptFile
    {
        private static IRuntimeEnvironment _environment;

        [TestFixtureSetUp]
        public static void InitFixture()
        {
            _environment = Environment.Current;
            Environment.Current = new FakeRuntimeEnvironment();

            FakeRuntimeFileInfo.AddFile(@"c:\test.txt", new DateTime(2012, 1, 2, 3, 4, 5, DateTimeKind.Utc), new MemoryStream(Encoding.UTF8.GetBytes("This is a short file")));
            FakeRuntimeFileInfo.AddFile(@"c:\test-txt.axx", new DateTime(2012, 2, 2, 3, 4, 5, 6, DateTimeKind.Utc), new MemoryStream());
        }

        [TestFixtureTearDownAttribute]
        public static void UnInitFixture()
        {
            Environment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestEncrypt()
        {
            AxCryptFile.Encrypt(Environment.Current.FileInfo(@"c:\test.txt"), Environment.Current.FileInfo(@"c:\test-txt.axx"), new Passphrase("axcrypt"));
        }
    }
}