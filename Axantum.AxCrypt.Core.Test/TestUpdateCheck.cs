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
using Axantum.AxCrypt.Core.Runtime;
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
            _environment = OS.Current;
            OS.Current = _fakeRuntimeEnvironment = new FakeRuntimeEnvironment();
        }

        [TestFixtureTearDown]
        public static void TeardownFixture()
        {
            OS.Current = _environment;
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
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable), "The new version was newer.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static void TestVersionUpdatedWithInvalidVersionFormatFromServer()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                // The version returned has 5 components - bad!
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.307.0.0"",""R"":307,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "This is not a successful check, and it was DateTime.MinValue since the last.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(UpdateCheck.VersionUnknown), "The new version has 5 components, and should be parsed as unknown.");

            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                // The version returned is an empty string - bad!
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":"""",""R"":307,""S"":0,""M"":""OK""}");
            };

            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "This is not a successful check, and it was DateTime.MinValue since the last.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(UpdateCheck.VersionUnknown), "The new version is an empty string and should be parse as unknown.");
        }

        [Test]
        public static void TestArgumentNullException()
        {
            string nullVersion = null;
            Uri nullUrl = null;
            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 307, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                Assert.Throws<ArgumentNullException>(() => { updateCheck.CheckInBackground(DateTime.MinValue, nullVersion, restApiUrl, updateWebPageUrl); }, "Null argument should throw.");
                Assert.Throws<ArgumentNullException>(() => { updateCheck.CheckInBackground(DateTime.MinValue, newVersion.ToString(), nullUrl, updateWebPageUrl); }, "Null argument should throw.");
                Assert.Throws<ArgumentNullException>(() => { updateCheck.CheckInBackground(DateTime.MinValue, newVersion.ToString(), restApiUrl, nullUrl); }, "Null argument should throw.");

                updateCheck.WaitForBackgroundCheckComplete();
            }
        }

        [Test]
        public static void TestDoubleDisposeAndObjectDisposedException()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.307.0"",""R"":307,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 307, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            UpdateCheck updateCheck = new UpdateCheck(thisVersion);
            updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
            {
                eventArgs = e;
            };
            updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
            updateCheck.WaitForBackgroundCheckComplete();
            updateCheck.Dispose();

            Assert.DoesNotThrow(updateCheck.Dispose);
            Assert.Throws<ObjectDisposedException>(() => { updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl); });
            Assert.Throws<ObjectDisposedException>(updateCheck.WaitForBackgroundCheckComplete);

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable), "The new version was newer.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static void TestVersionNotUpdatedNotCheckedBefore()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.207.0"",""R"":207,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 207, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.IsUpToDateOrRecentlyChecked), "The new version was older, so this version is up to date.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static void TestVersionSameAndCheckedRecently()
        {
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.300.0"",""R"":300,""S"":0,""M"":""OK""}");
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 300, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(utcNow.AddDays(-2), UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.IsUpToDateOrRecentlyChecked), "The new version was the same and we checked recently.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back.");
        }

        [Test]
        public static void TestVersionAlreadyCheckedRecently()
        {
            bool wasCalled = false;
            FakeWebCaller webCaller = new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.400.0"",""R"":300,""S"":0,""M"":""OK""}");
            webCaller.Calling += (object sender, EventArgs e) => { wasCalled = true; };
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return webCaller;
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(utcNow.AddHours(-1), thisVersion.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.IsUpToDateOrRecentlyChecked), "No check should be made, and it is assumed this version is up to date.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(updateWebPageUrl), "The original URL should be passed in the event args since no call is made.");
            Assert.That(wasCalled, Is.False, "The web caller should never be called.");
            Assert.That(eventArgs.Version, Is.EqualTo(thisVersion), "The new version should not be passed back, since no call should be made.");
        }

        [Test]
        public static void TestOnlyOneCallMadeWhenCheckIsMadeWithCheckPending()
        {
            int calls = 0;
            ManualResetEvent wait = new ManualResetEvent(false);
            FakeWebCaller webCaller = new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.400.0"",""R"":300,""S"":0,""M"":""OK""}");
            webCaller.Calling += (object sender, EventArgs e) => { wait.WaitOne(); ++calls; };
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return webCaller;
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Version newVersion = new Version(2, 0, 400, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                wait.Set();
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.NewerVersionIsAvailable), "One check should be made, indicating a newer version is available.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The new URL should be passed since a call is made.");
            Assert.That(calls, Is.EqualTo(1), "The web caller should only be called once.");
            Assert.That(eventArgs.Version, Is.EqualTo(newVersion), "The new version should be passed back, since one call should be made.");
        }

        [Test]
        public static void TestExceptionDuringVersionCall()
        {
            FakeWebCaller webCaller = new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.400.0"",""R"":300,""S"":0,""M"":""OK""}");
            webCaller.Calling += (object sender, EventArgs e) => { throw new InvalidOperationException("Oops - a forced exception during the call."); };
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return webCaller;
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "No check could be made, and it was a long time since a check was made.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(updateWebPageUrl), "The original URL should be passed since the call failed.");
            Assert.That(eventArgs.Version, Is.EqualTo(UpdateCheck.VersionUnknown), "An unknown version should be returned, since the call failed.");
        }

        [Test]
        public static void TestExceptionDuringVersionCallButRecentlyChecked()
        {
            FakeWebCaller webCaller = new FakeWebCaller(@"{""U"":""http://localhost/AxCrypt/Downloads.html"",""V"":""2.0.400.0"",""R"":300,""S"":0,""M"":""OK""}");
            webCaller.Calling += (object sender, EventArgs e) => { throw new InvalidOperationException("Oops - a forced exception during the call."); };
            _fakeRuntimeEnvironment.WebCallerCreator = () =>
            {
                return webCaller;
            };

            DateTime utcNow = DateTime.UtcNow;
            _fakeRuntimeEnvironment.TimeFunction = () => { return utcNow; };

            Version thisVersion = new Version(2, 0, 300, 0);
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(utcNow.AddDays(-2), UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called with non-null VersionEventArgs.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.ShortTimeSinceLastSuccessfulCheck), "Although the check failed, a check was recently made a short time ago.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(updateWebPageUrl), "The original URL should be passed since the call failed.");
            Assert.That(eventArgs.Version, Is.EqualTo(UpdateCheck.VersionUnknown), "An unknown version should be returned, since the call failed.");
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
            Uri restApiUrl = new Uri("http://localhost/RestApi.asxh/axcrypt2version");
            Uri updateWebPageUrl = new Uri("http://www.axantum.com/");
            VersionEventArgs eventArgs = null;
            using (UpdateCheck updateCheck = new UpdateCheck(thisVersion))
            {
                updateCheck.VersionUpdate += (object sender, VersionEventArgs e) =>
                {
                    eventArgs = e;
                };
                updateCheck.CheckInBackground(DateTime.MinValue, UpdateCheck.VersionUnknown.ToString(), restApiUrl, updateWebPageUrl);
                updateCheck.WaitForBackgroundCheckComplete();
            }

            Assert.That(eventArgs, Is.Not.Null, "The VersionUpdate event should be called even when an invalid version is returned.");
            Assert.That(eventArgs.VersionUpdateStatus, Is.EqualTo(VersionUpdateStatus.LongTimeSinceLastSuccessfulCheck), "No check has been performed previously and no new version is known.");
            Assert.That(eventArgs.UpdateWebpageUrl, Is.EqualTo(new Uri("http://localhost/AxCrypt/Downloads.html")), "The right URL should be passed in the event args.");
            Assert.That(eventArgs.Version, Is.EqualTo(UpdateCheck.VersionUnknown), "The version is not known since it could not be parsed.");
        }
    }
}