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
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestThreadWorker
    {
        [Test]
        public static void TestSimple()
        {
            int workThreadId = -1;
            int completeThreadId = -1;
            FileOperationStatus returnedStatus = FileOperationStatus.UnspecifiedError;

            bool done = false;
            using (ThreadWorker worker = new ThreadWorker("DisplayTest"))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        workThreadId = Thread.CurrentThread.ManagedThreadId;
                        e.Result = FileOperationStatus.Success;
                    };
                worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        returnedStatus = e.Result;
                        completeThreadId = Thread.CurrentThread.ManagedThreadId;
                        done = true;
                    };
                worker.Run();
                worker.Join();
            }

            Assert.That(returnedStatus, Is.EqualTo(FileOperationStatus.Success), "The status should be returned as successful.");
            Assert.That(workThreadId, Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId), "The work should not be performed on the caller thread.");
            Assert.That(done, Is.True, "The background work must have executed the completed handler now.");
        }

        [Test]
        public static void TestProgress()
        {
            FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
            environment.CurrentTiming.CurrentTiming = TimeSpan.Zero;
            int progressCalls = 0;

            using (ThreadWorker worker = new ThreadWorker("DisplayTest"))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        environment.CurrentTiming.CurrentTiming = e.ProgressContext.NextProgressing;
                        e.ProgressContext.Current = 1;
                        e.Result = FileOperationStatus.Success;
                    };
                worker.Progress += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        ++progressCalls;
                    };
                worker.Run();
                worker.Join();
            }

            Assert.That(progressCalls, Is.EqualTo(1), "The Progressing event should be raised exactly one time.");
        }

        [Test]
        public static void TestObjectDisposedException()
        {
            ThreadWorker worker = new ThreadWorker("DisposedTest");
            worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    e.Result = FileOperationStatus.Success;
                };
            try
            {
                worker.Run();
                worker.Join();
            }
            finally
            {
                worker.Dispose();
            }

            Assert.Throws<ObjectDisposedException>(() => { worker.Run(); });
            Assert.Throws<ObjectDisposedException>(() => { worker.Join(); });
            Assert.DoesNotThrow(() => { worker.Dispose(); });
        }
    }
}