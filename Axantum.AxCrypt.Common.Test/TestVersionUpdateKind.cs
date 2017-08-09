using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axantum.AxCrypt.Common.Test
{
    [TestFixture]
    public class TestVersionUpdateKind
    {
        [Test]
        public void TestVersionUpdateLevelsNoNeed()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.0.3000.0", string.Empty, string.Empty);

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsNeedsSecurityUpdate()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.0.3000.0", string.Empty, "2.0.0.0-2.0.3001.0");

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsNeedsReliabilityUpdate()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.1.3000.0", "2.0.0.0-2.1.3010.0", "2.0.0.0-2.0.4000.0");

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsNoNeedBecauseIsVersionZero()
        {
            VersionUpdateKind kind = new VersionUpdateKind(string.Empty, "2.0.0.0-2.1.3010.0", "2.0.0.0-2.0.4000.0");

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsIsNeededBecauseSpecificVersion()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.1.3000.0", "2.1.3000.0", string.Empty);

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsIsNeededBecauseOneOfSeveralSpecificVersions()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.1.3001.0", "2.1.3000.0 2.1.3001.0 2.1.3002.0", "2.1.3000.0 2.1.3002.0 2.1.3003.0");

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.False, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }

        [Test]
        public void TestVersionUpdateLevelsIsNeededBecauseOneOfSeveralSpecificVersionsAndRange()
        {
            VersionUpdateKind kind = new VersionUpdateKind("2.1.3001.0", "2.1.3000.0 2.1.0.0-2.1.2000.0 2.1.3001.0 2.1.3002.0", "2.1.3000.0 2.1.3001.0 2.1.3003.0-2.1.30004");

            Assert.That(kind.NeedsCriticalReliabilityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalReliabilityUpdate));
            Assert.That(kind.NeedsCriticalSecurityUpdate, Is.True, nameof(VersionUpdateKind.NeedsCriticalSecurityUpdate));
        }
    }
}