#region Coypright and License

/*
 * AxCrypt - Copyright 2014, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
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
            Assert.That(sessionEvent.FullName, Is.EqualTo(String.Empty));
            Assert.That(sessionEvent.Identity.Equals(LogOnIdentity.Empty));

            LogOnIdentity key = new LogOnIdentity("key");
            sessionEvent = new SessionNotification(SessionNotificationType.KnownKeyChange, key);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.KnownKeyChange));
            Assert.That(sessionEvent.FullName, Is.EqualTo(String.Empty));
            Assert.That(sessionEvent.Identity, Is.EqualTo(key));

            string fullName = @"C:\Test\Test.txt";
            sessionEvent = new SessionNotification(SessionNotificationType.ActiveFileChange, fullName);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.ActiveFileChange));
            Assert.That(sessionEvent.FullName, Is.EqualTo(fullName));
            Assert.That(sessionEvent.Identity.Equals(LogOnIdentity.Empty));

            fullName = @"C:\Test\";
            sessionEvent = new SessionNotification(SessionNotificationType.WatchedFolderAdded, key, fullName);
            Assert.That(sessionEvent.NotificationType, Is.EqualTo(SessionNotificationType.WatchedFolderAdded));
            Assert.That(sessionEvent.FullName, Is.EqualTo(fullName));
            Assert.That(sessionEvent.Identity, Is.EqualTo(key));
        }

        [Test]
        public static void TestEquality()
        {
            LogOnIdentity key = new LogOnIdentity("passphrase1");
            string fullName = @"C:\Test\Test.txt";

            SessionNotification sessionEventA1 = new SessionNotification(SessionNotificationType.ActiveFileChange);
            SessionNotification sessionEventA2 = new SessionNotification(SessionNotificationType.ActiveFileChange);
            SessionNotification sessionEventA3 = new SessionNotification(SessionNotificationType.KnownKeyChange);

            SessionNotification sessionEventB1 = new SessionNotification(SessionNotificationType.ActiveFileChange, key);
            SessionNotification sessionEventB2 = new SessionNotification(SessionNotificationType.ActiveFileChange, key);
            SessionNotification sessionEventB3 = new SessionNotification(SessionNotificationType.ActiveFileChange, new LogOnIdentity("passphrase2"));

            SessionNotification sessionEventC1 = new SessionNotification(SessionNotificationType.ActiveFileChange, fullName);
            SessionNotification sessionEventC2 = new SessionNotification(SessionNotificationType.ActiveFileChange, fullName);
            SessionNotification sessionEventC3 = new SessionNotification(SessionNotificationType.ActiveFileChange, @"D:\Other\Different.txt");

            SessionNotification sessionEventD1 = new SessionNotification(SessionNotificationType.ActiveFileChange, key, fullName);
            SessionNotification sessionEventD2 = new SessionNotification(SessionNotificationType.ActiveFileChange, key, fullName);
            SessionNotification sessionEventD3 = new SessionNotification(SessionNotificationType.ActiveFileChange, new LogOnIdentity("passphrase"), fullName);

            SessionNotification nullSessionEvent = null;
            SessionNotification sessionEventA1Alias = sessionEventA1;
            SessionNotification nullSessionEvent2 = null;

            Assert.That(sessionEventD1.GetHashCode(), Is.EqualTo(sessionEventD2.GetHashCode()));
            Assert.That(sessionEventC1.GetHashCode(), Is.Not.EqualTo(sessionEventD1.GetHashCode()));

            Assert.That(sessionEventA1 == sessionEventA1Alias);
            Assert.That(nullSessionEvent != sessionEventA1);
            Assert.That(sessionEventA1 != nullSessionEvent);
            Assert.That(nullSessionEvent == nullSessionEvent2);

            Assert.That(sessionEventA1 == sessionEventA2);
            Assert.That(sessionEventA1 != sessionEventA3);

            Assert.That(sessionEventB1 == sessionEventB2);
            Assert.That(sessionEventB1 != sessionEventB3);

            Assert.That(sessionEventC1 == sessionEventC2);
            Assert.That(sessionEventC1 != sessionEventC3);

            Assert.That(sessionEventD1 == sessionEventD2);
            Assert.That(sessionEventD1 != sessionEventD3);

            object objectSessionEventD3 = sessionEventD3;
            object objectSessionEventD1 = sessionEventD1;

            Assert.That(objectSessionEventD1.Equals(sessionEventD1));
            Assert.That(!objectSessionEventD1.Equals(objectSessionEventD3));
            Assert.That(!objectSessionEventD1.Equals(new SymmetricKey(128)));
        }
    }
}