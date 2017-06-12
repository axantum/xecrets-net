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
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public static async Task TestNotificationDuringProcessingOfNotification()
        {
            int notificationCount = 0;

            SessionNotify monitor = new SessionNotify();
            monitor.AddCommand((notification) =>
            {
                if (notification.NotificationType == SessionNotificationType.LogOn)
                {
                    ++notificationCount;
                }
                return Constant.CompletedTask;
            });

            await monitor.NotifyAsync(new SessionNotification(SessionNotificationType.LogOn));
            Assert.That(notificationCount, Is.EqualTo(1));
        }
    }
}