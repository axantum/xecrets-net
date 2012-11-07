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
        public static void TestSimple()
        {
            int workThreadId = -1;
            FileOperationStatus returnedStatus = FileOperationStatus.UnspecifiedError;

            bool done = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        workThreadId = Thread.CurrentThread.ManagedThreadId;
                        e.Result = FileOperationStatus.Success;
                    };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        returnedStatus = e.Result;
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
            int progressCalls = 0;

            ProgressContext progress = new ProgressContext();
            using (ThreadWorker worker = new ThreadWorker(progress))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                        e.Progress.AddCount(1);
                        e.Result = FileOperationStatus.Success;
                    };
                progress.Progressing += (object sender, ProgressEventArgs e) =>
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
            ThreadWorker worker = new ThreadWorker(new ProgressContext());
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

            bool hasCompleted = false;
            Assert.Throws<ObjectDisposedException>(() => { worker.Run(); });
            Assert.Throws<ObjectDisposedException>(() => { worker.Join(); });
            Assert.Throws<ObjectDisposedException>(() => { hasCompleted = worker.HasCompleted; });
            Assert.That(!hasCompleted, "Although the thread has completed, the variable should still be false since the attempt to set it is after Dispose().");
            Assert.DoesNotThrow(() => { worker.Dispose(); });
        }

        [Test]
        public static void TestCancellationByException()
        {
            bool wasCanceled = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        throw new OperationCanceledException();
                    };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        wasCanceled = e.Result == FileOperationStatus.Canceled;
                    };
                worker.Run();
                worker.Join();
            }

            Assert.That(wasCanceled, Is.True, "The operation was canceled and should return status as such.");
        }

        [Test]
        public static void TestCancellationByRequest()
        {
            bool wasCanceled = false;
            FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    e.Progress.Cancel = true;
                    environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                    e.Progress.AddCount(1);
                };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    wasCanceled = e.Result == FileOperationStatus.Canceled;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(wasCanceled, Is.True, "The operation was canceled and should return status as such.");
        }

        [Test]
        public static void TestPrepare()
        {
            bool wasPrepared = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Prepare += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        wasPrepared = true;
                    };
                worker.Run();
                worker.Join();
            }

            Assert.That(wasPrepared, Is.True, "The Prepare event should be raised.");
        }

        [Test]
        public static void TestErrorSetInWorkCompleted()
        {
            bool errorInWork = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    throw new InvalidOperationException();
                };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    errorInWork = e.Result == FileOperationStatus.Exception;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(errorInWork, Is.True, "The operation was interrupted by an exception and should return status as such.");
        }

        [Test]
        public static void TestHasCompleted()
        {
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                bool wasCompletedInWork = false;
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    wasCompletedInWork = worker.HasCompleted;
                };
                bool wasCompletedInCompleted = false;
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    wasCompletedInCompleted = worker.HasCompleted;
                };
                worker.Run();
                worker.Join();
                Assert.That(!wasCompletedInWork, "Completion is not set as true in the work event.");
                Assert.That(!wasCompletedInCompleted, "Completion is not set as true until after the completed event.");
                Assert.That(worker.HasCompleted, "Completion should be set as true when the thread is joined.");
            }
        }
    }
}