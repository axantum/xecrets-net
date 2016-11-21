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
 * The source is maintained at http://bitbucket.org/axantum/axcrypt-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Core.UI;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestWorkerGroupProgressContext
    {
        private static Mock<IProgressContext> _progressMock;
        private static Mock<ISingleThread> _singleThreadMock;

        private static WorkerGroupProgressContext _progress;

        [SetUp]
        public static void Setup()
        {
            _progressMock = new Mock<IProgressContext>();
            _singleThreadMock = new Mock<ISingleThread>();

            _progress = new WorkerGroupProgressContext(_progressMock.Object, _singleThreadMock.Object);
        }

        [TearDown]
        public static void Teardown()
        {
        }

        [Test]
        public static void TestAddCount()
        {
            _progress.AddCount(456);

            _progressMock.Verify(p => p.AddCount(It.Is<long>(l => l == 456)), Times.Once);
        }

        [Test]
        public static void TestAddTotal()
        {
            _progress.AddTotal(123);

            _progressMock.Verify(p => p.AddTotal(It.Is<long>(l => l == 123)), Times.Once);
        }

        [Test]
        public static void TestAllItemsConfirmed()
        {
            bool allConfirmed = _progress.AllItemsConfirmed;
            _progress.AllItemsConfirmed = true;

            Assert.That(allConfirmed, Is.False);
            _progressMock.VerifyGet(p => p.AllItemsConfirmed, Times.Once);
            _progressMock.VerifySet(p => p.AllItemsConfirmed = true, Times.Once);
        }

        [Test]
        public static void TestCancel()
        {
            bool cancel = _progress.Cancel;
            _progress.Cancel = true;

            Assert.That(cancel, Is.False);
            _progressMock.VerifyGet(p => p.Cancel, Times.Once);
            _progressMock.VerifySet(p => p.Cancel = true, Times.Once);
        }

        [Test]
        public async static Task TestEnterSingleThread()
        {
            await _progress.EnterSingleThread();
            _singleThreadMock.Verify(s => s.Enter(), Times.Once);
        }

        [Test]
        public static void TestLeaveSingleThread()
        {
            _progress.LeaveSingleThread();
            _singleThreadMock.Verify(s => s.Leave(), Times.Once);
        }

        [Test]
        public static void TestNotifyLevelFinished()
        {
            _progress.NotifyLevelFinished();
            _progressMock.Verify(p => p.NotifyLevelFinished(), Times.Once);
        }

        [Test]
        public static void TestNotifyLevelStart()
        {
            _progress.NotifyLevelStart();
            _progressMock.Verify(p => p.NotifyLevelStart(), Times.Once);
        }

        [Test]
        public static void TestProgressing()
        {
            int percent = 0;
            EventHandler<ProgressEventArgs> handler = (sender, e) =>
            {
                percent = e.Percent;
            };
            _progress.Progressing += handler;

            _progressMock.Raise(p => p.Progressing += null, new ProgressEventArgs(13));

            Assert.That(percent, Is.EqualTo(13));

            _progress.Progressing -= handler;
            _progressMock.Raise(p => p.Progressing += null, new ProgressEventArgs(26));

            Assert.That(percent, Is.EqualTo(13));
        }

        [Test]
        public static void TestRemoveCount()
        {
            _progress.RemoveCount(999, 777);

            _progressMock.Verify(p => p.RemoveCount(It.Is<long>(l => l == 999), It.Is<long>(l => l == 777)), Times.Once);
        }
    }
}