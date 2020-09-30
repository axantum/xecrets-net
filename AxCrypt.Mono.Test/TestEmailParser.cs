using AxCrypt.Core.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Mono.Test
{
    [TestFixture]
    public static class TestEmailParser
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
        public static void TestEmailParserExtract()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("test@axantum.com");

            Assert.That(addresses.Count(), Is.EqualTo(1));
            Assert.That(addresses.First(), Is.EqualTo("test@axantum.com"));
        }

        [Test]
        public static void TestEmailParserExtractCaseFolding()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("TEST@axantum.com");

            Assert.That(addresses.Count(), Is.EqualTo(1));
            Assert.That(addresses.First(), Is.EqualTo("test@axantum.com"));
        }

        [Test]
        public static void TestEmailParserExtractEmpty()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract(string.Empty);

            Assert.That(addresses.Count(), Is.EqualTo(0));
        }

        [Test]
        public static void TestEmailParserExtractWithAdditional()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("recipients. This is a permanent error. The following address(es) failed:  test@gmail.com SMTP error from remote mail server after");

            Assert.That(addresses.Count(), Is.EqualTo(1));
            Assert.That(addresses.First(), Is.EqualTo("test@gmail.com"));
        }

        [Test]
        public static void TestEmailParserExtractWithAngleBrackets()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("Firstname Lastname <test@gmail.com>");

            Assert.That(addresses.Count(), Is.EqualTo(1));
            Assert.That(addresses.First(), Is.EqualTo("test@gmail.com"));
        }

        [Test]
        public static void TestEmailParserExtractValidSpecialCharacters()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("test.extra@gmail.com test+extra@gmail.com test_extra@gmail.com test'extra@gmail.com test0.extra@gmail.com");

            Assert.That(addresses.Count(), Is.EqualTo(5));
            string[] list = addresses.ToArray();

            Assert.That(list[0], Is.EqualTo("test.extra@gmail.com"));
            Assert.That(list[1], Is.EqualTo("test+extra@gmail.com"));
            Assert.That(list[2], Is.EqualTo("test_extra@gmail.com"));
            Assert.That(list[3], Is.EqualTo("test'extra@gmail.com"));
            Assert.That(list[4], Is.EqualTo("test0.extra@gmail.com"));
        }

        [Test]
        public static void TestEmailParserExtractInvalidSpecialCharacters()
        {
            EmailParser parser = new EmailParser();

            IEnumerable<string> addresses = parser.Extract("test$extra@gmail.com test?extra@gmail.com test/extra@gmail.com test!extra@gmail.com test0.extra@gmail.com");

            Assert.That(addresses.Count(), Is.EqualTo(5));
            string[] list = addresses.ToArray();

            Assert.That(list[0], Is.EqualTo("extra@gmail.com"));
            Assert.That(list[1], Is.EqualTo("extra@gmail.com"));
            Assert.That(list[2], Is.EqualTo("extra@gmail.com"));
            Assert.That(list[3], Is.EqualTo("extra@gmail.com"));
            Assert.That(list[4], Is.EqualTo("test0.extra@gmail.com"));
        }
    }
}