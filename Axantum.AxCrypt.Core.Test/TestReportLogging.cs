using Axantum.AxCrypt.Abstractions;
using Axantum.AxCrypt.Core.IO;
using Axantum.AxCrypt.Core.Portable;
using Axantum.AxCrypt.Core.Test.Properties;
using Axantum.AxCrypt.Desktop;
using Axantum.AxCrypt.Fake;
using Axantum.AxCrypt.Mono.Portable;
using AxCrypt.Content;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Axantum.AxCrypt.Abstractions.TypeResolve;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestReportLogging
    {
        [SetUp]
        public static void Setup()
        {
            TypeMap.Register.Singleton<INow>(() => new FakeNow());
            TypeMap.Register.New<string, IDataStore>((path) => new FakeDataStore(path));
            TypeMap.Register.Singleton<IPortableFactory>(() => new PortableFactory());
            TypeMap.Register.Singleton<FileLocker>(() => new FileLocker());
            FakeDataStore.AddFile(@"c:\test\ReportSnapshot.txt", new MemoryStream(Resources.ReportSnapshot));
            FakeDataStore.AddFile(@"c:\test\ReportSnapshot.1.txt", new MemoryStream(Resources.ReportSnapshot_1));
        }

        [TearDown]
        public static void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public static void TestReportSimpleExceptionLogging()
        {
            IReport report = new Report(@"c:\test\");
            report.Exception(new Exception("This is a test"));
            string logText = new StreamReader(New<IDataStore>(@"c:\test\ReportSnapshot.txt").OpenRead(), Encoding.UTF8).ReadToEnd();

            Assert.That(logText.Contains(Texts.ReportSnapshotIntro), "Report log header not found.");
            Assert.That(logText.Contains("This is a test"), "Exception detail not found.");

            report.Exception(new Exception("This is a test2"));

            Regex searchFor = new Regex(Texts.ReportSnapshotIntro);
            int numberOfTimes = searchFor.Matches(logText).Count;

            Assert.That(numberOfTimes == 1, "Report log header present multiple time.");
        }
    }
}