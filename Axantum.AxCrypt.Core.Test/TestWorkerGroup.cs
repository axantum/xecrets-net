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
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWorkerGroup
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
        public static void TestCoreFunctionality()
        {
            int threadCount = 0;
            int maxCount = 0;
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                object threadLock = new object();
                IThreadWorker worker1 = workerGroup.CreateWorker();
                worker1.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        lock (threadLock)
                        {
                            ++threadCount;
                            if (threadCount > maxCount)
                            {
                                maxCount = threadCount;
                            }
                        }
                        Thread.Sleep(100);
                    };
                worker1.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        --threadCount;
                    };
                worker1.Run();

                IThreadWorker worker2 = workerGroup.CreateWorker();
                worker2.Work += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        lock (threadLock)
                        {
                            ++threadCount;
                            if (threadCount > maxCount)
                            {
                                maxCount = threadCount;
                            }
                        }
                        Thread.Sleep(100);
                    };
                worker2.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        --threadCount;
                    };
                worker2.Run();

                workerGroup.WaitAllAndFinish();
                Assert.That(maxCount, Is.EqualTo(1), "There should never be more than one thread active at one time.");
            }
        }

        [Test]
        public static void TestInvalidOperationException()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker();

                bool? f = null;
                Assert.Throws<InvalidOperationException>(() => { f = worker.HasCompleted; });
                Assert.That(!f.HasValue, "No value should be set, since an exception should have occurred.");
                Assert.Throws<InvalidOperationException>(() => { worker.Join(); });
				worker.Abort();
            }
        }

        [Test]
		public static void TestAddingSubscribersToWorkerThread ()
		{
			using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker();
                bool wasPrepared = false;
                worker.Prepare += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        wasPrepared = true;
                    };
                bool didWork = false;
                worker.Work += (object sender, ThreadWorkerEventArgs e) =>
                {
                    didWork = true;
                };
                bool didComplete = false;
                worker.Completing += (object sender, ThreadWorkerEventArgs e) =>
                {
                    didComplete = true;
                };
                worker.Run();
                workerGroup.WaitAllAndFinish();
                Assert.That(wasPrepared, "The Prepare event should be raised.");
                Assert.That(didWork, "The Work event should be raised.");
                Assert.That(didComplete, "The Completed event should be raised.");
            }
        }

        private static void ThreadWorkerEventHandler(object sender, ThreadWorkerEventArgs e)
        {
            e.Result = FileOperationStatus.UnspecifiedError;
        }

        [Test]
        public static void TestRemovingSubscribersFromWorkerThread()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker();
                worker.Prepare += ThreadWorkerEventHandler;
                worker.Work += ThreadWorkerEventHandler;
                worker.Completing += ThreadWorkerEventHandler;

                worker.Run();
                workerGroup.WaitAllAndFinish();

                Assert.That(workerGroup.FirstError, Is.EqualTo(FileOperationStatus.UnspecifiedError), "The status should be set by one of the event handlers.");
            }

            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker();
                worker.Prepare += ThreadWorkerEventHandler;
                worker.Work += ThreadWorkerEventHandler;
                worker.Completing += ThreadWorkerEventHandler;

                worker.Prepare -= ThreadWorkerEventHandler;
                worker.Work -= ThreadWorkerEventHandler;
                worker.Completing -= ThreadWorkerEventHandler;

                worker.Run();
                workerGroup.WaitAllAndFinish();

                Assert.That(workerGroup.FirstError, Is.EqualTo(FileOperationStatus.Success), "None of the event handlers should have been called, so the status should not have been set there.");
            }
        }

        [Test]
        public static void TestDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
                {
                    using (WorkerGroup workerGroup = new WorkerGroup())
                    {
                        IThreadWorker worker = workerGroup.CreateWorker();
                        worker.Run();
                        workerGroup.Dispose();
                    }
                });
        }

        [Test]
        public static void TestObjectDisposed()
        {
			WorkerGroup workerGroup = new WorkerGroup();
            IThreadWorker worker = workerGroup.CreateWorker();
            worker.Run();
            workerGroup.Dispose();

            worker = null;
            Assert.Throws<ObjectDisposedException>(() => { worker = workerGroup.CreateWorker(); }, "A call to a method on a disposed object should raise ObjectDisposedException.");
            Assert.That(worker, Is.Null, "The worker should still be null, since the previous attempt to create should fail with an exception.");
            Assert.Throws<ObjectDisposedException>(() => { workerGroup.WaitAllAndFinish(); }, "A call to a method on a disposed object should raise ObjectDisposedException.");
        }

        [Test]
        public static void TestDoubleFinished()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker();
                worker.Run();
                workerGroup.WaitAllAndFinish();
                Assert.Throws<InvalidOperationException>(() => { workerGroup.WaitAllAndFinish(); });
            }
        }

        [Test]
        public static void TestExplicitConstructor()
        {
            ProgressContext progress = new ProgressContext();
            int percent = 0;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
                {
                    percent = e.Percent;
                };
            using (WorkerGroup workerGroup = new WorkerGroup(1, progress))
            {
                IThreadWorker worker = workerGroup.CreateWorker();
                worker.Run();
                workerGroup.WaitAllAndFinish();
            }
            Assert.That(percent, Is.EqualTo(100), "Progress at 100 percent should always be reported when the thread completes.");
        }

        [Test]
        public static void TestProgressing()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                int percent = 0;
                workerGroup.Progress.Progressing += (object sender, ProgressEventArgs e) =>
                    {
                        percent = e.Percent;
                    };
                IThreadWorker worker = workerGroup.CreateWorker();
                worker.Run();
                workerGroup.WaitAllAndFinish();
                Assert.That(percent, Is.EqualTo(100), "Progress at 100 percent should always be reported when the thread completes.");
            }
        }

        [Test]
        public static void TestFinishInBackground()
        {
            bool didComplete = false;
            ProgressContext progress = new ProgressContext();
			progress.Progressing += (object sender2, ProgressEventArgs e2) =>
			{
				didComplete = true;
			};
			
			using (ThreadWorker threadWorker = new ThreadWorker(progress))
			{
				threadWorker.Work += (object sender, ThreadWorkerEventArgs e) =>
	            {
					using (WorkerGroup workerGroup = new WorkerGroup(progress))
					{
		                IThreadWorker worker = workerGroup.CreateWorker();
		                worker.Work += (object sender2, ThreadWorkerEventArgs e2) =>
		                {
		                    e2.Progress.NotifyLevelStart();
							e2.Progress.NotifyLevelFinished();
						};
		                worker.Run();
					}
				};
	            threadWorker.Run();
			}
			
            Assert.That(didComplete, "Execution should continue here, with the flag set indicating that the progress event occurred.");
        }
    }
}