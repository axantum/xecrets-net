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

using Axantum.AxCrypt.Core.Crypto;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            SessionEvent sessionEvent;

            sessionEvent = new SessionEvent(SessionEventType.ProcessExit);
            Assert.That(sessionEvent.SessionEventType, Is.EqualTo(SessionEventType.ProcessExit));
            Assert.That(sessionEvent.FullName, Is.Null);
            Assert.That(sessionEvent.Key, Is.Null);

            AesKey key = new AesKey();
            sessionEvent = new SessionEvent(SessionEventType.KnownKeyChange, key);
            Assert.That(sessionEvent.SessionEventType, Is.EqualTo(SessionEventType.KnownKeyChange));
            Assert.That(sessionEvent.FullName, Is.Null);
            Assert.That(sessionEvent.Key, Is.EqualTo(key));

            string fullName = @"C:\Test\Test.txt";
            sessionEvent = new SessionEvent(SessionEventType.ActiveFileChange, fullName);
            Assert.That(sessionEvent.SessionEventType, Is.EqualTo(SessionEventType.ActiveFileChange));
            Assert.That(sessionEvent.FullName, Is.EqualTo(fullName));
            Assert.That(sessionEvent.Key, Is.Null);

            fullName = @"C:\Test\";
            sessionEvent = new SessionEvent(SessionEventType.WatchedFolderAdded, key, fullName);
            Assert.That(sessionEvent.SessionEventType, Is.EqualTo(SessionEventType.WatchedFolderAdded));
            Assert.That(sessionEvent.FullName, Is.EqualTo(fullName));
            Assert.That(sessionEvent.Key, Is.EqualTo(key));
        }

        [Test]
        public static void TestEquality()
        {
            AesKey key = new AesKey();
            string fullName = @"C:\Test\Test.txt";

            SessionEvent sessionEventA1 = new SessionEvent(SessionEventType.ActiveFileChange);
            SessionEvent sessionEventA2 = new SessionEvent(SessionEventType.ActiveFileChange);
            SessionEvent sessionEventA3 = new SessionEvent(SessionEventType.KnownKeyChange);

            SessionEvent sessionEventB1 = new SessionEvent(SessionEventType.ActiveFileChange, key);
            SessionEvent sessionEventB2 = new SessionEvent(SessionEventType.ActiveFileChange, key);
            SessionEvent sessionEventB3 = new SessionEvent(SessionEventType.ActiveFileChange, new AesKey());

            SessionEvent sessionEventC1 = new SessionEvent(SessionEventType.ActiveFileChange, fullName);
            SessionEvent sessionEventC2 = new SessionEvent(SessionEventType.ActiveFileChange, fullName);
            SessionEvent sessionEventC3 = new SessionEvent(SessionEventType.ActiveFileChange, @"D:\Other\Different.txt");

            SessionEvent sessionEventD1 = new SessionEvent(SessionEventType.ActiveFileChange, key, fullName);
            SessionEvent sessionEventD2 = new SessionEvent(SessionEventType.ActiveFileChange, key, fullName);
            SessionEvent sessionEventD3 = new SessionEvent(SessionEventType.ActiveFileChange, new AesKey(), fullName);

            SessionEvent nullSessionEvent = null;
            SessionEvent sessionEventA1Alias = sessionEventA1;
            SessionEvent nullSessionEvent2 = null;

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
            Assert.That(!objectSessionEventD1.Equals(new AesKey()));
        }
    }
}