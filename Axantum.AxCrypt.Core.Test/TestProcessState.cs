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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.Session;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestProcessState
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
        public static void TestDoubleDispose()
        {
            ProcessState ps = new ProcessState();
            Assert.DoesNotThrow(() => ps.Dispose());
            Assert.DoesNotThrow(() => ps.Dispose());
        }

        [Test]
        public static void TestPurgeInactive()
        {
            TypeMap.Register.New<ILauncher>(() => new FakeLauncher());

            ProcessState ps = new ProcessState();

            ActiveFile activeFile1 = new ActiveFile(New<IDataStore>(@"C:\encrypted.axx"), New<IDataStore>(@"C:\decrypted.txt"), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            ILauncher launcher1 = New<ILauncher>();
            launcher1.Launch(activeFile1.EncryptedFileInfo.FullName);
            ps.Add(launcher1, activeFile1);

            Assert.That(ps.HasActiveProcess(activeFile1), Is.True);

            FakeLauncher fakeLauncher1 = (FakeLauncher)launcher1;

            fakeLauncher1.HasExited = true;
            Assert.That(ps.HasActiveProcess(activeFile1), Is.False);

            fakeLauncher1.HasExited = false;
            Assert.That(ps.HasActiveProcess(activeFile1), Is.True);

            fakeLauncher1.HasExited = true;
            Assert.That(ps.HasActiveProcess(activeFile1), Is.False);

            ActiveFile activeFile2 = new ActiveFile(New<IDataStore>(@"C:\encrypted2.axx"), New<IDataStore>(@"C:\decrypted2.txt"), new LogOnIdentity("passphrase"), ActiveFileStatus.NotDecrypted, new V1Aes128CryptoFactory().CryptoId);
            ILauncher launcher2 = New<ILauncher>();
            launcher2.Launch(activeFile2.EncryptedFileInfo.FullName);
            ps.Add(launcher2, activeFile2);

            Assert.That(ps.HasActiveProcess(activeFile1), Is.False);

            fakeLauncher1.HasExited = false;
            Assert.That(ps.HasActiveProcess(activeFile1), Is.False);
        }
    }
}