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
            FileOperationStatus returnedStatus = FileOperationStatus.UnspecifiedError;

            bool done = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        workThreadId = Thread.CurrentThread.ManagedThreadId;
                        e.Result = FileOperationStatus.Success;
                    };
                worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
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
                        environment.CurrentTiming.CurrentTiming = e.ProgressContext.NextProgressing;
                        e.ProgressContext.AddCount(1);
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

            Assert.Throws<ObjectDisposedException>(() => { worker.Run(); });
            Assert.Throws<ObjectDisposedException>(() => { worker.Join(); });
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
                worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
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
                    e.ProgressContext.Cancel = true;
                    environment.CurrentTiming.CurrentTiming = e.ProgressContext.NextProgressing;
                    e.ProgressContext.AddCount(1);
                };
                worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
                {
                    wasCanceled = e.Result == FileOperationStatus.Canceled;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(wasCanceled, Is.True, "The operation was canceled and should return status as such.");
        }

        [Test]
        public static void TestExceptionInWork()
        {
            bool exceptionCaught = false;
            using (ThreadWorker worker = new ThreadWorker(new ProgressContext()))
            {
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    throw new InvalidOperationException();
                };
                worker.Completed += (object sender, ThreadWorkerEventArgs e) =>
                {
                    exceptionCaught = e.Result == FileOperationStatus.Exception;
                };
                worker.Run();
                worker.Join();
            }

            Assert.That(exceptionCaught, Is.True, "The operation was interrupted by an exception and should return status as such.");
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
    }
}