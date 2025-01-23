#region Xecrets Cli Copyright and GPL License notice

/*
 * Xecrets Cli - Changes and additions Copyright © 2022-2025, Svante Seleborg, All Rights Reserved.
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
using AxCrypt.Core.Runtime;
using NUnit.Framework;
using System;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public static class TestExceptions
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(Fake.CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        public static void TestAxCryptExceptions()
        {
            Assert.Throws<FileFormatException>(() =>
            {
                throw new FileFormatException();
            });
            try
            {
                throw new FileFormatException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }

            Assert.Throws<InternalErrorException>(() =>
            {
                throw new InternalErrorException();
            });
            try
            {
                throw new InternalErrorException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }

            Assert.Throws<AxCrypt.Core.Runtime.IncorrectDataException>(() =>
            {
                throw new AxCrypt.Core.Runtime.IncorrectDataException();
            });
            try
            {
                throw new AxCrypt.Core.Runtime.IncorrectDataException();
            }
            catch (AxCryptException ace)
            {
                Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.Unknown), "Parameterless constructor should result in status Unknown.");
            }
        }

        [Test]
        public static void TestInnerException()
        {
            try
            {
                int i = (int)new object();

                // Use the instance to avoid FxCop errors.
                Object.Equals(i, null);
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new FileFormatException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.FileFormatError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }

            try
            {
                if ((int)new object() == 0) { }
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new InternalErrorException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.InternalError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }

            try
            {
                if ((int)new object() == 0) { }
            }
            catch (InvalidCastException ice)
            {
                try
                {
                    throw new AxCrypt.Core.Runtime.IncorrectDataException("Testing inner", ice);
                }
                catch (AxCryptException ace)
                {
                    Assert.That(ace.ErrorStatus, Is.EqualTo(ErrorStatus.DataError), "Wrong status.");
                    Assert.That(ace.Message, Is.EqualTo("Testing inner"), "Wrong message.");
                    Assert.That(ace.InnerException.GetType(), Is.EqualTo(typeof(InvalidCastException)), "Wrong inner exception.");
                }
            }
        }
    }
}
