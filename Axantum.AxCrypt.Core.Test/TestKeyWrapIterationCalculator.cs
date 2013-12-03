#region Coypright and License

/*
 * AxCrypt - Copyright 2013, Svante Seleborg, All Rights Reserved
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

using Axantum.AxCrypt.Core.Crypto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axantum.AxCrypt.Core.Test
{
    public static class TestKeyWrapIterationCalculator
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