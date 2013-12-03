#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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
            DelayedAction delayedAction = null;
            Assert.Throws<ArgumentNullException>(() => { delayedAction = new DelayedAction(nullAction, new TimeSpan(0, 0, 1)); }, "The 'action' argument is not allowed to be null");
            if (delayedAction != null) { }
        }

        [Test]
        public static void TestShortDelayButNoStart()
        {
            bool wasHere = false;
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1)))
            {
                Thread.Sleep(50);
                Assert.That(wasHere, Is.False, "The event should not be triggered until started.");
            }
        }

        [Test]
        public static void TestShortDelayAndImmediateStart()
        {
            bool wasHere = false;
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1)))
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
            using (DelayedAction delayedAction = new DelayedAction(() => { ++eventCount; }, new TimeSpan(0, 0, 0, 0, 5)))
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
            using (DelayedAction delayedAction = new DelayedAction(() => { wasHere = true; }, new TimeSpan(0, 0, 0, 0, 1)))
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
            DelayedAction delayedAction = new DelayedAction(() => { }, new TimeSpan(0, 0, 0, 0, 1));
            delayedAction.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { delayedAction.RestartIdleTimer(); });
        }
    }
}