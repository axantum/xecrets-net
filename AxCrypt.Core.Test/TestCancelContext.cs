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

using AxCrypt.Core.UI;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCancelContext
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
        public static void TestAddRemoveProgressingEvent()
        {
            CancelProgressContext progress = new CancelProgressContext(new ProgressContext(TimeSpan.Zero));
            int percent = -1;

            EventHandler<ProgressEventArgs> handler = (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };

            progress.Progressing += handler;
            progress.AddTotal(200);
            progress.AddCount(100);
            Assert.That(percent, Is.EqualTo(50), "When halfway, the percent should be 50.");

            FakeRuntimeEnvironment.Instance.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
            progress.AddCount(50);
            Assert.That(percent, Is.EqualTo(75), "150 of 200 is 75%.");

            progress.Progressing -= handler;

            FakeRuntimeEnvironment.Instance.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
            progress.AddCount(50);
            Assert.That(percent, Is.EqualTo(75), "Since the event handler was removed, percent should stay the same.");
        }

        [Test]
        public static void TestAllItemsConfirmed()
        {
            ProgressContext progressContext = new ProgressContext(TimeSpan.Zero);
            CancelProgressContext cancelContext = new CancelProgressContext(progressContext);

            Assert.That(progressContext.AllItemsConfirmed, Is.False, "The default value is false.");
            Assert.That(cancelContext.AllItemsConfirmed, Is.EqualTo(progressContext.AllItemsConfirmed), "The cancel context should have the same value as the progressContext.");

            cancelContext.AllItemsConfirmed = true;
            Assert.That(progressContext.AllItemsConfirmed, Is.True, "The progressContext should reflect the setting of the cancelContext.");
            Assert.That(cancelContext.AllItemsConfirmed, Is.EqualTo(progressContext.AllItemsConfirmed), "The cancel context should have the same value as the progressContext.");
        }

        [Test]
        public static void TestRemoveCount()
        {
            ProgressContext progressContext = new ProgressContext(TimeSpan.Zero);
            CancelProgressContext cancelContext = new CancelProgressContext(progressContext);
            int percent = 0;
            cancelContext.Progressing += (sender, e) =>
            {
                percent = e.Percent;
            };

            cancelContext.AddTotal(100);
            cancelContext.AddCount(10);
            Assert.That(percent, Is.EqualTo(10), "10 of 100 is 10%.");

            cancelContext.RemoveCount(50, 10);
            FakeRuntimeEnvironment.Instance.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(1000);
            cancelContext.AddCount(10);
            Assert.That(percent, Is.EqualTo(20), "10 of 50 is 20%.");
        }
    }
}
