#region Coypright and License

/*
 * AxCrypt - Copyright 2012, Svante Seleborg, All Rights Reserved
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestUpdateCheck
    {
        private static IRuntimeEnvironment _environment;

        private static FakeRuntimeEnvironment _fakeRuntimeEnvironment;

        [TestFixtureSetUp]
        public static void SetupFixture()
        {
            _environment = AxCryptEnvironment.Current;
            AxCryptEnvironment.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            AxCryptEnvironment.Current = _environment;
            FakeRuntimeFileInfo.ClearFiles();
        }

        [Test]
        public static void TestVersionUpdated()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.307.0"",""R"":307,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 307, 0);
            Uri restApi = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion, restApi))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.Check();
                updateCheck.Wait();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.NewerWasFound, Is.True, "The new version was newer.");
            Assert.That(eventArgs.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
            Assert.That(eventArgs.LastCheckUtc, Is.EqualTo(utcNow), "The last check time should be update as expected.");
        }

        [Test]
        public static void TestVersionNotUpdated()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.207.0"",""R"":207,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 207, 0);
            Uri restApi = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion, restApi))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.Check();
                updateCheck.Wait();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.NewerWasFound, Is.False, "The new version was older.");
            Assert.That(eventArgs.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
            Assert.That(eventArgs.LastCheckUtc, Is.EqualTo(utcNow), "The last check time should be update as expected.");
        }

        [Test]
        public static void TestInvalidVersionReturned()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""x.y.z.z"",""R"":207,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri restApi = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion, restApi))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.Check();
                updateCheck.Wait();
            }

            Assert.That(eventArgs, Is.Null, "The VersionUpdate event should not be called when an invalid version is returned.");
        }
    }
}