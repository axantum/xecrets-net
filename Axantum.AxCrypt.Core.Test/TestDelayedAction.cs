using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestDelayedAction
    {
        [Test]
        public static void TestNullArgument()
        {
            Action nullAction = null;
            DelayedAction delayedAction;
            Assert.Throws<ArgumentNullException>(() => { delayedAction = new DelayedAction(nullAction, new TimeSpan(0, 0, 1), null); }, "The 'action' argument is not allowed to be null");
        }

        [Test]
        public static void TestShortDelayButNoStart()
        {
            bool wasHere = false;
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1), null))
            {
                Thread.Sleep(50);
                Assert.That(wasHere, Is.False, "The event should not be triggered until started.");
            }
        }

        [Test]
        public static void TestShortDelayAndImmediateStart()
        {
            bool wasHere = false;
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1), null))
            {
                delayedAction.RestartIdleTimer();
                Thread.Sleep(50);
                Assert.That(wasHere, Is.True, "The event should be triggered once started.");
                wasHere = false;
                Thread.Sleep(50);
                Assert.That(wasHere, Is.False, "The event should not be triggered more than once.");
            }
        }

        [Test]
        public static void TestManyRestartsButOnlyOneEvent()
        {
            int eventCount = 0;
            using (DelayedAction delayedAction = new DelayedAction(() => { ++eventCount; }, new TimeSpan(0, 0, 0, 0, 5), null))
            {
                for (int i = 0; i < 10; ++i)
                {
                    delayedAction.RestartIdleTimer();
                }
                Thread.Sleep(50);
                Assert.That(eventCount, Is.EqualTo(1), "The event should be triggered exactly once.");
                Thread.Sleep(50);
                Assert.That(eventCount, Is.EqualTo(1), "The event should still be triggered exactly once.");
            }
        }

        [Test]
        public static void TestEventAfterDispose()
        {
            bool wasHere = false;
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1), null))
            {
                delayedAction.RestartIdleTimer();
                Assert.That(wasHere, Is.False, "The event should not be triggered immediately.");
            }
            Thread.Sleep(50);
            Assert.That(wasHere, Is.False, "The event should be note be triggered once disposed.");
        }

        [Test]
        public static void TestObjectDisposedException()
        {
            DelayedAction delayedAction = new DelayedAction(() => { }, new TimeSpan(0, 0, 0, 0, 1), null);
            delayedAction.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { delayedAction.RestartIdleTimer(); });
        }
    }
}