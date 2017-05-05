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
using Axantum.AxCrypt.Abstractions.Algorithm;
using Axantum.AxCrypt.Core;
using Axantum.AxCrypt.Core.Algorithm;
using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Mono.Portable;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestRuntimeEnvironment
    {
        private static string _workFolderPath;

        [SetUp]
        public static void Setup()
        {
            _workFolderPath = Path.Combine(Path.GetTempPath(), @"Axantum.AxCrypt.Mono.Test.TestRuntimeEnvironment\");
            Directory.CreateDirectory(_workFolderPath);

            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.Singleton<IReport>(() => new FakeReport());
            TypeMap.Register.New<string, IDataStore>((path) => new DataStore(path));
            TypeMap.Register.New<string, IDataContainer>((path) => new DataContainer(path));
            TypeMap.Register.Singleton<IRuntimeEnvironment>(() => new RuntimeEnvironment(".axx"));
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<WorkFolder>(() => new WorkFolder(_workFolderPath));
            TypeMap.Register.Singleton<ILogging>(() => new Logging());
            TypeMap.Register.Singleton<IRandomGenerator>(() => new RandomGenerator());
            TypeMap.Register.New<RandomNumberGenerator>(() => PortableFactory.RandomNumberGenerator());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
            Directory.Delete(_workFolderPath, true);
        }

        [Test]
        public static void TestAxCryptExtension()
        {
            Assert.That(OS.Current.AxCryptExtension, Is.EqualTo(".axx"), "Checking the standard AxCrypt extension.");
        }

        [Test]
        public static void TestIfIsLittleEndian()
        {
            Assert.That(OS.Current.IsLittleEndian, Is.EqualTo(BitConverter.IsLittleEndian), "Checking endianess.");
        }

        [Test]
        public static void TestRandomBytes()
        {
            byte[] randomBytes = Resolve.RandomGenerator.Generate(100);
            Assert.That(randomBytes.Length, Is.EqualTo(100), "Ensuring we really got the right number of bytes.");
            Assert.That(randomBytes, Is.Not.EquivalentTo(new byte[100]), "It is not in practice possible that all zero bytes are returned by GetRandomBytes().");

            randomBytes = Resolve.RandomGenerator.Generate(1000);
            double average = randomBytes.Average(b => b);
            Assert.That(average >= 115 && average <= 140, "Unscientific, but the sample sequence should not vary much from a mean of 127.5, but was {0}".InvariantFormat(average));
        }

        [Test]
        public static void TestRuntimeFileInfo()
        {
            IDataStore runtimeFileInfo = New<IDataStore>(Path.Combine(Path.GetTempPath(), "A File.txt"));
            Assert.That(runtimeFileInfo is DataStore, "The instance returned should be of type DataStore");
            Assert.That(runtimeFileInfo.Name, Is.EqualTo("A File.txt"));
            runtimeFileInfo = New<IDataStore>(Path.Combine(Path.GetTempPath(), "A File.txt"));
            Assert.That(runtimeFileInfo.Name, Is.EqualTo("A File.txt"));
        }

        [Test]
        public static void TestTemporaryDirectoryInfo()
        {
            IDataContainer tempInfo = New<WorkFolder>().FileInfo;
            Assert.That(tempInfo is DataContainer, "The instance returned should be of type DataStore");
            IDataStore tempFileInfo = New<IDataStore>(Path.Combine(tempInfo.FullName, "AxCryptTestTemp.tmp"));
            Assert.DoesNotThrow(() =>
            {
                try
                {
                    using (Stream stream = tempFileInfo.OpenWrite())
                    {
                    }
                }
                finally
                {
                    tempFileInfo.Delete();
                }
            }, "Write permissions should always be present in the temp directory.");
        }

        [Test]
        public static void TestUtcNow()
        {
            DateTime utcNow = DateTime.UtcNow;
            DateTime utcNowAgain = New<INow>().Utc;
            Assert.That(utcNowAgain - utcNow < new TimeSpan(0, 0, 1), "The difference should not be greater than one second, that's not reasonable.");
        }
    }
}