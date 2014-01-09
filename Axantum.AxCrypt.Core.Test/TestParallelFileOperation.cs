#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

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
        public static void TestRunningOnUIThread()
        {
            FakeUIThread fakeUIThread = new FakeUIThread();
            fakeUIThread.IsOnUIThread = true;
            bool runOnUIThread = false;
            fakeUIThread.RunOnUIThreadAction = (action) => runOnUIThread = true;

            IRuntimeFileInfo info1 = Factory.New<IRuntimeFileInfo>(@"c:\file1.txt");
            IRuntimeFileInfo info2 = Factory.New<IRuntimeFileInfo>(@"c:\file2.txt");
            ParallelFileOperation pfo = new ParallelFileOperation(fakeUIThread);
            int callCount = 0;
            pfo.DoFiles(new IRuntimeFileInfo[] { info1, info2 },
                (info, progress) =>
                {
                    ++callCount;
                    return FileOperationStatus.Success;
                },
                (status) =>
                {
                });

            Assert.That(runOnUIThread, Is.False, "Since the operation is performed on the UI Thread there should be no need to further marshall to the UI Thread.");
            Assert.That(callCount, Is.EqualTo(2), "There are two files, so there should be two calls.");
        }

        [Test]
        public static void TestNotRunningOnUIThread()
        {
            FakeUIThread fakeUIThread = new FakeUIThread();
            fakeUIThread.IsOnUIThread = false;
            bool runOnUIThread = false;
            fakeUIThread.RunOnUIThreadAction = (action) => { runOnUIThread = true; action(); };
            Factory.Instance.Singleton<IUIThread>(() => fakeUIThread);

            IRuntimeFileInfo info1 = Factory.New<IRuntimeFileInfo>(@"c:\file1.txt");
            IRuntimeFileInfo info2 = Factory.New<IRuntimeFileInfo>(@"c:\file2.txt");
            ParallelFileOperation pfo = new ParallelFileOperation(fakeUIThread);
            int callCount = 0;
            pfo.DoFiles(new IRuntimeFileInfo[] { info1, info2 },
                (info, progress) =>
                {
                    progress.SerializeOnUIThread(() =>
                    {
                        ++callCount;
                    });
                    return FileOperationStatus.Success;
                },
                (status) =>
                {
                });

            Assert.That(runOnUIThread, Is.True, "Since the operation is not performed on the UI Thread there should be a need to further marshall to the UI Thread.");
            Assert.That(callCount, Is.EqualTo(2), "There are two files, so there should be two calls.");
        }

        [Test]
        public static void TestQuitAllOnError()
        {
            FakeUIThread fakeUIThread = new FakeUIThread();
            fakeUIThread.IsOnUIThread = true;
            Factory.Instance.Singleton<IUIThread>(() => fakeUIThread);

            FakeRuntimeEnvironment.Instance.MaxConcurrency = 2;

            IRuntimeFileInfo info1 = Factory.New<IRuntimeFileInfo>(@"c:\file1.txt");
            IRuntimeFileInfo info2 = Factory.New<IRuntimeFileInfo>(@"c:\file2.txt");
            IRuntimeFileInfo info3 = Factory.New<IRuntimeFileInfo>(@"c:\file3.txt");
            IRuntimeFileInfo info4 = Factory.New<IRuntimeFileInfo>(@"c:\file4.txt");
            ParallelFileOperation pfo = new ParallelFileOperation(fakeUIThread);

            int callCount = 0;
            pfo.DoFiles(new IRuntimeFileInfo[] { info1, info2, info3, info4 },
                (info, progress) =>
                {
                    int result = Interlocked.Increment(ref callCount);
                    if (result == 1)
                    {
                        return FileOperationStatus.UnspecifiedError;
                    }
                    Thread.Sleep(1);
                    return FileOperationStatus.Success;
                },
                (status) => { });
            Assert.That(callCount, Is.LessThanOrEqualTo(2), "There are several files, but max concurrency is two, so there could be up to two calls.");
        }
    }
}