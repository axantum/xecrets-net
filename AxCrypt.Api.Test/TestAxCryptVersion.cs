using AxCrypt.Abstractions;
using AxCrypt.Api.Model;
using AxCrypt.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCrypt.Api.Test
{
    [TestFixture]
    public class TestAxCryptVersion
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
            TypeMap.Register.Clear();
        }

        [Test]
        public void TestAxCryptVersionIsEmpty()
        {
            AxCryptVersion version = new AxCryptVersion(String.Empty, VersionUpdateKind.Empty);

            Assert.That(version.IsEmpty, Is.True, nameof(AxCryptVersion.IsEmpty));
        }
    }
}