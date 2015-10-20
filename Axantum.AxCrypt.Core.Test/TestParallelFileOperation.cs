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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestParallelFileOperation
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
        public static void TestParallelFileOperationSimple()
        {
            IDataStore info1 = New<IDataStore>(@"c:\file1.txt");
            IDataStore info2 = New<IDataStore>(@"c:\file2.txt");
            ParallelFileOperation pfo = new ParallelFileOperation();
            int callCount = 0;
            pfo.DoFiles(new IDataStore[] { info1, info2 },
                (info, progress) =>
                {
                    ++callCount;
                    return new FileOperationContext(String.Empty, ErrorStatus.Success);
                },
                (status) =>
                {
                });

            Assert.That(callCount, Is.EqualTo(2), "There are two files, so there should be two calls.");
        }

        [Test]
        public static void TestQuitAllOnError()
        {
            FakeUIThread fakeUIThread = new FakeUIThread();
            fakeUIThread.IsOnUIThread = true;
            TypeMap.Register.Singleton<IUIThread>(() => fakeUIThread);

            FakeRuntimeEnvironment.Instance.MaxConcurrency = 2;

            IDataStore info1 = New<IDataStore>(@"c:\file1.txt");
            IDataStore info2 = New<IDataStore>(@"c:\file2.txt");
            IDataStore info3 = New<IDataStore>(@"c:\file3.txt");
            IDataStore info4 = New<IDataStore>(@"c:\file4.txt");
            ParallelFileOperation pfo = new ParallelFileOperation();

            int callCount = 0;
            pfo.DoFiles(new IDataStore[] { info1, info2, info3, info4 },
                (info, progress) =>
                {
                    int result = Interlocked.Increment(ref callCount);
                    if (result == 1)
                    {
                        return new FileOperationContext(String.Empty, ErrorStatus.UnspecifiedError);
                    }
                    Thread.Sleep(1);
                    return new FileOperationContext(String.Empty, ErrorStatus.Success);
                },
                (status) => { });
            Assert.That(callCount, Is.LessThanOrEqualTo(2), "There are several files, but max concurrency is two, so there could be up to two calls.");
        }
    }
}