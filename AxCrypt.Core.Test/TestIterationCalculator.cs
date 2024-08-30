#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions copyright © 2022-2024, Svante Seleborg, All Rights Reserved.
 *
 * This code file is part of Xecrets Cli, but is derived from AxCrypt as licensed under GPL v3 or later.
 * 
 * The changes and additions are separately copyrighted and only licensed under GPL v3 or later as detailed below,
 * unless explicitly licensed otherwise. If you use any part of these changes and additions in your software,
 * please see https://www.gnu.org/licenses/ for details of what this means for you.
 * 
 * Warning: If you are using the original AxCrypt code under a non-GPL v3 or later license, these changes and additions
 * are not included in that license. If you use these changes under those circumstances, all your code becomes subject to
 * the GPL v3 or later license, according to the principle of strong copyleft as applied to GPL v3 or later.
 *
 * Xecrets Cli is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * Xecrets Cli is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with Xecrets Cli. If not, see
 * https://www.gnu.org/licenses/.
 *
 * The source repository can be found at https://github.com/axantum/xecrets-net please go there for more information,
 * suggestions and contributions, as well for commit history detailing changes and additions that fall under the strong
 * copyleft provisions mentioned above. You may also visit https://www.axantum.com for more information about the author.
*/

#endregion Xecrets Cli Copyright and GPL License notice
#region Coypright and License

/*
 * AxCrypt - Copyright 2016, Svante Seleborg, All Rights Reserved
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
 * The source is maintained at http://bitbucket.org/AxCrypt.Desktop.Window-net please visit for
 * updates, contributions and contact with the author. You may also visit
 * http://www.axcrypt.net for more information about the author.
*/

#endregion Coypright and License

using AxCrypt.Abstractions;
using AxCrypt.Core.Crypto;
using AxCrypt.Fake;
using NUnit.Framework;
using System;
using System.Linq;

using static AxCrypt.Abstractions.TypeResolve;

#pragma warning disable 3016 // Attribute-arguments as arrays are not CLS compliant. Ignore this here, it's how NUnit works.

namespace AxCrypt.Core.Test
{
    [TestFixture(CryptoImplementation.Mono)]
    [TestFixture(CryptoImplementation.WindowsDesktop)]
    [TestFixture(CryptoImplementation.BouncyCastle)]
    public class TestIterationCalculator
    {
        private CryptoImplementation _cryptoImplementation;

        public TestIterationCalculator(CryptoImplementation cryptoImplementation)
        {
            _cryptoImplementation = cryptoImplementation;
        }

        [SetUp]
        public void Setup()
        {
            SetupAssembly.AssemblySetup(_cryptoImplementation);
        }

        [TearDown]
        public void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public void TestMinimumGuaranteeV1KeyWrapIterations()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            ((FakeNow)New<INow>()).TimeFunction = () =>
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

            long keyWrapIterations = new IterationCalculator().KeyWrapIterations(new V1Aes128CryptoFactory().CryptoId);

            Assert.That(keyWrapIterations, Is.EqualTo(5000), "The minimum guarantee should hold.");
        }

        [Test]
        public void TestMinimumGuaranteeV2KeyWrapIterations()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            ((FakeNow)New<INow>()).TimeFunction = () =>
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

            long keyWrapIterations = new IterationCalculator().KeyWrapIterations(new V2Aes256CryptoFactory().CryptoId);

            Assert.That(keyWrapIterations, Is.EqualTo(5000), "The minimum guarantee should hold.");
        }

        [Test]
        public void TestCalculatedV1KeyWrapIterations()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            ((FakeNow)New<INow>()).TimeFunction = () =>
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

            long keyWrapIterations = new IterationCalculator().KeyWrapIterations(new V1Aes128CryptoFactory().CryptoId);

            Assert.That(keyWrapIterations, Is.EqualTo(12500), "If we do 125000 iterations in 500ms, the result should be 12500 as default iterations (1/20s).");
        }

        [Test]
        public void TestCalculatedV2KeyWrapIterations()
        {
            DateTime now = DateTime.UtcNow;
            int callCounter = -1;
            bool shouldTerminate = false;
            ((FakeNow)New<INow>()).TimeFunction = () =>
            {
                if (shouldTerminate)
                {
                    throw new InvalidOperationException("There should be no more calls at this point.");
                }
                if (callCounter++ == 0)
                {
                    return now;
                }
                // Reach 500 ms after 125 calls.
                return now.AddMilliseconds(callCounter * 4);
            };

            long keyWrapIterations = new IterationCalculator().KeyWrapIterations(new V2Aes256CryptoFactory().CryptoId);

            Assert.That(keyWrapIterations, Is.EqualTo(12500), "If we do 125000 iterations in 500ms, the result should be 12500 as default iterations.");
        }
    }
}
