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
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWorkerGroup
    {
        [Test]
        public static void TestCoreFunctionality()
        {
            int threadCount = 0;
            int maxCount = 0;
            using (WorkerGroup workerGroup = new WorkerGroup())
            {
                object threadLock = new object();
                ThreadWorker worker1 = workerGroup.CreateWorker();
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
                worker1.Completed += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        --threadCount;
                    };
                worker1.Run();

                ThreadWorker worker2 = workerGroup.CreateWorker();
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
                worker2.Completed += (object sender, ThreadWorkerEventArgs e) =>
                    {
                        --threadCount;
                    };
                worker2.Run();

                workerGroup.NotifyFinished();
                Assert.That(maxCount, Is.EqualTo(1), "There should never be more than one thread active at one time.");
            }
        }
    }
}