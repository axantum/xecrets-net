﻿#region Xecrets Cli Copyright and GPL License notice

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

using AxCrypt.Core.Crypto;
using AxCrypt.Core.Header;
using AxCrypt.Core.IO;
using AxCrypt.Core.Reader;
using AxCrypt.Core.Runtime;
using Xecrets.Net.Core.Test.Properties;
using AxCrypt.Fake;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AxCrypt.Core.Test
{
    [TestFixture]
    public class TestAxCryptReaderPreambleHeaderBlock
    {
        [SetUp]
        public static void Setup()
        {
            SetupAssembly.AssemblySetup(CryptoImplementation.WindowsDesktop);
        }

        [TearDown]
        public static void Teardown()
        {
            SetupAssembly.AssemblyTeardown();
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestFindPreambleHeaderBlockFirstButMoreThanOnceShouldThrow()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(testStream);
                preambleHeaderBlock.Write(testStream);
                testStream.Position = 0;
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(testStream)))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the next HeaderBlock");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.HeaderBlock), "We're expecting to have found a HeaderBlock");
                    Assert.That(axCryptReader.CurrentHeaderBlock.HeaderBlockType, Is.EqualTo(HeaderBlockType.Preamble), "We're expecting to have found a Preamble specifically");
                    Assert.Throws<FileFormatException>(() => axCryptReader.Read());
                }
            }
        }

        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is test, readability and coding ease is a concern, not performance.")]
        public void TestFindPreambleHeaderBlockNotFirstShouldThrow()
        {
            using (MemoryStream testStream = new MemoryStream())
            {
                AxCrypt1Guid.Write(testStream);
                VersionHeaderBlock versionHeaderBlock = new VersionHeaderBlock(new byte[] { 3, 2, 2, 0, 0 });
                versionHeaderBlock.Write(testStream);
                PreambleHeaderBlock preambleHeaderBlock = new PreambleHeaderBlock();
                preambleHeaderBlock.Write(testStream);
                testStream.Position = 0;
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(testStream)))
                {
                    Assert.That(axCryptReader.Read(), Is.True, "We should be able to read the Guid");
                    Assert.That(axCryptReader.CurrentItemType, Is.EqualTo(AxCryptItemType.MagicGuid), "We're expecting to have found a MagicGuid");
                    Assert.Throws<FileFormatException>(() => axCryptReader.Read());
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times"), Test]
        public static void TestFindPreambleHeaderBlockFromSimpleFile()
        {
            using (Stream testStream = FakeDataStore.ExpandableMemoryStream(Resources.helloworld_key_a_txt))
            {
                using (V1AxCryptReader axCryptReader = new V1AxCryptReader(new LookAheadStream(testStream)))
                {
                    bool blockFound = false;
                    int headers = 0;
                    while (axCryptReader.Read())
                    {
                        switch (axCryptReader.CurrentItemType)
                        {
                            case AxCryptItemType.None:
                                break;

                            case AxCryptItemType.MagicGuid:
                                break;

                            case AxCryptItemType.HeaderBlock:
                                if (axCryptReader.CurrentHeaderBlock.HeaderBlockType == HeaderBlockType.Preamble)
                                {
                                    Assert.That(blockFound, Is.False, "We should only find one single PreambleHeaderBlock");
                                    blockFound = true;

                                    Assert.That(headers, Is.EqualTo(0), "Preamble must be first");
                                    PreambleHeaderBlock preambleHeaderBlock = (PreambleHeaderBlock)axCryptReader.CurrentHeaderBlock;
                                    Assert.That(preambleHeaderBlock.Hmac.Length, Is.EqualTo(V1Hmac.RequiredLength), "The HMAC in the preamble must be exactly 16 bytes.");
                                }
                                ++headers;
                                break;

                            case AxCryptItemType.Data:
                                break;

                            case AxCryptItemType.EndOfStream:
                                break;

                            default:
                                break;
                        }
                    }
                    Assert.That(blockFound, Is.True, "We're expecting a VersionHeaderBlock to be found!");
                }
            }
        }
    }
}
