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
using Axantum.AxCrypt.Core.System;
using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestProgressContext
    {
        [Test]
        public static void TestProgressNoMax()
        {
            ProgressContext progressContext = new ProgressContext(TimeSpan.Zero);
            int percent = -1;
            progressContext.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progressContext.Current = 100;
            Assert.That(percent, Is.EqualTo(0), "Since there is no Max set, the percentage should always be zero.");
        }

        [Test]
        public static void TestProgressContextContext()
        {
            object testObject = new object();
            ProgressContext progressContext = new ProgressContext("TestProgress", testObject, TimeSpan.Zero);
            object progressObject = null;
            progressContext.Progressing += (object sender, ProgressEventArgs e) =>
            {
                progressObject = e.Context;
            };
            progressContext.Current = 0;
            Assert.That(progressObject, Is.EqualTo(testObject), "The context should be passed exactly as is to the event.");
        }

        [Test]
        public static void TestDisplayText()
        {
            ProgressContext progressContext = new ProgressContext("TestProgress", null);
            Assert.That(progressContext.DisplayText, Is.EqualTo("TestProgress"), "The DisplayText property should reflect the value used in the constructor.");
        }

        [Test]
        public static void TestCurrentAndMax()
        {
            ProgressContext progressContext = new ProgressContext();
            progressContext.Max = 99;
            progressContext.Finished();
            Assert.That(progressContext.Current, Is.EqualTo(99), "After Finished(), Current should be equal to Max.");
        }

        [Test]
        public static void TestPercent()
        {
            ProgressContext progressContext = new ProgressContext();
            progressContext.Max = 200;
            progressContext.Current = 100;
            Assert.That(progressContext.Percent, Is.EqualTo(50), "When halfway, the percent should be 50.");
        }

        [Test]
        public static void TestFirstDelay()
        {
            IRuntimeEnvironment environment = OS.Current;
            FakeRuntimeEnvironment fakeEnvironment = new FakeRuntimeEnvironment();
            OS.Current = fakeEnvironment;
            try
            {
                ProgressContext progressContext = new ProgressContext(TimeSpan.FromMilliseconds(13));
                bool wasHere = false;
                progressContext.Progressing += (object sender, ProgressEventArgs e) =>
                {
                    wasHere = true;
                };
                progressContext.Max = 100;
                progressContext.Current = 50;
                Assert.That(wasHere, Is.False, "No progress should be raised, since the first delay time has not elapsed as yet.");

                fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(12);
                progressContext.Current = 51;
                Assert.That(wasHere, Is.False, "No progress should be raised, since the first delay time has not elapsed as yet.");

                fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(13);
                progressContext.Current = 52;
                Assert.That(wasHere, Is.True, "Progress should be raised, since the first delay time has now elapsed.");
            }
            finally
            {
                OS.Current = environment;
            }
        }
    }
}