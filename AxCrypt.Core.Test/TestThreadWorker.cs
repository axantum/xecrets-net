#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Runtime;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestThreadWorker
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestSimple), new ProgressContext(), false))
            {
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                    {
                        workThreadId = Thread.CurrentThread.ManagedThreadId;
                        e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
                        return Task.FromResult<object>(null);
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestProgress), progress, false))
            {
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                    {
                        environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                        e.Progress.AddCount(1);
                        e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
                        return Task.FromResult<object>(null);
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
            IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestObjectDisposedException), new ProgressContext(), false);
            worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                {
                    e.Result = new FileOperationContext(String.Empty, ErrorStatus.Success);
                    return Task.FromResult<object>(null);
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestCancellationByException), new ProgressContext(), false))
            {
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestCancellationByRequest), new CancelProgressContext(new ProgressContext()), false))
            {
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                {
                    e.Progress.Cancel = true;
                    environment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
                    e.Progress.AddCount(1);
                    return Task.FromResult<object>(null);
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestPrepare), new ProgressContext(), false))
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestErrorSetInWorkCompleted), new ProgressContext(), false))
            {
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestHasCompleted), new ProgressContext(), false))
            {
                bool wasCompletedInWork = false;
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                {
                    wasCompletedInWork = worker.HasCompleted;
                    return Task.FromResult<object>(null);
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
            using (IThreadWorker worker = Resolve.Portable.ThreadWorker(nameof(TestExceptionDuringWork), new ProgressContext(), false))
            {
                worker.WorkAsync = (e) =>
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
