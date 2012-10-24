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
    public static class TestProgressContext
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
        public static void TestProgressNoMax()
        {
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            int percent = -1;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progress.AddCount(100);
            Assert.That(percent, Is.EqualTo(0), "Since there is no Total set, the percentage should always be zero.");
        }

        [Test]
        public static void TestCurrentAndMax()
        {
            ProgressContext progress = new ProgressContext();
            progress.NotifyLevelStart();
            int percent = -1;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progress.AddTotal(99);
            progress.NotifyLevelFinished();
            Assert.That(percent, Is.EqualTo(100), "After Finished(), Percent should always be 100.");
        }

        [Test]
        public static void TestPercent()
        {
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            int percent = -1;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progress.AddTotal(200);
            progress.AddCount(100);
            Assert.That(percent, Is.EqualTo(50), "When halfway, the percent should be 50.");
        }

        [Test]
        public static void TestSeveralCallsToCount()
        {
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            int percent = -1;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            FakeRuntimeEnvironment fakeEnvironment = (FakeRuntimeEnvironment)OS.Current;
            progress.AddTotal(500);
            progress.AddCount(50);
            progress.AddCount(50);
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(1);
            progress.AddCount(50);
            progress.AddCount(50);
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(2);
            progress.AddCount(50);
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromSeconds(2);
            Assert.That(percent, Is.EqualTo(50), "When halfway, the percent should be 50.");
        }

        [Test]
        public static void TestFirstDelay()
        {
            FakeRuntimeEnvironment fakeEnvironment = (FakeRuntimeEnvironment)OS.Current;
            ProgressContext progress = new ProgressContext(TimeSpan.FromMilliseconds(13));
            bool wasHere = false;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                wasHere = true;
            };
            progress.AddTotal(100);
            progress.AddCount(50);
            Assert.That(wasHere, Is.False, "No progress should be raised, since the first delay time has not elapsed as yet.");

            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(12);
            progress.AddCount(1);
            Assert.That(wasHere, Is.False, "No progress should be raised, since the first delay time has not elapsed as yet.");

            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(13);
            progress.AddCount(1);
            Assert.That(wasHere, Is.True, "Progress should be raised, since the first delay time has now elapsed.");
        }

        [Test]
        public static void TestAddTotalAfterFinished()
        {
            ProgressContext progress = new ProgressContext();
            progress.NotifyLevelStart();
            progress.NotifyLevelFinished();

            Assert.Throws<InvalidOperationException>(() => { progress.AddTotal(1); });
        }

        [Test]
        public static void TestAddCountAfterFinished()
        {
            ProgressContext progress = new ProgressContext();
            progress.NotifyLevelStart();
            progress.NotifyLevelFinished();

            Assert.Throws<InvalidOperationException>(() => { progress.AddCount(1); });
        }

        [Test]
        public static void TestDoubleNotifyFinished()
        {
            ProgressContext progress = new ProgressContext();
            progress.NotifyLevelStart();
            progress.NotifyLevelStart();
            progress.NotifyLevelFinished();

            Assert.DoesNotThrow(() => { progress.NotifyLevelFinished(); });
        }

        [Test]
        public static void TestTooManyNotifyFinished()
        {
            ProgressContext progress = new ProgressContext();

            Assert.Throws<InvalidOperationException>(() => { progress.NotifyLevelFinished(); });
        }

        [Test]
        public static void TestProgressTo100AndAboveShouldOnlyReturn99BeforeFinishedPercent()
        {
            FakeRuntimeEnvironment fakeEnvironment = (FakeRuntimeEnvironment)OS.Current;
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(1000);

            ProgressContext progress = new ProgressContext(TimeSpan.FromMilliseconds(1000));
            progress.NotifyLevelStart();
            int percent = 0;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
                {
                    percent = e.Percent;
                };
            progress.AddTotal(2);
            Assert.That(percent, Is.EqualTo(0), "No progress yet - should be zero.");
            progress.AddCount(1);
            Assert.That(percent, Is.EqualTo(50), "Halfway should be 50 percent.");
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(2000);
            progress.AddCount(1);
            Assert.That(percent, Is.EqualTo(99), "Even at 100 should report 99 percent.");
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(3000);
            progress.AddCount(1000);
            Assert.That(percent, Is.EqualTo(99), "Even at very much above 100 should report 99 percent.");
            progress.NotifyLevelFinished();
            Assert.That(percent, Is.EqualTo(100), "Only when NotifyFinished() is called should 100 percent be reported.");
        }

        [Test]
        public static void TestAddingNegativeCount()
        {
            FakeRuntimeEnvironment fakeEnvironment = (FakeRuntimeEnvironment)OS.Current;
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(1000);

            ProgressContext progress = new ProgressContext(TimeSpan.FromMilliseconds(1000));
            int percent = 0;
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progress.AddTotal(2);
            Assert.That(percent, Is.EqualTo(0), "No progress yet - should be zero.");
            progress.AddCount(-100);
            Assert.That(percent, Is.EqualTo(0), "Nothing should happen adding negative counts.");
            fakeEnvironment.CurrentTiming.CurrentTiming = TimeSpan.FromMilliseconds(2000);
            progress.AddCount(1);
            Assert.That(percent, Is.EqualTo(50), "1 of 2 is 50 percent.");
        }

        [Test]
        public static void TestProgressWithoutSynchronizationContext()
        {
            bool didProgress = false;
            SynchronizationContext currentContext = new SynchronizationContext();
            Thread thread = new Thread(
                (object state) =>
                {
                    currentContext = SynchronizationContext.Current;
                    ProgressContext progress = new ProgressContext();
                    progress.NotifyLevelStart();
                    progress.Progressing += (object sender, ProgressEventArgs e) =>
                        {
                            didProgress = true;
                        };
                    progress.NotifyLevelFinished();
                }
                );
            thread.Start();
            thread.Join();
            Assert.That(didProgress, "There should always be one Progressing event after NotifyFinished().");
            Assert.That(currentContext, Is.Null, "There should be no SynchronizationContext here.");
        }

        private class StateForSynchronizationContext
        {
            public bool DidProgress { get; set; }

            public SynchronizationContext SynchronizationContext { get; set; }

            public ManualResetEvent WaitEvent { get; set; }
        }

        [Test]
        public static void TestProgressWithSynchronizationContext()
        {
            SynchronizationContext synchronizationContext = new SynchronizationContext();
            StateForSynchronizationContext s = new StateForSynchronizationContext();
            s.WaitEvent = new ManualResetEvent(false);
            s.SynchronizationContext = synchronizationContext;
            synchronizationContext.Post(
                (object state) =>
                {
                    StateForSynchronizationContext ss = (StateForSynchronizationContext)state;
                    SynchronizationContext.SetSynchronizationContext(ss.SynchronizationContext);
                    ss.SynchronizationContext = SynchronizationContext.Current;

                    ProgressContext progress = new ProgressContext();
                    progress.NotifyLevelStart();
                    progress.Progressing += (object sender, ProgressEventArgs e) =>
                    {
                        ss.DidProgress = true;
                    };
                    progress.NotifyLevelFinished();
                    ss.WaitEvent.Set();
                }, s);
            bool waitOk = s.WaitEvent.WaitOne(TimeSpan.FromSeconds(10), false);
            Assert.That(waitOk, "The wait should not time-out");
            Assert.That(s.SynchronizationContext, Is.EqualTo(synchronizationContext), "The SynchronizationContext should be current in the code executed.");
            Assert.That(s.DidProgress, "There should always be one Progressing event after NotifyFinished().");
        }

        [Test]
        public static void TestCancel()
        {
            ProgressContext progress = new ProgressContext();
            progress.NotifyLevelStart();
            progress.AddTotal(100);
            progress.AddCount(10);
            progress.Cancel = true;
            Assert.Throws<OperationCanceledException>(() => { progress.NotifyLevelStart(); });
            Assert.Throws<OperationCanceledException>(() => { progress.NotifyLevelFinished(); });
            Assert.Throws<OperationCanceledException>(() => { progress.AddTotal(50); });
            Assert.Throws<OperationCanceledException>(() => { progress.AddCount(10); });
        }

        [Test]
        public static void TestMultipleAddTotal()
        {
            int percent = 0;
            ProgressContext progress = new ProgressContext(TimeSpan.Zero);
            progress.Progressing += (object sender, ProgressEventArgs e) =>
            {
                percent = e.Percent;
            };
            progress.AddTotal(50);
            progress.AddTotal(50);
            progress.AddCount(50);
            Assert.That(percent, Is.EqualTo(50), "The total should be 100, so 50 is 50% and the progressing event should be raised at the first progress.");
        }
    }
}