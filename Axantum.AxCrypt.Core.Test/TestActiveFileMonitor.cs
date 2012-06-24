using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axantum.AxCrypt.Core.Session;
using NUnit.Framework;

namespace Axantum.AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestActiveFileMonitor
    {
        [Test]
        public static void TestConstructor()
        {
            using (ActiveFileMonitor monitor = new ActiveFileMonitor())
            {
            }
        }
    }
}