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
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

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
            FileOperationContext returnedStatus = new FileOperationContext(String.Empty, ErrorStatus.UnspecifiedError);

            bool done = false;
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        workThreadId = Thread.CurrentThread.ManagedThreadId;
                        e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
                    };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        returnedStatus = e.Result;
                        done = true;
                    };
                worker.Run();
                worker.Join();
            }

            Assert.That(returnedStatus.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "The status should be returned as successful.");
            Assert.That(workThreadId, Is.Not.EqualTo(Thread.CurrentThread.ManagedThreadId), "The work should not be performed on the caller thread.");
            Assert.That(done, Is.True, "The background work must have executed the completed handler now.");
        }

        [Test]
        public static void TestProgress()
        {
            FakeRuntimeEnvironment environment = (FakeRuntimeEnvironment)OS.Current;
            int progressCalls = 0;

            ProgressContext progress = new ProgressContext();
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(progress, false))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                        e.Progress.AddCount(1);
                        e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
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
            IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false);
            worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
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
            Assert.DoesNotThrow(() => { worker.Join(); });
            Assert.DoesNotThrow(() => { hasCompleted = worker.HasCompleted; });
            Assert.That(hasCompleted, "Even though the thread has completed, the variable should be set, since we allow calls to HasCompleted even after Dispose().");
            Assert.DoesNotThrow(() => { worker.Dispose(); });
        }

        [Test]
        public static void TestCancellationByException()
        {
            bool wasCanceled = false;
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        throw new OperationCanceledException();
                    };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        wasCanceled = e.Result.ErrorStatus == ErrorStatus.Canceled;
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new CancelProgressContext(new ProgressContext()), false))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    e.Progress.Cancel = true;
                    environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                    e.Progress.AddCount(1);
                };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    wasCanceled = e.Result.ErrorStatus == ErrorStatus.Canceled;
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    throw new InvalidOperationException();
                };
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    errorInWork = e.Result.ErrorStatus == ErrorStatus.Exception;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(errorInWork, Is.True, "The operation was interrupted by an exception and should return status as such.");
        }

        [Test]
        public static void TestHasCompleted()
        {
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
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

        [Test]
        public static void TestExceptionDuringWork()
        {
            FileOperationContext status = new FileOperationContext(String.Empty, ErrorStatus.Unknown);
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(new ProgressContext(), false))
            {
                worker.Work += (sender, e) =>
                {
                    throw new InvalidOperationException("Something went intentionally wrong.");
                };
                worker.Completed += (sender, e) =>
                {
                    status = e.Result;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(status.ErrorStatus, Is.EqualTo(ErrorStatus.Exception));
        }
    }
}