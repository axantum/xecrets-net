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
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSessionNotificationMonitor
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
        public static void TestNotificationDuringProcessingOfNotification()
        {
            int notificationCount = 0;
            FakeSleep fakeSleep = new FakeSleep();
            FakeDelayTimer fakeTimer = new FakeDelayTimer(fakeSleep);
            DelayedAction delayedAction = new DelayedAction(fakeTimer, new TimeSpan(0, 0, 10));

            SessionNotificationMonitor monitor = new SessionNotificationMonitor(delayedAction);
            monitor.Notification += (sender, e) =>
            {
                if (e.Notification.NotificationType == SessionNotificationType.LogOn)
                {
                    ++notificationCount;
                    if (notificationCount == 1)
                    {
                        monitor.Notify(new SessionNotification(SessionNotificationType.LogOn));
                        fakeSleep.Time(new TimeSpan(0, 0, 10));
                    }
                }
            };

            monitor.Notify(new SessionNotification(SessionNotificationType.LogOn));
            Assert.That(notificationCount, Is.EqualTo(0));

            fakeSleep.Time(new TimeSpan(0, 0, 5));
            Assert.That(notificationCount, Is.EqualTo(0));

            fakeSleep.Time(new TimeSpan(0, 0, 5));
            Assert.That(notificationCount, Is.EqualTo(1));

            fakeSleep.Time(new TimeSpan(0, 0, 5));
            Assert.That(notificationCount, Is.EqualTo(1));

            fakeSleep.Time(new TimeSpan(0, 0, 5));
            Assert.That(notificationCount, Is.EqualTo(2));
        }

        [Test]
        public static void TestDoAllNow()
        {
            int notificationCount = 0;
            FakeSleep fakeSleep = new FakeSleep();
            FakeDelayTimer fakeTimer = new FakeDelayTimer(fakeSleep);
            DelayedAction delayedAction = new DelayedAction(fakeTimer, new TimeSpan(0, 0, 10));

            SessionNotificationMonitor monitor = new SessionNotificationMonitor(delayedAction);
            monitor.Notification += (sender, e) =>
            {
                if (e.Notification.NotificationType == SessionNotificationType.LogOn)
                {
                    ++notificationCount;
                }
            };

            monitor.Notify(new SessionNotification(SessionNotificationType.LogOn));
            Assert.That(notificationCount, Is.EqualTo(0));

            fakeSleep.Time(new TimeSpan(0, 0, 5));
            Assert.That(notificationCount, Is.EqualTo(0));

            while (monitor.NotifyPending)
            {
                monitor.NotifyNow();
            }
            Assert.That(notificationCount, Is.EqualTo(1));
        }
    }
}