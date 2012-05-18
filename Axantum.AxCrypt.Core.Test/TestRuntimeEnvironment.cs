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
using Axantum.AxCrypt.Core.IO;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestRuntimeEnvironment
    {
        [Test]
        public static void TestRuntimeEnvironmentMethods()
        {
            Assert.That(AxCryptEnvironment.Current.AxCryptExtension, Is.EqualTo(".axx"), "Checking the standard AxCrypt extension.");
            Assert.That(AxCryptEnvironment.Current.IsLittleEndian, Is.EqualTo(BitConverter.IsLittleEndian), "Checking endianess.");
            byte[] randomBytes = AxCryptEnvironment.Current.GetRandomBytes(100);
            Assert.That(randomBytes.Length, Is.EqualTo(100), "Ensuring we really got the right number of bytes.");
            Assert.That(randomBytes, Is.Not.EquivalentTo(new byte[100]), "It is not in practice possible that all zero bytes are returned by GetRandomBytes().");
            IRuntimeFileInfo runtimeFileInfo = AxCryptEnvironment.Current.FileInfo(new FileInfo(@"C:\Temp\A File.txt"));
            Assert.That(runtimeFileInfo.Name, Is.EqualTo("A File.txt"));
            runtimeFileInfo = AxCryptEnvironment.Current.FileInfo(@"C:\Temp\A File.txt");
            Assert.That(runtimeFileInfo.Name, Is.EqualTo("A File.txt"));
        }
    }
}