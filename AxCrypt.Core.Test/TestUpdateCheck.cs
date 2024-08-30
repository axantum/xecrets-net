#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Abstractions.Rest;
using AxCrypt.Common;
using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static AxCrypt.Abstractions.TypeResolve;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestUpdateCheck
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static async Task TestVersionUpdated()
        {
            TypeMap.Register.New<IRestCaller>(
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.307.0"",""revision"":307}")
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 307, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
            {
                eventArgs = e;
            };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable), "The new version was newer.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static async Task TestVersionUpdatedWithInvalidVersionFormatFromServer()
        {
            TypeMap.Register.New<IRestCaller>(
                // The version returned has 5 components - bad!
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.307.0.0"",""revision"":307}")
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "This is not a successful check, and it was DateTime.MinValue since the last.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://www.axantum.com/")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(DownloadVersion.VersionUnknown), "The new version has 5 components, and should be parsed as unknown.");

            TypeMap.Register.New<IRestCaller>(
                // The version returned is an empty string - bad!
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":"""",""revision"":307}")
            );

            updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "This is not a successful check, and it was DateTime.MinValue since the last.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://www.axantum.com/")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(DownloadVersion.VersionUnknown), "The new version is an empty string and should be parsed as unknown.");
        }

        [Test]
        public static void TestArgumentNullException()
        {
            string nullVersion = null;
            Uri nullUrl = null;
            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 307, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            Assert.ThrowsAsync<ArgumentNullException>(async () => { await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, nullVersion, updateWebPageUrl, String.Empty); }, "Null argument should throw.");
            Assert.ThrowsAsync<ArgumentNullException>(async () => { await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, newVersion.ToString(), nullUrl, String.Empty); }, "Null argument should throw.");
        }

        [Test]
        public static async Task TestVersionNotUpdatedNotCheckedBefore()
        {
            TypeMap.Register.New<IRestCaller>(
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.207.0"",""revision"":207}")
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 207, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.IsUpToDate), "The new version was older, so this version is up to date.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static async Task TestVersionSameAndCheckedRecently()
        {
            TypeMap.Register.New<IRestCaller>(
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.300.0"",""revision"":300}")
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(utcNow.AddDays(-2), DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.IsUpToDate), "The new version was the same and we checked recently.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static async Task TestVersionAlreadyCheckedRecently()
        {
            bool wasCalled = false;
            FakeRestCaller restCaller = new FakeRestCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.400.0"",""R"":300,""S"":0,""M"":""OK""}");
            restCaller.Calling += (object sender, EventArgs e) => { wasCalled = true; };
            TypeMap.Register.New<IRestCaller>(
                () => restCaller
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(utcNow.AddHours(-1), thisVersion.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.IsUpToDate), "No check should be made, and it is assumed this version is up to date.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(updateWebPageUrl), "The original URL should be passed in the event args since no call is made.");
            Assert.That(wasCalled, Is.False, "The web caller should never be called.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(thisVersion), "The new version should not be passed back, since no call should be made.");
        }

        [Test]
        public static async Task TestOnlyOneCallMadeWhenCheckIsMadeWithCheckPending()
        {
            int calls = 0;
            ManualResetEvent wait = new ManualResetEvent(false);
            FakeRestCaller restCaller = new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.400.0"",""revision"":300}");
            restCaller.Calling += (object sender, EventArgs e) => { wait.WaitOne(); ++calls; };
            TypeMap.Register.New<IRestCaller>(
                () => restCaller
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 400, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            Task t1 = updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);
            wait.Set();
            await t1;

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable), "One check should be made, indicating a newer version is available.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The new URL should be passed since a call is made.");
            Assert.That(calls, Is.EqualTo(1), "The web caller should only be called once.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(newVersion), "The new version should be passed back, since one call should be made.");
        }

        [Test]
        public static async Task TestExceptionDuringVersionCall()
        {
            FakeRestCaller restCaller = new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.400.0"",""revision"":300}");
            restCaller.Calling += (object sender, EventArgs e) => { throw new InvalidOperationException("Oops - a forced exception during the call."); };
            TypeMap.Register.New<IRestCaller>(
                () => restCaller
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "No check could be made, and it was a long time since a check was made.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(updateWebPageUrl), "The original URL should be passed since the call failed.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(DownloadVersion.VersionUnknown), "An unknown version should be returned, since the call failed.");
        }

        [Test]
        public static async Task TestExceptionDuringVersionCallButRecentlyChecked()
        {
            FakeRestCaller restCaller = new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""2.0.400.0"",""revision"":300}");
            restCaller.Calling += (object sender, EventArgs e) => { throw new InvalidOperationException("Oops - a forced exception during the call."); };
            TypeMap.Register.New<IRestCaller>(
                () => restCaller
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(utcNow.AddDays(-2), DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck), "Although the check failed, a check was recently made a short time ago.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(updateWebPageUrl), "The original URL should be passed since the call failed.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(DownloadVersion.VersionUnknown), "An unknown version should be returned, since the call failed.");
        }

        [Test]
        public static async Task TestInvalidVersionReturned()
        {
            TypeMap.Register.New<IRestCaller>(
                () => new FakeRestCaller(@"{""url"":""http://localhost/AxCrypt/Downloads.html"",""version"":""x.y.z.z"",""revision"":207}")
            );

            DateTime utcNow = DateTime.UtcNow;
            ((FakeNow)New<INow>()).TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            AxCryptUpdateCheck updateCheck = new AxCryptUpdateCheck(thisVersion);
            updateCheck.AxCryptUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
            await updateCheck.CheckInBackgroundAsync(DateTime.MinValue, DownloadVersion.VersionUnknown.ToString(), updateWebPageUrl, String.Empty);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called even when an invalid version is returned.");
            Assert.That(eventArgs.DownloadVersion.CalculateStatus(thisVersion, utcNow, eventArgs.LastUpdateCheck), Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "No check has been performed previously and no new version is known.");
            Assert.That(eventArgs.DownloadVersion.Url, Is.EqualTo(new Uri("http://www.axantum.com/")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.DownloadVersion.Version, Is.EqualTo(DownloadVersion.VersionUnknown), "The version is not known since it could not be parsed.");
        }
    }
}
