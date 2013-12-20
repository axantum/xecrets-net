using Axantum.AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Linq;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestCancelContext
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
        public static void TestAddRemoveProgressingEvent()
        {
            CancelContext progress = new CancelContext(new ProgressContext(TimeSpan.Zero));
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
            CancelContext cancelContext = new CancelContext(progressContext);

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
            CancelContext cancelContext = new CancelContext(progressContext);
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