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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Extensions;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestSessionEvent
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
        public static void TestConstructors()
        {
            SessionNotification sessionEvent;

            sessionEvent = new SessionNotification(SessionNotificationType.ProcessExit);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.ProcessExit));
            Assert.That(sessionEvent.FullNames.Count(), Is.EqualTo(0));
            Assert.That(sessionEvent.Identity.Equals(LogOnIdentity.Empty));

            LogOnIdentity key = new LogOnIdentity("key");
            sessionEvent = new SessionNotification(SessionNotificationType.KnownKeyChange, key);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.KnownKeyChange));
            Assert.That(sessionEvent.FullNames.Count(), Is.EqualTo(0));
            Assert.That(sessionEvent.Identity, Is.EqualTo(key));

            string fullName = @"C:\Test\Test.txt".NormalizeFilePath();
            sessionEvent = new SessionNotification(SessionNotificationType.ActiveFileChange, fullName);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.ActiveFileChange));
            Assert.That(sessionEvent.FullNames.First(), Is.EqualTo(fullName));
            Assert.That(sessionEvent.Identity.Equals(LogOnIdentity.Empty));

            fullName = @"C:\Test\".NormalizeFolderPath();
            sessionEvent = new SessionNotification(SessionNotificationType.WatchedFolderAdded, key, fullName);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.WatchedFolderAdded));
            Assert.That(sessionEvent.FullNames.First(), Is.EqualTo(fullName));
            Assert.That(sessionEvent.Identity, Is.EqualTo(key));
        }
    }
}