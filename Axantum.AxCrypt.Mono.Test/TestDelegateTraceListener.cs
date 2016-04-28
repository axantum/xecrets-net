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

using Axantum.AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestDelegateTraceListener
    {
        [SetUp]
        public static void Setup()
        {
        }

        [TearDown]
        public static void Teardown()
        {
        }

        [Test]
        public static void TestIt()
        {
            string listened = null;
            DelegateTraceListener listener = new DelegateTraceListener("Test Listener", (string message) => { listened = message; });

            listener.Write(1);
            Assert.That(listened, Is.Null, "The listener should buffer until a new line is received.");

            listener.WriteLine(String.Empty);
            Assert.That(listened, Is.EqualTo("1" + Environment.NewLine), "The buffer should be emptied as soon as a New Line is received.");

            listener.Write("Hello" + Environment.NewLine + "New World");
            Assert.That(listened, Is.EqualTo("Hello" + Environment.NewLine), "The buffer should contain text up to the last new line but not more.");
        }
    }
}