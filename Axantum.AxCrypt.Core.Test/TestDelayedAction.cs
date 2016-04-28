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

using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.Runtime;
using Axantum.AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDelayedAction
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<ISleep>(() => new FakeSleep());
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestShortDelayButNoStart()
        {
            bool wasHere = false;
            FakeSleep fakeSleep = new FakeSleep();
            using (DelayedAction delayedAction = new DelayedAction(new FakeDelayTimer(fakeSleep), new TimeSpan(0, 0, 0, 0, 1)))
            {
                delayedAction.Action += (sender, e) => wasHere = true;
                fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 50));
                Assert.That(wasHere, Is.False, "The event should not be triggered until started.");
            }
        }

        [Test]
        public static void TestShortDelayAndImmediateStart()
        {
            bool wasHere = false;
            FakeSleep fakeSleep = new FakeSleep();
            using (DelayedAction delayedAction = new DelayedAction(new FakeDelayTimer(fakeSleep), new TimeSpan(0, 0, 0, 0, 1)))
            {
                delayedAction.Action += (sender, e) => wasHere = true;
                delayedAction.StartIdleTimer();
                fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 50));
                Assert.That(wasHere, Is.True, "The event should be triggered once started.");
                wasHere = false;
                fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 50));
                Assert.That(wasHere, Is.False, "The event should not be triggered more than once.");
            }
        }

        [Test]
        public static void TestManyRestartsButOnlyOneEvent()
        {
            int eventCount = 0;
            FakeSleep fakeSleep = new FakeSleep();
            using (DelayedAction delayedAction = new DelayedAction(new FakeDelayTimer(fakeSleep), new TimeSpan(0, 0, 0, 0, 5)))
            {
                delayedAction.Action += (sender, e) => ++eventCount;
                for (int i = 0; i < 10; ++i)
                {
                    delayedAction.StartIdleTimer();
                }
                fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 5));
                Assert.That(eventCount, Is.EqualTo(1), "The event should be triggered exactly once.");
                fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 5));
                Assert.That(eventCount, Is.EqualTo(1), "The event should still be triggered exactly once.");
            }
        }

        [Test]
        public static void TestEventAfterDispose()
        {
            bool wasHere = false;
            FakeSleep fakeSleep = new FakeSleep();
            using (DelayedAction delayedAction = new DelayedAction(new FakeDelayTimer(fakeSleep), new TimeSpan(0, 0, 0, 0, 1)))
            {
                delayedAction.Action += (sender, e) => wasHere = true;
                delayedAction.StartIdleTimer();
                Assert.That(wasHere, Is.False, "The event should not be triggered immediately.");
            }
            fakeSleep.Time(new TimeSpan(0, 0, 0, 0, 1));
            Assert.That(wasHere, Is.False, "The event should be not be triggered once disposed.");
        }

        [Test]
        public static void TestObjectDisposedException()
        {
            FakeSleep fakeSleep = new FakeSleep();
            DelayedAction delayedAction = new DelayedAction(new FakeDelayTimer(fakeSleep), new TimeSpan(0, 0, 0, 0, 1));
            delayedAction.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { delayedAction.StartIdleTimer(); });
        }
    }
}