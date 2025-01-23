﻿#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWorkerGroup
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(Fake.CryptoImplementation.WindowsDesktop);
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
                IThreadWorker worker1 = workerGroup.CreateWorker(nameof(TestCoreFunctionality) + "1");
                worker1.WorkAsync = (ThreadWorkerEventArgs e) =>
                    {
                        lock (threadLock)
                        {
                            ++threadCount;
                            if (threadCount > maxCount)
                            {
                                maxCount = threadCount;
                            }
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
                        return Task.FromResult<object>(null);
                    };
                worker1.Completing += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        --threadCount;
                    };
                worker1.Run();

                IThreadWorker worker2 = workerGroup.CreateWorker(nameof(TestCoreFunctionality) + "2");
                worker2.WorkAsync = (ThreadWorkerEventArgs e) =>
                    {
                        lock (threadLock)
                        {
                            ++threadCount;
                            if (threadCount > maxCount)
                            {
                                maxCount = threadCount;
                            }
                        }
                        Thread.Sleep(new TimeSpan(0, 0, 0, 0, 100));
                        return Task.FromResult<object>(null);
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
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestInvalidOperationException));

                bool? f = null;
                Assert.Throws<InvalidOperationException>(() => { f = worker.HasCompleted; });
                Assert.That(!f.HasValue, "No value should be set, since an exception should have occurred.");
                Assert.Throws<InvalidOperationException>(() => { worker.Join(); });
                worker.Abort();
            }
        }

        [Test]
        public static void TestAddingSubscribersToWorkerThread()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestAddingSubscribersToWorkerThread));
                bool wasPrepared = false;
                worker.Prepare += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        wasPrepared = true;
                    };
                bool didWork = false;
                worker.WorkAsync = (ThreadWorkerEventArgs e) =>
                {
                    didWork = true;
                    return Task.FromResult<object>(null);
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
            e.Result = new FileOperationContext(String.Empty, ErrorStatus.UnspecifiedError);
        }

        private static Task ThreadWorkerCommandHandler(ThreadWorkerEventArgs e)
        {
            e.Result = new FileOperationContext(String.Empty, ErrorStatus.UnspecifiedError);
            return Task.FromResult<object>(null);
        }

        [Test]
        public static void TestRemovingSubscribersFromWorkerThread()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestRemovingSubscribersFromWorkerThread) + "1");
                worker.Prepare += ThreadWorkerEventHandler;
                worker.WorkAsync = ThreadWorkerCommandHandler;
                worker.Completing += ThreadWorkerEventHandler;

                worker.Run();
                workerGroup.WaitAllAndFinish();

                Assert.That(workerGroup.FirstError.ErrorStatus, Is.EqualTo(ErrorStatus.UnspecifiedError), "The status should be set by one of the event handlers.");
            }

            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestRemovingSubscribersFromWorkerThread) + "2");
                worker.Prepare += ThreadWorkerEventHandler;
                worker.WorkAsync = ThreadWorkerCommandHandler;
                worker.Completing += ThreadWorkerEventHandler;

                worker.Prepare -= ThreadWorkerEventHandler;
                worker.WorkAsync = null;
                worker.Completing -= ThreadWorkerEventHandler;

                worker.Run();
                workerGroup.WaitAllAndFinish();

                Assert.That(workerGroup.FirstError.ErrorStatus, Is.EqualTo(ErrorStatus.Success), "None of the event handlers should have been called, so the status should not have been set there.");
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestDoubleDispose()
        {
            Assert.DoesNotThrow(() =>
                {
                    using (WorkerGroup workerGroup = new WorkerGroup())
                    {
                        IThreadWorker worker = workerGroup.CreateWorker(nameof(TestDoubleDispose));
                        worker.Run();
                        workerGroup.Dispose();
                    }
                });
        }

        [Test]
        public static void TestObjectDisposed()
        {
            WorkerGroup workerGroup = new WorkerGroup();
            IThreadWorker worker = workerGroup.CreateWorker(nameof(TestObjectDisposed));
            worker.Run();
            workerGroup.Dispose();

            worker = null;
            Assert.Throws<ObjectDisposedException>(() => { worker = workerGroup.CreateWorker(nameof(TestObjectDisposed)); }, "A call to a method on a disposed object should raise ObjectDisposedException.");
            Assert.That(worker, Is.Null, "The worker should still be null, since the previous attempt to create should fail with an exception.");
            Assert.Throws<ObjectDisposedException>(() => { workerGroup.WaitAllAndFinish(); }, "A call to a method on a disposed object should raise ObjectDisposedException.");
        }

        [Test]
        public static void TestDoubleFinished()
        {
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestDoubleFinished));
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
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestExplicitConstructor));
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
                IThreadWorker worker = workerGroup.CreateWorker(nameof(TestProgressing));
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

            using (IThreadWorker threadWorker = Resolve.Portable.ThreadWorker(nameof(TestFinishInBackground) + "Outer", progress, false))
            {
                threadWorker.WorkAsync = (ThreadWorkerEventArgs e) =>
                {
                    using (WorkerGroup workerGroup = new WorkerGroup(progress))
                    {
                        IThreadWorker worker = workerGroup.CreateWorker(nameof(TestFinishInBackground) + "Inner");
                        worker.WorkAsync = (ThreadWorkerEventArgs e2) =>
                        {
                            e2.Progress.NotifyLevelStart();
                            e2.Progress.NotifyLevelFinished();
                            return Task.FromResult<object>(null);
                        };
                        worker.Run();
                    }
                    return Task.FromResult<object>(null);
                };
                threadWorker.Run();
                threadWorker.Join();
            }

            Assert.That(didComplete, "Execution should continue here, with the flag set indicating that the progress event occurred.");
        }
    }
}
