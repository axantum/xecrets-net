using Axantum.AxCrypt.Core.Crypto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    internal static class TestKeyWrapIterationCalculator
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
        public static void TestMinimumGuarantee()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () =>
            {
                if (shouldTerminate)
                {
                    throw new InvalidOperationException("There should be no more calls at this point.");
                }
                if (callCounter++ == 0)
                {
                    return now;
                }
                if (callCounter < 5)
                {
                    return now.AddMilliseconds(callCounter * 50);
                }
                shouldTerminate = true;
                return now.AddMilliseconds(500);
            };

            long iterations = KeyWrapIterationCalculator.CalculatedKeyWrapIterations;

            Assert.That(iterations, Is.EqualTo(20000), "The minimum guarantee should hold.");
        }

        [Test]
        public static void TestCalculatedKeyWrapIterations()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            SetupAssembly.FakeRuntimeEnvironment.TimeFunction = () =>
            {
                if (shouldTerminate)
                {
                    throw new InvalidOperationException("There should be no more calls at this point.");
                }
                if (callCounter++ == 0)
                {
                    return now;
                }
                return now.AddMilliseconds(callCounter * 4);
            };

            long iterations = KeyWrapIterationCalculator.CalculatedKeyWrapIterations;

            Assert.That(iterations, Is.EqualTo(25000), "If we do 125000 iterations in 500ms, the result should be 25000 as default iterations.");
        }
    }
}
